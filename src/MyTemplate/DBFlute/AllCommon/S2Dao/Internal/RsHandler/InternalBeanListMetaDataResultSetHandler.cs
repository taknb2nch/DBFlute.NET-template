
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Seasar.Extension.ADO;
using Seasar.Dao;
using Seasar.Dao.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.RsHandler {

    public class InternalBeanListMetaDataResultSetHandler : InternalAbstractBeanMetaDataResultSetHandler {

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalBeanListMetaDataResultSetHandler(IBeanMetaData beanMetaData, IRowCreator rowCreator, IRelationRowCreator relationRowCreator)
            : base(beanMetaData, rowCreator, relationRowCreator) {
        }

        // ===============================================================================
        //                                                                          Handle
        //                                                                          ======
        public override Object Handle(IDataReader dataReader) {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            Handle(dataReader, list);
            return list;
        }

        protected void Handle(IDataReader dataReader, System.Collections.IList list) {
            // Lazy initialization because if the result is zero, the resources are unused.
            System.Collections.IList columnNames = null;
            IColumnMetaData[] columns = null;
            IDictionary<String, IDictionary<String, IPropertyType>> relationPropertyCache = null;
            RelationRowCache relRowCache = null;

            int relSize = BeanMetaData.RelationPropertyTypeSize;
            bool hasCB = HasConditionBean();
            bool skipRelationLoop;
            {
                bool emptyRelation = IsSelectedForeignInfoEmpty();
                bool hasOSC = HasOutsideSqlContext();
                bool specifiedOutsideSql = IsSpecifiedOutsideSql();

                // If it has condition-bean that has no relation to get
                // or it has outside-sql context that is specified-outside-sql,
                // they are unnecessary to do relation loop!
                skipRelationLoop = (hasCB && emptyRelation) || (hasOSC && specifiedOutsideSql);
            }
            bool canCache = hasCB && CanRelationMappingCache();
            Map<String, int?> selectIndexMap = GetSelectIndexMap();

            while (dataReader.Read()) {
                if (columnNames == null) {
                    columnNames = CreateColumnNames(dataReader.GetSchemaTable());
                }
                if (columns == null) {
                    columns = CreateColumnMetaData(columnNames);
                }

                // Create row instance of base table by row property cache.
                Object row = CreateRow(dataReader, columns);
                if (skipRelationLoop) {
                    PostCreateRow(row, BeanMetaData);
                    list.Add(row);
                    continue;
                }

                if (relationPropertyCache == null) {
                    relationPropertyCache = CreateRelationPropertyCache(columnNames);
                }
                if (relRowCache == null) {
                    relRowCache = new RelationRowCache(relSize);
                }
                for (int i = 0; i < relSize; ++i) {
                    IRelationPropertyType rpt = BeanMetaData.GetRelationPropertyType(i);
                    if (rpt == null) {
                        continue;
                    }

                    // Do only selected foreign property for performance if condition-bean exists.
                    if (hasCB && !HasSelectedForeignInfo(BuildRelationNoSuffix(rpt))) {
                        continue;
                    }

                    Object relationRow = null;
                    System.Collections.Hashtable relKeyValues = new System.Collections.Hashtable();
                    RelationKey relKey = CreateRelationKey(dataReader, rpt, columnNames, relKeyValues, selectIndexMap);
                    if (relKey != null) {
                        relationRow = GetCachedRelationRow(relRowCache, i, relKey, canCache);
                        if (relationRow == null) {
                            relationRow = CreateRelationRow(dataReader, rpt, columnNames, relKeyValues, relationPropertyCache);
                            if (relationRow != null) {
                                AddRelationRowCache(relRowCache, i, relKey, relationRow, canCache);
                                PostCreateRow(relationRow, rpt.BeanMetaData);
                            }
                        }
                    }
                    if (relationRow != null) {
                        PropertyInfo pi = rpt.PropertyInfo;
                        pi.SetValue(row, relationRow, null);
                    }
                }
                PostCreateRow(row, BeanMetaData);
                list.Add(row);
            }
        }

        protected RelationKey CreateRelationKey(IDataReader reader, IRelationPropertyType rpt
                , System.Collections.IList columnNames, System.Collections.Hashtable relKeyValues
                , Map<String, int?> selectIndexMap) {
            System.Collections.ArrayList keyList = new System.Collections.ArrayList();
            IBeanMetaData bmd = rpt.BeanMetaData;
            for (int i = 0; i < rpt.KeySize; ++i) {
                IValueType valueType = null;
                IPropertyType pt = rpt.BeanMetaData.GetPropertyTypeByColumnName(rpt.GetYourKey(i));
                String relationNoSuffix = BuildRelationNoSuffix(rpt);
                String columnName = RemoveQuoteIfExists(pt.ColumnName) + relationNoSuffix;
                if (columnNames.Contains(columnName)) {
                    valueType = pt.ValueType;
                } else {
                    // basically unreachable
                    // because the referred column (basically PK or FK) must exist
                    // if the relation's select clause is specified
                    return null;
                }
                Object value;
                if (selectIndexMap != null) {
                    value = GetValue(reader, columnName, valueType, selectIndexMap);
                } else {
                    value = valueType.GetValue(reader, columnName);
                }
                if (value == null) {
                    // reachable when the referred column data is null
                    // (treated as no relation data)
                    return null;
                }
                relKeyValues[columnName] = value;
                keyList.Add(value);
            }
            if (keyList.Count > 0) {
                object[] keys = keyList.ToArray();
                return new RelationKey(keys);
            }
            else return null;
        }

        protected Object GetCachedRelationRow(RelationRowCache cache, int relno, RelationKey relKey, bool canCache) {
            return canCache ? cache.GetRelationRow(relno, relKey) : null;
        }

        protected void AddRelationRowCache(RelationRowCache cache, int relno, RelationKey relKey, Object relationRow, bool canCache) {
            if (canCache) {
                cache.AddRelationRow(relno, relKey, relationRow);
            }
        }

        protected static String RemoveQuoteIfExists(String name) {
            if (name.StartsWith("\"") && name.EndsWith("\"")) {
                name = name.Substring(1);
                name = name.Substring(0, name.Length - 1);
            } else if (name.StartsWith("[") && name.EndsWith("]")) {
                name = name.Substring(1);
                name = name.Substring(0, name.Length - 1);
            }
            return name;
        }

        // ===================================================================================
        //                                                                        Select Index
        //                                                                        ============
        protected Map<String, int?> GetSelectIndexMap() { // ResourceContext in Java
            if (!HasConditionBean()) {
                return null;
            }
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.getSelectIndexMap();
        }

        protected Object GetValue(IDataReader rs, String columnName, IValueType valueType,
                Map<String, int?> selectIndexMap) { // ResourceContext in Java
            // lower because of @see AbstractSqlClause
            int? selectIndex = selectIndexMap != null ? selectIndexMap.get(columnName.ToLower()) : null;
            if (selectIndex != null) {
                return valueType.GetValue(rs, selectIndex.Value);
            } else {
                return valueType.GetValue(rs, columnName);
            }
        }

        // ===================================================================================
        //                                                                       ConditionBean
        //                                                                       =============
        protected bool HasConditionBean() {
            return ConditionBeanContext.IsExistConditionBeanOnThread();
        }

        protected bool IsSelectedForeignInfoEmpty() {
            if (!HasConditionBean()) {
                return true;
            }
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.isSelectedForeignInfoEmpty();
        }

        // You should call HasConditionBean() before calling this!
        protected bool HasSelectedForeignInfo(String relationNoSuffix) {
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.hasSelectedForeignInfo(relationNoSuffix);
        }

        protected String BuildRelationNoSuffix(IRelationPropertyType rpt) {
            return "_" + rpt.RelationNo;
        }

        // You should call HasConditionBean() before calling this!
        protected bool CanRelationMappingCache() {
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.CanRelationMappingCache();
        }

        // ===================================================================================
        //                                                                          OutsideSql
        //                                                                          ==========
        protected bool HasOutsideSqlContext() {
            return OutsideSqlContext.IsExistOutsideSqlContextOnThread();
        }

        protected bool IsSpecifiedOutsideSql() {
            if (!HasOutsideSqlContext()) {
                return false;
            }
            OutsideSqlContext context = OutsideSqlContext.GetOutsideSqlContextOnThread();
            return context.IsSpecifiedOutsideSql;
        }
    }
}
