
using System;
using System.Collections.Generic;
using System.Data;

using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Dao;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.RsHandler {

    public abstract class InternalAbstractBeanMetaDataResultSetHandler : IDataReaderHandler {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        private readonly IBeanMetaData _beanMetaData;
        private string[] _clearModifiedOnlyPropertyNamePrefixes = new string[] { "Clear" }; // [DAONET-57] 2007/10/02
        protected IRowCreator _rowCreator; // [DAONET-56] (2007/08/29)
        protected IRelationRowCreator _relationRowCreator; // [DAONET-56] (2007/08/29)

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalAbstractBeanMetaDataResultSetHandler(IBeanMetaData beanMetaData, IRowCreator rowCreator, IRelationRowCreator relationRowCreator) {
            _beanMetaData = beanMetaData;
            _rowCreator = rowCreator;
            _relationRowCreator = relationRowCreator;
        }

        // ===============================================================================
        //                                                                          Handle
        //                                                                          ======
        public virtual Object Handle(IDataReader dataReader) {
            return null;
        }

        // ===============================================================================
        //                                                                   Assist Helper
        //                                                                   =============
        protected virtual IColumnMetaData[] CreateColumnMetaData(System.Collections.IList columnNames) {
            return _rowCreator.CreateColumnMetaData(columnNames, _beanMetaData);
        }

        protected virtual Object CreateRow(IDataReader reader, IColumnMetaData[] columns) {
            return _rowCreator.CreateRow(reader, columns, _beanMetaData.BeanType);
        }

        protected virtual IDictionary<string, IDictionary<string, IPropertyType>> CreateRelationPropertyCache(System.Collections.IList columnNames) {
            return _relationRowCreator.CreateRelationPropertyCache(columnNames, _beanMetaData);
        }

        protected virtual Object CreateRelationRow(IDataReader reader, IRelationPropertyType rpt,
            System.Collections.IList columnNames, System.Collections.Hashtable relKeyValues,
            IDictionary<String, IDictionary<String, IPropertyType>> relationColumnMetaDataCache) {
            return _relationRowCreator.CreateRelationRow(reader, rpt, columnNames, relKeyValues, relationColumnMetaDataCache);
        }

        protected virtual bool IsTargetProperty(IPropertyType pt) { // [DAONET-56] (2007/08/29)
            return pt.PropertyInfo.CanWrite;
        }

        protected virtual Object CreateRelationRow(IRelationPropertyType rpt) {
            return ClassUtil.NewInstance(rpt.PropertyInfo.PropertyType);
        }

        protected virtual System.Collections.IList CreateColumnNames(DataTable dt) { // ResourceContext in Java
            System.Collections.IList columnSet = new CaseInsentiveSet();
            foreach (DataRow row in dt.Rows) {
                string columnName = (string) row["ColumnName"];
                columnSet.Add(columnName);
            }
            Map<String, String> selectIndexReverseMap = GetSelectIndexReverseMap();
            if (selectIndexReverseMap == null) {
                return columnSet;
            }
            System.Collections.IList realColumnSet = new CaseInsentiveSet();
            foreach (String columnName in columnSet) {
                String realColumnName = selectIndexReverseMap.get(columnName.ToLower()); // lower because of @see AbstractSqlClause
                if (realColumnName != null) { // mainly true
                    realColumnSet.Add(realColumnName);
                } else { // for derived columns and so on
                    realColumnSet.Add(columnName);
                }
            }
            return realColumnSet;
        }

        protected Map<String, String> GetSelectIndexReverseMap() { // ResourceContext in Java
            if (!ConditionBeanContext.IsExistConditionBeanOnThread()) {
                return null;
            }
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.getSelectIndexReverseMap();
        }

        protected virtual void PostCreateRow(Object row, IBeanMetaData bmd) {
            if (row is Entity) { // DBFlute Target
                ((Entity)row).ClearModifiedPropertyNames();
            } else { // Basically Unreachable
                bmd.ClearModifiedPropertyNames(row);
            }
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IBeanMetaData BeanMetaData {
            get { return _beanMetaData; }
        }
    }
}
