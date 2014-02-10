
using System;
using System.Collections;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Helper;

using Aaa.Bbb.Ccc.DBFlute.CBean;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ;
using Aaa.Bbb.Ccc.DBFlute.CBean.Nss;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.BS {

    [System.Serializable]
    public class BsMyTableCB : AbstractConditionBean {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected MyTableCQ _conditionQuery;

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        public override String TableDbName { get { return "my_table"; } }

        // ===============================================================================
        //                                                             PrimaryKey Handling
        //                                                             ===================
        public void AcceptPrimaryKey(int? id) {
            assertObjectNotNull("id", id);
            BsMyTableCB cb = this;
            cb.Query().SetId_Equal(id);
        }

        public override ConditionBean AddOrderBy_PK_Asc() {
            Query().AddOrderBy_Id_Asc();
            return this;
        }

        public override ConditionBean AddOrderBy_PK_Desc() {
            Query().AddOrderBy_Id_Desc();
            return this;
        }

        // ===============================================================================
        //                                                                           Query
        //                                                                           =====
        public MyTableCQ Query() {
            return this.ConditionQuery;
        }

        public MyTableCQ ConditionQuery {
            get {
                if (_conditionQuery == null) {
                    _conditionQuery = CreateLocalCQ();
                }
                return _conditionQuery;
            }
        }

        protected virtual MyTableCQ CreateLocalCQ() {
            return xcreateCQ(null, this.SqlClause, this.SqlClause.getBasePointAliasName(), 0);
        }

        protected virtual MyTableCQ xcreateCQ(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel) {
            return new MyTableCQ(childQuery, sqlClause, aliasName, nestLevel);
        }

        public override ConditionQuery LocalCQ {
            get { return this.ConditionQuery; }
        }

        // ===============================================================================
        //                                                                           Union
        //                                                                           =====
	    public virtual void Union(UnionQuery<MyTableCB> unionQuery) {
            MyTableCB cb = new MyTableCB();
            cb.xsetupForUnion(this); xsyncUQ(cb); unionQuery.Invoke(cb);
		    MyTableCQ cq = cb.Query(); Query().xsetUnionQuery(cq);
        }

	    public virtual void UnionAll(UnionQuery<MyTableCB> unionQuery) {
            MyTableCB cb = new MyTableCB();
            cb.xsetupForUnion(this); xsyncUQ(cb); unionQuery.Invoke(cb);
		    MyTableCQ cq = cb.Query(); Query().xsetUnionAllQuery(cq);
	    }

        public override bool HasUnionQueryOrUnionAllQuery() {
            return Query().hasUnionQueryOrUnionAllQuery();
        }

        // ===============================================================================
        //                                                                    Setup Select
        //                                                                    ============

        // [DBFlute-0.7.4]
        // ===============================================================================
        //                                                                         Specify
        //                                                                         =======
        protected MyTableCBSpecification _specification;
        public MyTableCBSpecification Specify() {
            if (_specification == null) { _specification = new MyTableCBSpecification(this, new MySpQyCall(this), _forDerivedReferrer, _forScalarSelect, _forScalarCondition, _forColumnQuery); }
            return _specification;
        }
        protected bool HasSpecifiedColumn { get {
            return _specification != null && _specification.IsAlreadySpecifiedRequiredColumn;
        }}
        protected class MySpQyCall : HpSpQyCall<MyTableCQ> {
			protected BsMyTableCB _myCB;
			public MySpQyCall(BsMyTableCB myCB) { _myCB = myCB; }
    		public bool has() { return true; } public MyTableCQ qy() { return _myCB.Query(); }
    	}

        // [DBFlute-0.8.9.18]
        // ===============================================================================
        //                                                                     ColumnQuery
        //                                                                     ===========
        public HpColQyOperand<MyTableCB> ColumnQuery(SpecifyQuery<MyTableCB> leftSpecifyQuery) {
            return new HpColQyOperand<MyTableCB>(delegate(SpecifyQuery<MyTableCB> rightSp, String operand) {
                xcolqy(xcreateColumnQueryCB(), xcreateColumnQueryCB(), leftSpecifyQuery, rightSp, operand);
            });
        }

        protected MyTableCB xcreateColumnQueryCB() {
            MyTableCB cb = new MyTableCB();
            cb.xsetupForColumnQuery((MyTableCB)this);
            return cb;
        }

        // [DBFlute-0.8.9.9]
        // ===============================================================================
        //                                                                    OrScopeQuery
        //                                                                    ============
        public void OrScopeQuery(OrQuery<MyTableCB> orQuery) {
            xorQ((MyTableCB)this, orQuery);
        }

        // ===============================================================================
        //                                                                    Purpose Type
        //                                                                    ============
        public void xsetupForColumnQuery(MyTableCB mainCB) {
            xinheritSubQueryInfo(mainCB.LocalCQ);
            //xchangePurposeSqlClause(HpCBPurpose.COLUMN_QUERY);
            _forColumnQuery = true; // old style

            // inherits a parent query to synchronize real name
            // (and also for suppressing query check) 
            Specify().xsetSyncQyCall(new MyTableCBColQySpQyCall(mainCB));
        }
    }

    public class MyTableCBColQySpQyCall : HpSpQyCall<MyTableCQ> {
        protected MyTableCB _mainCB;
        public MyTableCBColQySpQyCall(MyTableCB mainCB) {
            _mainCB = mainCB;
        }
        public bool has() { return true; } 
        public MyTableCQ qy() { return _mainCB.Query(); } 
    }

    public class MyTableCBSpecification : AbstractSpecification<MyTableCQ> {
        public MyTableCBSpecification(ConditionBean baseCB, HpSpQyCall<MyTableCQ> qyCall
                                                      , bool forDerivedReferrer, bool forScalarSelect, bool forScalarSubQuery, bool forColumnQuery)
        : base(baseCB, qyCall, forDerivedReferrer, forScalarSelect, forScalarSubQuery, forColumnQuery) { }
        public void ColumnId() { doColumn("id"); }
        public void ColumnUserName() { doColumn("user_name"); }
        public void ColumnAge() { doColumn("age"); }
        public void ColumnAttendanceFlag() { doColumn("attendance_flag"); }
        public void ColumnCreatedDatetime() { doColumn("created_datetime"); }
        public void ColumnCreatedUser() { doColumn("created_user"); }
        public void ColumnUpdatedDatetime() { doColumn("updated_datetime"); }
        public void ColumnUpdatedUser() { doColumn("updated_user"); }
        public void ColumnVersionNo() { doColumn("version_no"); }
        protected override void doSpecifyRequiredColumn() {
            ColumnId(); // PK
        }
        protected override String getTableDbName() { return "my_table"; }
    }
}
