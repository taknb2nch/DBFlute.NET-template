
using System;
using System.Reflection;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm.Info {

    public class ColumnInfo {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        protected static readonly List<String> EMPTY_LIST = new ArrayList<String>();

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected DBMeta dbmeta;
        protected String columnDbName;
        protected String columnSqlName;
        protected String columnSynonym;
        protected String columnAlias;
        protected bool notNull;
        protected String propertyName;
        protected Type propertyType;
        protected bool primary;
        protected String columnDbType;
        protected int? columnSize;
        protected int? columnDecimalDigits;
        protected bool commonColumn;
        protected OptimisticLockType optimisticLockType;
        protected String columnComment;
        protected List<String> foreignPropList;
        protected List<String> referrerPropList;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public ColumnInfo(DBMeta dbmeta, String columnDbName, String columnSqlName
                            , String columnSynonym, String columnAlias, bool notNull, String propertyName
                            , Type propertyType, bool primary, String columnDbType, int? columnSize
                            , int? columnDecimalDigits, bool commonColumn, OptimisticLockType optimisticLockType
                            , String columnComment, List<String> foreignPropList, List<String> referrerPropList) {
            AssertObjectNotNull("dbmeta", dbmeta);
            AssertObjectNotNull("columnDbName", columnDbName);
            AssertObjectNotNull("columnSqlName", columnSqlName);
            AssertObjectNotNull("propertyName", propertyName);
            AssertObjectNotNull("propertyType", propertyType);
            AssertObjectNotNull("optimisticLockType", optimisticLockType);
            this.dbmeta = dbmeta;
            this.columnDbName = columnDbName;
            this.columnSqlName = columnSqlName;
            this.columnSynonym = columnSynonym;
            this.columnAlias = columnAlias;
            this.notNull = notNull;
            this.propertyName = propertyName;
            this.propertyType = propertyType;
            this.primary = primary;
            this.columnDbType = columnDbType;
            this.columnSize = columnSize;
            this.columnDecimalDigits = columnDecimalDigits;
            this.commonColumn = commonColumn;
            this.optimisticLockType = optimisticLockType;
            this.columnComment = columnComment;
            this.foreignPropList = foreignPropList != null ? foreignPropList : EMPTY_LIST;
            this.referrerPropList = referrerPropList != null ? referrerPropList : EMPTY_LIST;
        }

        // ===============================================================================
        //                                                                          Finder
        //                                                                          ======
        public PropertyInfo FindProperty() {
            return FindProperty(dbmeta.EntityType, propertyName);
        }

        // ===============================================================================
        //                                                                 Internal Helper
        //                                                                 ===============
        protected virtual PropertyInfo FindProperty(Type clazz, String name) {
            return clazz.GetProperty(name);
        }

        // ===============================================================================
        //                                                                  General Helper
        //                                                                  ==============
        protected void AssertObjectNotNull(String variableName, Object value) {
            if (variableName == null) {
                String msg = "The value should not be null: variableName=" + variableName + " value=" + value;
                throw new ArgumentException(msg);
            }
            if (value == null) {
                String msg = "The value should not be null: variableName=" + variableName;
                throw new ArgumentException(msg);
            }
        }

        // ===============================================================================
        //                                                                  Basic Override
        //                                                                  ==============
        public override int GetHashCode() {
            return dbmeta.GetHashCode() + columnDbName.GetHashCode();
        }

        public override bool Equals(Object obj) {
            if (obj == null || !(obj is ColumnInfo)) {
                return false;
            }
            ColumnInfo target = (ColumnInfo)obj;
            if (!dbmeta.Equals(target.DBMeta)) {
                return false;
            }
            if (!columnDbName.Equals(target.ColumnDbName)) {
                return false;
            }
            return true;
        }

        public override String ToString() {
            return dbmeta.TableDbName + "." + columnDbName;
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public DBMeta DBMeta {
            get { return dbmeta; }
        }

        public String ColumnDbName {
            get { return columnDbName; }
        }

        public String ColumnSqlName {
            get { return columnSqlName; }
        }

        public String ColumnSynonym {
            get { return columnSynonym; }
        }

        public String ColumnAlias {
            get { return columnAlias; }
        }

        public bool IsNotNull {
            get { return notNull; }
        }

        public String PropertyName {
            get { return propertyName; }
        }

        public Type PropertyType {
            get { return propertyType; }
        }

        public bool IsPrimary {
            get { return primary; }
        }

        public String ColumnDbType {
            get { return columnDbType; }
        }

        public int? ColumnSize {
            get { return columnSize; }
        }

        public int? ColumnDecimalDigits {
            get { return columnDecimalDigits; }
        }

        public bool IsCommonColumn {
            get { return commonColumn; }
        }
        public bool IsOptimisticLock {
            get { return IsVersionNo || IsUpdateDate; }
        }

        public bool IsVersionNo {
            get { return OptimisticLockType.VERSION_NO == optimisticLockType; }
        }

        public bool IsUpdateDate {
            get { return OptimisticLockType.UPDATE_DATE == optimisticLockType; }
        }

        public List<ForeignInfo> ForeignInfoList { get {
            List<ForeignInfo> foreignInfoList = new ArrayList<ForeignInfo>();
            foreach (String foreignProp in foreignPropList) {
                foreignInfoList.add(this.DBMeta.FindForeignInfo(foreignProp));
            }
            return foreignInfoList;
        }}

        public List<ReferrerInfo> ReferrerInfoList { get {
            List<ReferrerInfo> referrerInfoList = new ArrayList<ReferrerInfo>();
            foreach (String referrerProp in referrerPropList) {
                referrerInfoList.add(this.DBMeta.FindReferrerInfo(referrerProp));
            }
            return referrerInfoList;
        }}
    }
}
