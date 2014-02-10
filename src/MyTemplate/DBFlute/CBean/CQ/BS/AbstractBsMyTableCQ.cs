
using System;
using System.Collections.Generic;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.CQ.BS {

    [System.Serializable]
    public abstract class AbstractBsMyTableCQ : AbstractConditionQuery {

        public AbstractBsMyTableCQ(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel)
            : base(childQuery, sqlClause, aliasName, nestLevel) {}

        public override String getTableDbName() { return "my_table"; }
        public override String getTableSqlName() { return "my_table"; }

        public void SetId_Equal(int? v) { regId(CK_EQ, v); }
        public void SetId_NotEqual(int? v) { regId(CK_NES, v); }
        public void SetId_GreaterThan(int? v) { regId(CK_GT, v); }
        public void SetId_LessThan(int? v) { regId(CK_LT, v); }
        public void SetId_GreaterEqual(int? v) { regId(CK_GE, v); }
        public void SetId_LessEqual(int? v) { regId(CK_LE, v); }
        public void SetId_InScope(IList<int?> ls) { regINS<int?>(CK_INS, cTL<int?>(ls), getCValueId(), "id"); }
        public void SetId_NotInScope(IList<int?> ls) { regINS<int?>(CK_NINS, cTL<int?>(ls), getCValueId(), "id"); }
        public void SetId_IsNull() { regId(CK_ISN, DUMMY_OBJECT); }
        public void SetId_IsNotNull() { regId(CK_ISNN, DUMMY_OBJECT); }
        protected void regId(ConditionKey k, Object v) { regQ(k, v, getCValueId(), "id"); }
        protected abstract ConditionValue getCValueId();

        public void SetUserName_Equal(String v) { DoSetUserName_Equal(fRES(v)); }
        protected void DoSetUserName_Equal(String v) { regUserName(CK_EQ, v); }
        public void SetUserName_NotEqual(String v) { DoSetUserName_NotEqual(fRES(v)); }
        protected void DoSetUserName_NotEqual(String v) { regUserName(CK_NES, v); }
        public void SetUserName_GreaterThan(String v) { regUserName(CK_GT, fRES(v)); }
        public void SetUserName_LessThan(String v) { regUserName(CK_LT, fRES(v)); }
        public void SetUserName_GreaterEqual(String v) { regUserName(CK_GE, fRES(v)); }
        public void SetUserName_LessEqual(String v) { regUserName(CK_LE, fRES(v)); }
        public void SetUserName_InScope(IList<String> ls) { regINS<String>(CK_INS, cTL<String>(ls), getCValueUserName(), "user_name"); }
        public void SetUserName_NotInScope(IList<String> ls) { regINS<String>(CK_NINS, cTL<String>(ls), getCValueUserName(), "user_name"); }
        public void SetUserName_PrefixSearch(String v) { SetUserName_LikeSearch(v, cLSOP()); }
        public void SetUserName_LikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_LS, fRES(v), getCValueUserName(), "user_name", option); }
        public void SetUserName_NotLikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_NLS, fRES(v), getCValueUserName(), "user_name", option); }
        public void SetUserName_IsNull() { regUserName(CK_ISN, DUMMY_OBJECT); }
        public void SetUserName_IsNotNull() { regUserName(CK_ISNN, DUMMY_OBJECT); }
        protected void regUserName(ConditionKey k, Object v) { regQ(k, v, getCValueUserName(), "user_name"); }
        protected abstract ConditionValue getCValueUserName();

        public void SetAge_Equal(int? v) { regAge(CK_EQ, v); }
        public void SetAge_NotEqual(int? v) { regAge(CK_NES, v); }
        public void SetAge_GreaterThan(int? v) { regAge(CK_GT, v); }
        public void SetAge_LessThan(int? v) { regAge(CK_LT, v); }
        public void SetAge_GreaterEqual(int? v) { regAge(CK_GE, v); }
        public void SetAge_LessEqual(int? v) { regAge(CK_LE, v); }
        public void SetAge_InScope(IList<int?> ls) { regINS<int?>(CK_INS, cTL<int?>(ls), getCValueAge(), "age"); }
        public void SetAge_NotInScope(IList<int?> ls) { regINS<int?>(CK_NINS, cTL<int?>(ls), getCValueAge(), "age"); }
        protected void regAge(ConditionKey k, Object v) { regQ(k, v, getCValueAge(), "age"); }
        protected abstract ConditionValue getCValueAge();

        public void SetAttendanceFlag_Equal(String v) { DoSetAttendanceFlag_Equal(fRES(v)); }
        /// <summary>
        /// Set the value of True of attendanceFlag as equal. { = }
        /// はい: 有効を示す
        /// </summary>
        public void SetAttendanceFlag_Equal_True() {
            DoSetAttendanceFlag_Equal(CDef.Flag.True.Code);
        }
        /// <summary>
        /// Set the value of False of attendanceFlag as equal. { = }
        /// いいえ: 無効を示す
        /// </summary>
        public void SetAttendanceFlag_Equal_False() {
            DoSetAttendanceFlag_Equal(CDef.Flag.False.Code);
        }
        protected void DoSetAttendanceFlag_Equal(String v) { regAttendanceFlag(CK_EQ, v); }
        public void SetAttendanceFlag_NotEqual(String v) { DoSetAttendanceFlag_NotEqual(fRES(v)); }
        /// <summary>
        /// Set the value of True of attendanceFlag as notEqual. { &lt;&gt; }
        /// はい: 有効を示す
        /// </summary>
        public void SetAttendanceFlag_NotEqual_True() {
            DoSetAttendanceFlag_NotEqual(CDef.Flag.True.Code);
        }
        /// <summary>
        /// Set the value of False of attendanceFlag as notEqual. { &lt;&gt; }
        /// いいえ: 無効を示す
        /// </summary>
        public void SetAttendanceFlag_NotEqual_False() {
            DoSetAttendanceFlag_NotEqual(CDef.Flag.False.Code);
        }
        protected void DoSetAttendanceFlag_NotEqual(String v) { regAttendanceFlag(CK_NES, v); }
        public void SetAttendanceFlag_InScope(IList<String> ls) { regINS<String>(CK_INS, cTL<String>(ls), getCValueAttendanceFlag(), "attendance_flag"); }
        public void SetAttendanceFlag_NotInScope(IList<String> ls) { regINS<String>(CK_NINS, cTL<String>(ls), getCValueAttendanceFlag(), "attendance_flag"); }
        protected void regAttendanceFlag(ConditionKey k, Object v) { regQ(k, v, getCValueAttendanceFlag(), "attendance_flag"); }
        protected abstract ConditionValue getCValueAttendanceFlag();

        public void SetCreatedDatetime_Equal(DateTime? v) { regCreatedDatetime(CK_EQ, v); }
        public void SetCreatedDatetime_GreaterThan(DateTime? v) { regCreatedDatetime(CK_GT, v); }
        public void SetCreatedDatetime_LessThan(DateTime? v) { regCreatedDatetime(CK_LT, v); }
        public void SetCreatedDatetime_GreaterEqual(DateTime? v) { regCreatedDatetime(CK_GE, v); }
        public void SetCreatedDatetime_LessEqual(DateTime? v) { regCreatedDatetime(CK_LE, v); }
        public void SetCreatedDatetime_FromTo(DateTime? from, DateTime? to, FromToOption option)
        { regFTQ(from, to, getCValueCreatedDatetime(), "created_datetime", option); }
        public void SetCreatedDatetime_DateFromTo(DateTime? from, DateTime? to) { SetCreatedDatetime_FromTo(from, to, new DateFromToOption()); }
        protected void regCreatedDatetime(ConditionKey k, Object v) { regQ(k, v, getCValueCreatedDatetime(), "created_datetime"); }
        protected abstract ConditionValue getCValueCreatedDatetime();

        public void SetCreatedUser_Equal(String v) { DoSetCreatedUser_Equal(fRES(v)); }
        protected void DoSetCreatedUser_Equal(String v) { regCreatedUser(CK_EQ, v); }
        public void SetCreatedUser_NotEqual(String v) { DoSetCreatedUser_NotEqual(fRES(v)); }
        protected void DoSetCreatedUser_NotEqual(String v) { regCreatedUser(CK_NES, v); }
        public void SetCreatedUser_GreaterThan(String v) { regCreatedUser(CK_GT, fRES(v)); }
        public void SetCreatedUser_LessThan(String v) { regCreatedUser(CK_LT, fRES(v)); }
        public void SetCreatedUser_GreaterEqual(String v) { regCreatedUser(CK_GE, fRES(v)); }
        public void SetCreatedUser_LessEqual(String v) { regCreatedUser(CK_LE, fRES(v)); }
        public void SetCreatedUser_InScope(IList<String> ls) { regINS<String>(CK_INS, cTL<String>(ls), getCValueCreatedUser(), "created_user"); }
        public void SetCreatedUser_NotInScope(IList<String> ls) { regINS<String>(CK_NINS, cTL<String>(ls), getCValueCreatedUser(), "created_user"); }
        public void SetCreatedUser_PrefixSearch(String v) { SetCreatedUser_LikeSearch(v, cLSOP()); }
        public void SetCreatedUser_LikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_LS, fRES(v), getCValueCreatedUser(), "created_user", option); }
        public void SetCreatedUser_NotLikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_NLS, fRES(v), getCValueCreatedUser(), "created_user", option); }
        protected void regCreatedUser(ConditionKey k, Object v) { regQ(k, v, getCValueCreatedUser(), "created_user"); }
        protected abstract ConditionValue getCValueCreatedUser();

        public void SetUpdatedDatetime_Equal(DateTime? v) { regUpdatedDatetime(CK_EQ, v); }
        public void SetUpdatedDatetime_GreaterThan(DateTime? v) { regUpdatedDatetime(CK_GT, v); }
        public void SetUpdatedDatetime_LessThan(DateTime? v) { regUpdatedDatetime(CK_LT, v); }
        public void SetUpdatedDatetime_GreaterEqual(DateTime? v) { regUpdatedDatetime(CK_GE, v); }
        public void SetUpdatedDatetime_LessEqual(DateTime? v) { regUpdatedDatetime(CK_LE, v); }
        public void SetUpdatedDatetime_FromTo(DateTime? from, DateTime? to, FromToOption option)
        { regFTQ(from, to, getCValueUpdatedDatetime(), "updated_datetime", option); }
        public void SetUpdatedDatetime_DateFromTo(DateTime? from, DateTime? to) { SetUpdatedDatetime_FromTo(from, to, new DateFromToOption()); }
        public void SetUpdatedDatetime_IsNull() { regUpdatedDatetime(CK_ISN, DUMMY_OBJECT); }
        public void SetUpdatedDatetime_IsNotNull() { regUpdatedDatetime(CK_ISNN, DUMMY_OBJECT); }
        protected void regUpdatedDatetime(ConditionKey k, Object v) { regQ(k, v, getCValueUpdatedDatetime(), "updated_datetime"); }
        protected abstract ConditionValue getCValueUpdatedDatetime();

        public void SetUpdatedUser_Equal(String v) { DoSetUpdatedUser_Equal(fRES(v)); }
        protected void DoSetUpdatedUser_Equal(String v) { regUpdatedUser(CK_EQ, v); }
        public void SetUpdatedUser_NotEqual(String v) { DoSetUpdatedUser_NotEqual(fRES(v)); }
        protected void DoSetUpdatedUser_NotEqual(String v) { regUpdatedUser(CK_NES, v); }
        public void SetUpdatedUser_GreaterThan(String v) { regUpdatedUser(CK_GT, fRES(v)); }
        public void SetUpdatedUser_LessThan(String v) { regUpdatedUser(CK_LT, fRES(v)); }
        public void SetUpdatedUser_GreaterEqual(String v) { regUpdatedUser(CK_GE, fRES(v)); }
        public void SetUpdatedUser_LessEqual(String v) { regUpdatedUser(CK_LE, fRES(v)); }
        public void SetUpdatedUser_InScope(IList<String> ls) { regINS<String>(CK_INS, cTL<String>(ls), getCValueUpdatedUser(), "updated_user"); }
        public void SetUpdatedUser_NotInScope(IList<String> ls) { regINS<String>(CK_NINS, cTL<String>(ls), getCValueUpdatedUser(), "updated_user"); }
        public void SetUpdatedUser_PrefixSearch(String v) { SetUpdatedUser_LikeSearch(v, cLSOP()); }
        public void SetUpdatedUser_LikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_LS, fRES(v), getCValueUpdatedUser(), "updated_user", option); }
        public void SetUpdatedUser_NotLikeSearch(String v, LikeSearchOption option)
        { regLSQ(CK_NLS, fRES(v), getCValueUpdatedUser(), "updated_user", option); }
        public void SetUpdatedUser_IsNull() { regUpdatedUser(CK_ISN, DUMMY_OBJECT); }
        public void SetUpdatedUser_IsNotNull() { regUpdatedUser(CK_ISNN, DUMMY_OBJECT); }
        protected void regUpdatedUser(ConditionKey k, Object v) { regQ(k, v, getCValueUpdatedUser(), "updated_user"); }
        protected abstract ConditionValue getCValueUpdatedUser();

        public void SetVersionNo_Equal(int? v) { regVersionNo(CK_EQ, v); }
        public void SetVersionNo_NotEqual(int? v) { regVersionNo(CK_NES, v); }
        public void SetVersionNo_GreaterThan(int? v) { regVersionNo(CK_GT, v); }
        public void SetVersionNo_LessThan(int? v) { regVersionNo(CK_LT, v); }
        public void SetVersionNo_GreaterEqual(int? v) { regVersionNo(CK_GE, v); }
        public void SetVersionNo_LessEqual(int? v) { regVersionNo(CK_LE, v); }
        public void SetVersionNo_InScope(IList<int?> ls) { regINS<int?>(CK_INS, cTL<int?>(ls), getCValueVersionNo(), "version_no"); }
        public void SetVersionNo_NotInScope(IList<int?> ls) { regINS<int?>(CK_NINS, cTL<int?>(ls), getCValueVersionNo(), "version_no"); }
        protected void regVersionNo(ConditionKey k, Object v) { regQ(k, v, getCValueVersionNo(), "version_no"); }
        protected abstract ConditionValue getCValueVersionNo();

        // ===================================================================================
        //                                                                    Scalar Condition
        //                                                                    ================
        public SSQFunction<MyTableCB> Scalar_Equal() {
            return xcreateSSQFunction("=");
        }

        public SSQFunction<MyTableCB> Scalar_NotEqual() {
            return xcreateSSQFunction("<>");
        }

        public SSQFunction<MyTableCB> Scalar_GreaterEqual() {
            return xcreateSSQFunction(">=");
        }

        public SSQFunction<MyTableCB> Scalar_GreaterThan() {
            return xcreateSSQFunction(">");
        }

        public SSQFunction<MyTableCB> Scalar_LessEqual() {
            return xcreateSSQFunction("<=");
        }

        public SSQFunction<MyTableCB> Scalar_LessThan() {
            return xcreateSSQFunction("<");
        }

        protected SSQFunction<MyTableCB> xcreateSSQFunction(String operand) {
            return new SSQFunction<MyTableCB>(delegate(String function, SubQuery<MyTableCB> subQuery) {
                xscalarSubQuery(function, subQuery, operand);
            });
        }

        protected void xscalarSubQuery(String function, SubQuery<MyTableCB> subQuery, String operand) {
            assertObjectNotNull("subQuery<MyTableCB>", subQuery);
            MyTableCB cb = new MyTableCB(); cb.xsetupForScalarCondition(this); subQuery.Invoke(cb);
            String subQueryPropertyName = keepScalarSubQuery(cb.Query()); // for saving query-value.
            registerScalarSubQuery(function, cb.Query(), subQueryPropertyName, operand);
        }
        public abstract String keepScalarSubQuery(MyTableCQ subQuery);

        // ===============================================================================
        //                                                                  MySelf InScope
        //                                                                  ==============
        public void MyselfInScope(SubQuery<MyTableCB> subQuery) {
            assertObjectNotNull("subQuery<MyTableCB>", subQuery);
            MyTableCB cb = new MyTableCB(); cb.xsetupForInScopeRelation(this); subQuery.Invoke(cb);
            String subQueryPropertyName = keepMyselfInScopeSubQuery(cb.Query()); // for saving query-value.
            registerInScopeSubQuery(cb.Query(), "id", "id", subQueryPropertyName);
        }
        public abstract String keepMyselfInScopeSubQuery(MyTableCQ subQuery);

        public override String ToString() { return xgetSqlClause().getClause(); }
    }
}
