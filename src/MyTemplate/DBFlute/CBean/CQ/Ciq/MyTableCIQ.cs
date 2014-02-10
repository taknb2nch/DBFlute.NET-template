
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ.BS;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.CQ.Ciq {

    [System.Serializable]
    public class MyTableCIQ : AbstractBsMyTableCQ {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected BsMyTableCQ _myCQ;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public MyTableCIQ(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel, BsMyTableCQ myCQ)
            : base(childQuery, sqlClause, aliasName, nestLevel) {
            _myCQ = myCQ;
            _foreignPropertyName = _myCQ.xgetForeignPropertyName();// Accept foreign property name.
            _relationPath = _myCQ.xgetRelationPath();// Accept relation path.
        }

        // ===================================================================================
        //                                                             Override about Register
        //                                                             =======================
        public override void reflectRelationOnUnionQuery(ConditionQuery baseQueryAsSuper, ConditionQuery unionQueryAsSuper) {
            throw new UnsupportedOperationException("InlineQuery must not need UNION method: " + baseQueryAsSuper + " : " + unionQueryAsSuper);
        }
    
        protected override void setupConditionValueAndRegisterWhereClause(ConditionKey key, Object value, ConditionValue cvalue, String colName) {
            regIQ(key, value, cvalue, colName);
        }
    
        protected override void setupConditionValueAndRegisterWhereClause(ConditionKey key, Object value, ConditionValue cvalue
                                                                        , String colName, ConditionOption option) {
            regIQ(key, value, cvalue, colName, option);
        }
    
        protected override void registerWhereClause(String whereClause) {
            registerInlineWhereClause(whereClause);
        }
    
        protected override String getInScopeSubQueryRealColumnName(String columnName) {
            if (_onClause) {
                throw new UnsupportedOperationException("InScopeSubQuery of on-clause is unsupported");
            }
            return _onClause ? xgetAliasName() + "." + columnName : columnName;
        }
    
        protected override void registerExistsSubQuery(ConditionQuery subQuery
                                     , String columnName, String relatedColumnName, String propertyName) {
            throw new UnsupportedOperationException("Sorry! ExistsSubQuery at inline view is unsupported. So please use InScopeSubQyery.");
        }


        protected override ConditionValue getCValueId() {
            return _myCQ.Id;
        }


        protected override ConditionValue getCValueUserName() {
            return _myCQ.UserName;
        }


        protected override ConditionValue getCValueAge() {
            return _myCQ.Age;
        }


        protected override ConditionValue getCValueAttendanceFlag() {
            return _myCQ.AttendanceFlag;
        }


        protected override ConditionValue getCValueCreatedDatetime() {
            return _myCQ.CreatedDatetime;
        }


        protected override ConditionValue getCValueCreatedUser() {
            return _myCQ.CreatedUser;
        }


        protected override ConditionValue getCValueUpdatedDatetime() {
            return _myCQ.UpdatedDatetime;
        }


        protected override ConditionValue getCValueUpdatedUser() {
            return _myCQ.UpdatedUser;
        }


        protected override ConditionValue getCValueVersionNo() {
            return _myCQ.VersionNo;
        }


        // ===================================================================================
        //                                                                     Scalar SubQuery
        //                                                                     ===============
        public override String keepScalarSubQuery(MyTableCQ subQuery) {
            throw new UnsupportedOperationException("ScalarSubQuery at inline() is unsupported! Sorry!");
        }

        // ===============================================================================
        //                                                         Myself InScope SubQuery
        //                                                         =======================
        public override String keepMyselfInScopeSubQuery(MyTableCQ subQuery) {
            throw new UnsupportedOperationException("MyselfInScopeSubQuery at inline() is unsupported! Sorry!");
        }
    }
}
