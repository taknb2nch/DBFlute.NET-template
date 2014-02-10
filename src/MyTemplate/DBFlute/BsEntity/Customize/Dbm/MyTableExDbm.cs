
using System;
using System.Reflection;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm.Info;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.ExEntity.Customize;
namespace Aaa.Bbb.Ccc.DBFlute.BsEntity.Customize.Dbm {

    public class MyTableExDbm : AbstractDBMeta {

        public static readonly Type ENTITY_TYPE = typeof(MyTableEx);

        private static readonly MyTableExDbm _instance = new MyTableExDbm();
        private MyTableExDbm() {
            InitializeColumnInfo();
            InitializeColumnInfoList();
            InitializeEntityPropertySetupper();
        }
        public static MyTableExDbm GetInstance() {
            return _instance;
        }

        // ===============================================================================
        //                                                                      Table Info
        //                                                                      ==========
        public override String TableDbName { get { return "MyTableEx"; } }
        public override String TablePropertyName { get { return "MyTableEx"; } }
        public override String TableSqlName { get { return "MyTableEx"; } }

        // ===============================================================================
        //                                                                     Column Info
        //                                                                     ===========
        protected ColumnInfo _columnId;
        protected ColumnInfo _columnUserName;
        protected ColumnInfo _columnAge;
        protected ColumnInfo _columnAttendanceFlag;
        protected ColumnInfo _columnCreatedDatetime;
        protected ColumnInfo _columnCreatedUser;
        protected ColumnInfo _columnUpdatedDatetime;
        protected ColumnInfo _columnUpdatedUser;
        protected ColumnInfo _columnVersionNo;

        public ColumnInfo ColumnId { get { return _columnId; } }
        public ColumnInfo ColumnUserName { get { return _columnUserName; } }
        public ColumnInfo ColumnAge { get { return _columnAge; } }
        public ColumnInfo ColumnAttendanceFlag { get { return _columnAttendanceFlag; } }
        public ColumnInfo ColumnCreatedDatetime { get { return _columnCreatedDatetime; } }
        public ColumnInfo ColumnCreatedUser { get { return _columnCreatedUser; } }
        public ColumnInfo ColumnUpdatedDatetime { get { return _columnUpdatedDatetime; } }
        public ColumnInfo ColumnUpdatedUser { get { return _columnUpdatedUser; } }
        public ColumnInfo ColumnVersionNo { get { return _columnVersionNo; } }

        protected void InitializeColumnInfo() {
            _columnId = cci("id", "id", null, null, false, "Id", typeof(int?), false, "int4", 10, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnUserName = cci("user_name", "user_name", null, null, false, "UserName", typeof(String), false, "varchar", 100, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnAge = cci("age", "age", null, null, false, "Age", typeof(int?), false, "int4", 10, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnAttendanceFlag = cci("attendance_flag", "attendance_flag", null, null, false, "AttendanceFlag", typeof(String), false, "bpchar", 1, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnCreatedDatetime = cci("created_datetime", "created_datetime", null, null, false, "CreatedDatetime", typeof(DateTime?), false, "timestamp", 29, 6, false, OptimisticLockType.NONE, null, null, null);
            _columnCreatedUser = cci("created_user", "created_user", null, null, false, "CreatedUser", typeof(String), false, "varchar", 100, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnUpdatedDatetime = cci("updated_datetime", "updated_datetime", null, null, false, "UpdatedDatetime", typeof(DateTime?), false, "timestamp", 29, 6, false, OptimisticLockType.NONE, null, null, null);
            _columnUpdatedUser = cci("updated_user", "updated_user", null, null, false, "UpdatedUser", typeof(String), false, "varchar", 100, 0, false, OptimisticLockType.NONE, null, null, null);
            _columnVersionNo = cci("version_no", "version_no", null, null, false, "VersionNo", typeof(int?), false, "int4", 10, 0, false, OptimisticLockType.VERSION_NO, null, null, null);
        }

        protected void InitializeColumnInfoList() {
            _columnInfoList = new ArrayList<ColumnInfo>();
            _columnInfoList.add(ColumnId);
            _columnInfoList.add(ColumnUserName);
            _columnInfoList.add(ColumnAge);
            _columnInfoList.add(ColumnAttendanceFlag);
            _columnInfoList.add(ColumnCreatedDatetime);
            _columnInfoList.add(ColumnCreatedUser);
            _columnInfoList.add(ColumnUpdatedDatetime);
            _columnInfoList.add(ColumnUpdatedUser);
            _columnInfoList.add(ColumnVersionNo);
        }

        // ===============================================================================
        //                                                                     Unique Info
        //                                                                     ===========
        public override UniqueInfo PrimaryUniqueInfo { get {
            throw new NotSupportedException("The table does not have primary key: " + TableDbName);
        }}

        // -------------------------------------------------
        //                                   Primary Element
        //                                   ---------------
        public override bool HasPrimaryKey { get { return false; } }
        public override bool HasCompoundPrimaryKey { get { return false; } }

        // ===============================================================================
        //                                                                   Relation Info
        //                                                                   =============
        // -------------------------------------------------
        //                                   Foreign Element
        //                                   ---------------


        // -------------------------------------------------
        //                                  Referrer Element
        //                                  ----------------

        // ===============================================================================
        //                                                                    Various Info
        //                                                                    ============
        public override bool HasVersionNo { get { return true; } }
        public override ColumnInfo VersionNoColumnInfo { get { return _columnVersionNo; } }
        public override bool HasCommonColumn { get { return false; } }

        // ===============================================================================
        //                                                                 Name Definition
        //                                                                 ===============
        #region Name

        // -------------------------------------------------
        //                                             Table
        //                                             -----
        public static readonly String TABLE_DB_NAME = "MyTableEx";
        public static readonly String TABLE_PROPERTY_NAME = "MyTableEx";

        // -------------------------------------------------
        //                                    Column DB-Name
        //                                    --------------
        public static readonly String DB_NAME_id = "id";
        public static readonly String DB_NAME_user_name = "user_name";
        public static readonly String DB_NAME_age = "age";
        public static readonly String DB_NAME_attendance_flag = "attendance_flag";
        public static readonly String DB_NAME_created_datetime = "created_datetime";
        public static readonly String DB_NAME_created_user = "created_user";
        public static readonly String DB_NAME_updated_datetime = "updated_datetime";
        public static readonly String DB_NAME_updated_user = "updated_user";
        public static readonly String DB_NAME_version_no = "version_no";

        // -------------------------------------------------
        //                              Column Property-Name
        //                              --------------------
        public static readonly String PROPERTY_NAME_id = "Id";
        public static readonly String PROPERTY_NAME_user_name = "UserName";
        public static readonly String PROPERTY_NAME_age = "Age";
        public static readonly String PROPERTY_NAME_attendance_flag = "AttendanceFlag";
        public static readonly String PROPERTY_NAME_created_datetime = "CreatedDatetime";
        public static readonly String PROPERTY_NAME_created_user = "CreatedUser";
        public static readonly String PROPERTY_NAME_updated_datetime = "UpdatedDatetime";
        public static readonly String PROPERTY_NAME_updated_user = "UpdatedUser";
        public static readonly String PROPERTY_NAME_version_no = "VersionNo";

        // -------------------------------------------------
        //                                      Foreign Name
        //                                      ------------
        // -------------------------------------------------
        //                                     Referrer Name
        //                                     -------------

        // -------------------------------------------------
        //                               DB-Property Mapping
        //                               -------------------
        protected static readonly Map<String, String> _dbNamePropertyNameKeyToLowerMap;
        protected static readonly Map<String, String> _propertyNameDbNameKeyToLowerMap;

        static MyTableExDbm() {
            {
                Map<String, String> map = new LinkedHashMap<String, String>();
                map.put(TABLE_DB_NAME.ToLower(), TABLE_PROPERTY_NAME);
                map.put(DB_NAME_id.ToLower(), PROPERTY_NAME_id);
                map.put(DB_NAME_user_name.ToLower(), PROPERTY_NAME_user_name);
                map.put(DB_NAME_age.ToLower(), PROPERTY_NAME_age);
                map.put(DB_NAME_attendance_flag.ToLower(), PROPERTY_NAME_attendance_flag);
                map.put(DB_NAME_created_datetime.ToLower(), PROPERTY_NAME_created_datetime);
                map.put(DB_NAME_created_user.ToLower(), PROPERTY_NAME_created_user);
                map.put(DB_NAME_updated_datetime.ToLower(), PROPERTY_NAME_updated_datetime);
                map.put(DB_NAME_updated_user.ToLower(), PROPERTY_NAME_updated_user);
                map.put(DB_NAME_version_no.ToLower(), PROPERTY_NAME_version_no);
                _dbNamePropertyNameKeyToLowerMap = map;
            }

            {
                Map<String, String> map = new LinkedHashMap<String, String>();
                map.put(TABLE_PROPERTY_NAME.ToLower(), TABLE_DB_NAME);
                map.put(PROPERTY_NAME_id.ToLower(), DB_NAME_id);
                map.put(PROPERTY_NAME_user_name.ToLower(), DB_NAME_user_name);
                map.put(PROPERTY_NAME_age.ToLower(), DB_NAME_age);
                map.put(PROPERTY_NAME_attendance_flag.ToLower(), DB_NAME_attendance_flag);
                map.put(PROPERTY_NAME_created_datetime.ToLower(), DB_NAME_created_datetime);
                map.put(PROPERTY_NAME_created_user.ToLower(), DB_NAME_created_user);
                map.put(PROPERTY_NAME_updated_datetime.ToLower(), DB_NAME_updated_datetime);
                map.put(PROPERTY_NAME_updated_user.ToLower(), DB_NAME_updated_user);
                map.put(PROPERTY_NAME_version_no.ToLower(), DB_NAME_version_no);
                _propertyNameDbNameKeyToLowerMap = map;
            }
        }

        #endregion

        // ===============================================================================
        //                                                                        Name Map
        //                                                                        ========
        #region Name Map
        public override Map<String, String> DbNamePropertyNameKeyToLowerMap { get { return _dbNamePropertyNameKeyToLowerMap; } }
        public override Map<String, String> PropertyNameDbNameKeyToLowerMap { get { return _propertyNameDbNameKeyToLowerMap; } }
        #endregion

        // ===============================================================================
        //                                                                       Type Name
        //                                                                       =========
        public override String EntityTypeName { get { return "Aaa.Bbb.Ccc.DBFlute.ExEntity.Customize.MyTableEx"; } }
        public override String DaoTypeName { get { return null; } }
        public override String ConditionBeanTypeName { get { return null; } }
        public override String BehaviorTypeName { get { return null; } }

        // ===============================================================================
        //                                                                     Object Type
        //                                                                     ===========
        public override Type EntityType { get { return ENTITY_TYPE; } }

        // ===============================================================================
        //                                                                 Object Instance
        //                                                                 ===============
        public override Entity NewEntity() { return NewMyEntity(); }
        public MyTableEx NewMyEntity() { return new MyTableEx(); }
        public override ConditionBean NewConditionBean() {
            String msg = "The entity does not have condition-bean. So this method is invalid.";
            throw new SystemException(msg + " dbmeta=" + ToString());
        }

        // ===============================================================================
        //                                                           Entity Property Setup
        //                                                           =====================
        protected Map<String, EntityPropertySetupper<MyTableEx>> _entityPropertySetupperMap = new LinkedHashMap<String, EntityPropertySetupper<MyTableEx>>();

        protected void InitializeEntityPropertySetupper() {
            RegisterEntityPropertySetupper("id", "Id", new EntityPropertyIdSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("user_name", "UserName", new EntityPropertyUserNameSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("age", "Age", new EntityPropertyAgeSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("attendance_flag", "AttendanceFlag", new EntityPropertyAttendanceFlagSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("created_datetime", "CreatedDatetime", new EntityPropertyCreatedDatetimeSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("created_user", "CreatedUser", new EntityPropertyCreatedUserSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("updated_datetime", "UpdatedDatetime", new EntityPropertyUpdatedDatetimeSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("updated_user", "UpdatedUser", new EntityPropertyUpdatedUserSetupper(), _entityPropertySetupperMap);
            RegisterEntityPropertySetupper("version_no", "VersionNo", new EntityPropertyVersionNoSetupper(), _entityPropertySetupperMap);
        }

        public override bool HasEntityPropertySetupper(String propertyName) {
            return _entityPropertySetupperMap.containsKey(propertyName);
        }

        public override void SetupEntityProperty(String propertyName, Object entity, Object value) {
            EntityPropertySetupper<MyTableEx> callback = _entityPropertySetupperMap.get(propertyName);
            callback.Setup((MyTableEx)entity, value);
        }

        public class EntityPropertyIdSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.Id = (value != null) ? (int?)value : null; }
        }
        public class EntityPropertyUserNameSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.UserName = (value != null) ? (String)value : null; }
        }
        public class EntityPropertyAgeSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.Age = (value != null) ? (int?)value : null; }
        }
        public class EntityPropertyAttendanceFlagSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.AttendanceFlag = (value != null) ? (String)value : null; }
        }
        public class EntityPropertyCreatedDatetimeSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.CreatedDatetime = (value != null) ? (DateTime?)value : null; }
        }
        public class EntityPropertyCreatedUserSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.CreatedUser = (value != null) ? (String)value : null; }
        }
        public class EntityPropertyUpdatedDatetimeSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.UpdatedDatetime = (value != null) ? (DateTime?)value : null; }
        }
        public class EntityPropertyUpdatedUserSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.UpdatedUser = (value != null) ? (String)value : null; }
        }
        public class EntityPropertyVersionNoSetupper : EntityPropertySetupper<MyTableEx> {
            public void Setup(MyTableEx entity, Object value) { entity.VersionNo = (value != null) ? (int?)value : null; }
        }
    }
}
