

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Helper;
using Aaa.Bbb.Ccc.DBFlute.ExEntity.Customize;
using Aaa.Bbb.Ccc.DBFlute.BsEntity.Customize.Dbm;


namespace Aaa.Bbb.Ccc.DBFlute.ExEntity.Customize {

    /// <summary>
    /// The entity of MyTableEx. (partial class for auto-generation)
    /// <![CDATA[
    /// [primary-key]
    ///     
    /// 
    /// [column]
    ///     id, user_name, age, attendance_flag, created_datetime, created_user, updated_datetime, updated_user, version_no
    /// 
    /// [sequence]
    ///     
    /// 
    /// [identity]
    ///     
    /// 
    /// [version-no]
    ///     version_no
    /// 
    /// [foreign-table]
    ///     
    /// 
    /// [referrer-table]
    ///     
    /// 
    /// [foreign-property]
    ///     
    /// 
    /// [referrer-property]
    ///     
    /// ]]>
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    [Seasar.Dao.Attrs.Table("MyTableEx")]
    [Seasar.Dao.Attrs.VersionNoProperty("VersionNo")]
    [System.Serializable]
    public partial class MyTableEx : Entity {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        #region Attribute
        /// <summary>id: {int4(10)}</summary>
        protected int? _id;

        /// <summary>user_name: {varchar(100)}</summary>
        protected String _userName;

        /// <summary>age: {int4(10)}</summary>
        protected int? _age;

        /// <summary>attendance_flag: {bpchar(1), classification=Flag}</summary>
        protected String _attendanceFlag;

        /// <summary>created_datetime: {timestamp(29, 6)}</summary>
        protected DateTime? _createdDatetime;

        /// <summary>created_user: {varchar(100)}</summary>
        protected String _createdUser;

        /// <summary>updated_datetime: {timestamp(29, 6)}</summary>
        protected DateTime? _updatedDatetime;

        /// <summary>updated_user: {varchar(100)}</summary>
        protected String _updatedUser;

        /// <summary>version_no: {int4(10)}</summary>
        protected int? _versionNo;

        protected EntityModifiedProperties __modifiedProperties = new EntityModifiedProperties();
        #endregion

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        public String TableDbName { get { return "MyTableEx"; } }
        public String TablePropertyName { get { return "MyTableEx"; } }

        // ===============================================================================
        //                                                                          DBMeta
        //                                                                          ======
        public DBMeta DBMeta { get { return MyTableExDbm.GetInstance(); } }

        // ===============================================================================
        //                                                         Classification Property
        //                                                         =======================
        #region Classification Property
        public CDef.Flag AttendanceFlagAsFlag { get {
            return CDef.Flag.CodeOf(_attendanceFlag);
        } set {
            AttendanceFlag = value != null ? value.Code : null;
        }}

        #endregion

        // ===============================================================================
        //                                                          Classification Setting
        //                                                          ======================
        #region Classification Setting
        /// <summary>
        /// Set the value of attendanceFlag as True.
        /// <![CDATA[
        /// はい: 有効を示す
        /// ]]>
        /// </summary>
        public void SetAttendanceFlag_True() {
            AttendanceFlagAsFlag = CDef.Flag.True;
        }

        /// <summary>
        /// Set the value of attendanceFlag as False.
        /// <![CDATA[
        /// いいえ: 無効を示す
        /// ]]>
        /// </summary>
        public void SetAttendanceFlag_False() {
            AttendanceFlagAsFlag = CDef.Flag.False;
        }

        #endregion

        // ===============================================================================
        //                                                    Classification Determination
        //                                                    ============================
        #region Classification Determination
        /// <summary>
        /// Is the value of attendanceFlag 'True'?
        /// <![CDATA[
        /// The difference of capital letters and small letters is NOT distinguished.
        /// If the value is null, this method returns false!
        /// はい: 有効を示す
        /// ]]>
        /// </summary>
        public bool IsAttendanceFlagTrue {
            get {
                CDef.Flag cls = AttendanceFlagAsFlag;
                return cls != null ? cls.Equals(CDef.Flag.True) : false;
            }
        }

        /// <summary>
        /// Is the value of attendanceFlag 'False'?
        /// <![CDATA[
        /// The difference of capital letters and small letters is NOT distinguished.
        /// If the value is null, this method returns false!
        /// いいえ: 無効を示す
        /// ]]>
        /// </summary>
        public bool IsAttendanceFlagFalse {
            get {
                CDef.Flag cls = AttendanceFlagAsFlag;
                return cls != null ? cls.Equals(CDef.Flag.False) : false;
            }
        }

        #endregion

        // ===============================================================================
        //                                                       Classification Name/Alias
        //                                                       =========================
        #region Classification Name/Alias
        public String AttendanceFlagName {
            get {
                CDef.Flag cls = AttendanceFlagAsFlag;
                return cls != null ? cls.Name : null;
            }
        }
        public String AttendanceFlagAlias {
            get {
                CDef.Flag cls = AttendanceFlagAsFlag;
                return cls != null ? cls.Alias : null;
            }
        }

        #endregion

        // ===============================================================================
        //                                                                Foreign Property
        //                                                                ================
        #region Foreign Property
        #endregion

        // ===============================================================================
        //                                                               Referrer Property
        //                                                               =================
        #region Referrer Property
        #endregion

        // ===============================================================================
        //                                                                   Determination
        //                                                                   =============
        public virtual bool HasPrimaryKeyValue {
            get {
                return false;
            }
        }

        // ===============================================================================
        //                                                             Modified Properties
        //                                                             ===================
        public virtual IDictionary<String, Object> ModifiedPropertyNames {
            get { return __modifiedProperties.PropertyNames; }
        }

        public virtual void ClearModifiedPropertyNames() {
            __modifiedProperties.Clear();
        }

        // ===============================================================================
        //                                                                  Basic Override
        //                                                                  ==============
        #region Basic Override
        public override bool Equals(Object other) {
            if (other == null || !(other is MyTableEx)) { return false; }
            MyTableEx otherEntity = (MyTableEx)other;
            if (!xSV(this.Id, otherEntity.Id)) { return false; }
            if (!xSV(this.UserName, otherEntity.UserName)) { return false; }
            if (!xSV(this.Age, otherEntity.Age)) { return false; }
            if (!xSV(this.AttendanceFlag, otherEntity.AttendanceFlag)) { return false; }
            if (!xSV(this.CreatedDatetime, otherEntity.CreatedDatetime)) { return false; }
            if (!xSV(this.CreatedUser, otherEntity.CreatedUser)) { return false; }
            if (!xSV(this.UpdatedDatetime, otherEntity.UpdatedDatetime)) { return false; }
            if (!xSV(this.UpdatedUser, otherEntity.UpdatedUser)) { return false; }
            if (!xSV(this.VersionNo, otherEntity.VersionNo)) { return false; }
            return true;
        }
        protected bool xSV(Object value1, Object value2) { // isSameValue()
            if (value1 == null && value2 == null) { return true; }
            if (value1 == null || value2 == null) { return false; }
            return value1.Equals(value2);
        }

        public override int GetHashCode() {
            int result = 17;
            result = xCH(result, _id);
            result = xCH(result, _userName);
            result = xCH(result, _age);
            result = xCH(result, _attendanceFlag);
            result = xCH(result, _createdDatetime);
            result = xCH(result, _createdUser);
            result = xCH(result, _updatedDatetime);
            result = xCH(result, _updatedUser);
            result = xCH(result, _versionNo);
            return result;
        }
        protected int xCH(int result, Object value) { // calculateHashcode()
            if (value == null) { return result; }
            return (31*result) + (value is byte[] ? ((byte[])value).Length : value.GetHashCode());
        }

        public override String ToString() {
            return "MyTableEx:" + BuildColumnString() + BuildRelationString();
        }

        public virtual String ToStringWithRelation() {
            StringBuilder sb = new StringBuilder();
            sb.Append(ToString());
            return sb.ToString();
        }

        public virtual String BuildDisplayString(String name, bool column, bool relation) {
            StringBuilder sb = new StringBuilder();
            if (name != null) { sb.Append(name).Append(column || relation ? ":" : ""); }
            if (column) { sb.Append(BuildColumnString()); }
            if (relation) { sb.Append(BuildRelationString()); }
            return sb.ToString();
        }
        protected virtual String BuildColumnString() {
            String c = ", ";
            StringBuilder sb = new StringBuilder();
            sb.Append(c).Append(this.Id);
            sb.Append(c).Append(this.UserName);
            sb.Append(c).Append(this.Age);
            sb.Append(c).Append(this.AttendanceFlag);
            sb.Append(c).Append(this.CreatedDatetime);
            sb.Append(c).Append(this.CreatedUser);
            sb.Append(c).Append(this.UpdatedDatetime);
            sb.Append(c).Append(this.UpdatedUser);
            sb.Append(c).Append(this.VersionNo);
            if (sb.Length > 0) { sb.Remove(0, c.Length); }
            sb.Insert(0, "{").Append("}");
            return sb.ToString();
        }
        protected virtual String BuildRelationString() {
            return "";
        }
        #endregion

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        #region Accessor
        /// <summary>id: {int4(10)}</summary>
        [Seasar.Dao.Attrs.Column("id")]
        public int? Id {
            get { return _id; }
            set {
                __modifiedProperties.AddPropertyName("Id");
                _id = value;
            }
        }

        /// <summary>user_name: {varchar(100)}</summary>
        [Seasar.Dao.Attrs.Column("user_name")]
        public String UserName {
            get { return _userName; }
            set {
                __modifiedProperties.AddPropertyName("UserName");
                _userName = value;
            }
        }

        /// <summary>age: {int4(10)}</summary>
        [Seasar.Dao.Attrs.Column("age")]
        public int? Age {
            get { return _age; }
            set {
                __modifiedProperties.AddPropertyName("Age");
                _age = value;
            }
        }

        /// <summary>attendance_flag: {bpchar(1), classification=Flag}</summary>
        [Seasar.Dao.Attrs.Column("attendance_flag")]
        public String AttendanceFlag {
            get { return _attendanceFlag; }
            set {
                __modifiedProperties.AddPropertyName("AttendanceFlag");
                _attendanceFlag = value;
            }
        }

        /// <summary>created_datetime: {timestamp(29, 6)}</summary>
        [Seasar.Dao.Attrs.Column("created_datetime")]
        public DateTime? CreatedDatetime {
            get { return _createdDatetime; }
            set {
                __modifiedProperties.AddPropertyName("CreatedDatetime");
                _createdDatetime = value;
            }
        }

        /// <summary>created_user: {varchar(100)}</summary>
        [Seasar.Dao.Attrs.Column("created_user")]
        public String CreatedUser {
            get { return _createdUser; }
            set {
                __modifiedProperties.AddPropertyName("CreatedUser");
                _createdUser = value;
            }
        }

        /// <summary>updated_datetime: {timestamp(29, 6)}</summary>
        [Seasar.Dao.Attrs.Column("updated_datetime")]
        public DateTime? UpdatedDatetime {
            get { return _updatedDatetime; }
            set {
                __modifiedProperties.AddPropertyName("UpdatedDatetime");
                _updatedDatetime = value;
            }
        }

        /// <summary>updated_user: {varchar(100)}</summary>
        [Seasar.Dao.Attrs.Column("updated_user")]
        public String UpdatedUser {
            get { return _updatedUser; }
            set {
                __modifiedProperties.AddPropertyName("UpdatedUser");
                _updatedUser = value;
            }
        }

        /// <summary>version_no: {int4(10)}</summary>
        [Seasar.Dao.Attrs.Column("version_no")]
        public int? VersionNo {
            get { return _versionNo; }
            set {
                __modifiedProperties.AddPropertyName("VersionNo");
                _versionNo = value;
            }
        }

        #endregion
    }
}
