
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ.Ciq;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.CQ.BS {

    [System.Serializable]
    public class BsMyTableCQ : AbstractBsMyTableCQ {

        protected MyTableCIQ _inlineQuery;

        public BsMyTableCQ(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel)
            : base(childQuery, sqlClause, aliasName, nestLevel) {}

        public MyTableCIQ Inline() {
            if (_inlineQuery == null) {
                _inlineQuery = new MyTableCIQ(xgetReferrerQuery(), xgetSqlClause(), xgetAliasName(), xgetNestLevel(), this);
            }
            _inlineQuery.xsetOnClause(false);
            return _inlineQuery;
        }
        
        public MyTableCIQ On() {
            if (isBaseQuery()) { throw new UnsupportedOperationException("Unsupported onClause of Base Table!"); }
            MyTableCIQ inlineQuery = Inline();
            inlineQuery.xsetOnClause(true);
            return inlineQuery;
        }


        protected ConditionValue _id;
        public ConditionValue Id {
            get { if (_id == null) { _id = new ConditionValue(); } return _id; }
        }
        protected override ConditionValue getCValueId() { return this.Id; }


        public BsMyTableCQ AddOrderBy_Id_Asc() { regOBA("id");return this; }
        public BsMyTableCQ AddOrderBy_Id_Desc() { regOBD("id");return this; }

        protected ConditionValue _userName;
        public ConditionValue UserName {
            get { if (_userName == null) { _userName = new ConditionValue(); } return _userName; }
        }
        protected override ConditionValue getCValueUserName() { return this.UserName; }


        public BsMyTableCQ AddOrderBy_UserName_Asc() { regOBA("user_name");return this; }
        public BsMyTableCQ AddOrderBy_UserName_Desc() { regOBD("user_name");return this; }

        protected ConditionValue _age;
        public ConditionValue Age {
            get { if (_age == null) { _age = new ConditionValue(); } return _age; }
        }
        protected override ConditionValue getCValueAge() { return this.Age; }


        public BsMyTableCQ AddOrderBy_Age_Asc() { regOBA("age");return this; }
        public BsMyTableCQ AddOrderBy_Age_Desc() { regOBD("age");return this; }

        protected ConditionValue _attendanceFlag;
        public ConditionValue AttendanceFlag {
            get { if (_attendanceFlag == null) { _attendanceFlag = new ConditionValue(); } return _attendanceFlag; }
        }
        protected override ConditionValue getCValueAttendanceFlag() { return this.AttendanceFlag; }


        public BsMyTableCQ AddOrderBy_AttendanceFlag_Asc() { regOBA("attendance_flag");return this; }
        public BsMyTableCQ AddOrderBy_AttendanceFlag_Desc() { regOBD("attendance_flag");return this; }

        protected ConditionValue _createdDatetime;
        public ConditionValue CreatedDatetime {
            get { if (_createdDatetime == null) { _createdDatetime = new ConditionValue(); } return _createdDatetime; }
        }
        protected override ConditionValue getCValueCreatedDatetime() { return this.CreatedDatetime; }


        public BsMyTableCQ AddOrderBy_CreatedDatetime_Asc() { regOBA("created_datetime");return this; }
        public BsMyTableCQ AddOrderBy_CreatedDatetime_Desc() { regOBD("created_datetime");return this; }

        protected ConditionValue _createdUser;
        public ConditionValue CreatedUser {
            get { if (_createdUser == null) { _createdUser = new ConditionValue(); } return _createdUser; }
        }
        protected override ConditionValue getCValueCreatedUser() { return this.CreatedUser; }


        public BsMyTableCQ AddOrderBy_CreatedUser_Asc() { regOBA("created_user");return this; }
        public BsMyTableCQ AddOrderBy_CreatedUser_Desc() { regOBD("created_user");return this; }

        protected ConditionValue _updatedDatetime;
        public ConditionValue UpdatedDatetime {
            get { if (_updatedDatetime == null) { _updatedDatetime = new ConditionValue(); } return _updatedDatetime; }
        }
        protected override ConditionValue getCValueUpdatedDatetime() { return this.UpdatedDatetime; }


        public BsMyTableCQ AddOrderBy_UpdatedDatetime_Asc() { regOBA("updated_datetime");return this; }
        public BsMyTableCQ AddOrderBy_UpdatedDatetime_Desc() { regOBD("updated_datetime");return this; }

        protected ConditionValue _updatedUser;
        public ConditionValue UpdatedUser {
            get { if (_updatedUser == null) { _updatedUser = new ConditionValue(); } return _updatedUser; }
        }
        protected override ConditionValue getCValueUpdatedUser() { return this.UpdatedUser; }


        public BsMyTableCQ AddOrderBy_UpdatedUser_Asc() { regOBA("updated_user");return this; }
        public BsMyTableCQ AddOrderBy_UpdatedUser_Desc() { regOBD("updated_user");return this; }

        protected ConditionValue _versionNo;
        public ConditionValue VersionNo {
            get { if (_versionNo == null) { _versionNo = new ConditionValue(); } return _versionNo; }
        }
        protected override ConditionValue getCValueVersionNo() { return this.VersionNo; }


        public BsMyTableCQ AddOrderBy_VersionNo_Asc() { regOBA("version_no");return this; }
        public BsMyTableCQ AddOrderBy_VersionNo_Desc() { regOBD("version_no");return this; }

        public BsMyTableCQ AddSpecifiedDerivedOrderBy_Asc(String aliasName) { registerSpecifiedDerivedOrderBy_Asc(aliasName); return this; }
        public BsMyTableCQ AddSpecifiedDerivedOrderBy_Desc(String aliasName) { registerSpecifiedDerivedOrderBy_Desc(aliasName); return this; }

        public override void reflectRelationOnUnionQuery(ConditionQuery baseQueryAsSuper, ConditionQuery unionQueryAsSuper) {

        }
    


	    // ===============================================================================
	    //                                                                 Scalar SubQuery
	    //                                                                 ===============
	    protected Map<String, MyTableCQ> _scalarSubQueryMap;
	    public Map<String, MyTableCQ> ScalarSubQuery { get { return _scalarSubQueryMap; } }
	    public override String keepScalarSubQuery(MyTableCQ subQuery) {
	        if (_scalarSubQueryMap == null) { _scalarSubQueryMap = new LinkedHashMap<String, MyTableCQ>(); }
	        String key = "subQueryMapKey" + (_scalarSubQueryMap.size() + 1);
	        _scalarSubQueryMap.put(key, subQuery); return "ScalarSubQuery." + key;
	    }

        // ===============================================================================
        //                                                         Myself InScope SubQuery
        //                                                         =======================
        protected Map<String, MyTableCQ> _myselfInScopeSubQueryMap;
        public Map<String, MyTableCQ> MyselfInScopeSubQuery { get { return _myselfInScopeSubQueryMap; } }
        public override String keepMyselfInScopeSubQuery(MyTableCQ subQuery) {
            if (_myselfInScopeSubQueryMap == null) { _myselfInScopeSubQueryMap = new LinkedHashMap<String, MyTableCQ>(); }
            String key = "subQueryMapKey" + (_myselfInScopeSubQueryMap.size() + 1);
            _myselfInScopeSubQueryMap.put(key, subQuery); return "MyselfInScopeSubQuery." + key;
        }
    }
}
