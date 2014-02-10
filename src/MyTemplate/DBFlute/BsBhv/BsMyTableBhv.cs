
using System;
using System.Collections.Generic;

using Seasar.Quill;
using Seasar.Quill.Attrs;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv.Load;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv.Setup;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.BsEntity.Dbm;
using Aaa.Bbb.Ccc.DBFlute.ExDao;
using Aaa.Bbb.Ccc.DBFlute.ExEntity;
using Aaa.Bbb.Ccc.DBFlute.CBean;


namespace Aaa.Bbb.Ccc.DBFlute.ExBhv {

    [Implementation]
    public partial class MyTableBhv : Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv.AbstractBehaviorWritable {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        /*df:beginQueryPath*/
        public static readonly String PATH_selectMyTableEx = "selectMyTableEx";
        /*df:endQueryPath*/

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected MyTableDao _dao;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public MyTableBhv() {
        }
        
        // ===============================================================================
        //                                                                Initialized Mark
        //                                                                ================
        public override bool IsInitialized { get { return _dao != null; } }

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        public override String TableDbName { get { return "my_table"; } }

        // ===============================================================================
        //                                                                          DBMeta
        //                                                                          ======
        public override DBMeta DBMeta { get { return MyTableDbm.GetInstance(); } }
        public MyTableDbm MyDBMeta { get { return MyTableDbm.GetInstance(); } }

        // ===============================================================================
        //                                                                    New Instance
        //                                                                    ============
        #region New Instance
        public override Entity NewEntity() { return NewMyEntity(); }
        public override ConditionBean NewConditionBean() { return NewMyConditionBean(); }
        public virtual MyTable NewMyEntity() { return new MyTable(); }
        public virtual MyTableCB NewMyConditionBean() { return new MyTableCB(); }
        #endregion

        // ===============================================================================
        //                                                                    Count Select
        //                                                                    ============
        #region Count Select
        public virtual int SelectCount(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            return this.DelegateSelectCount(cb);
        }

        protected override int DoReadCount(ConditionBean cb) {
            return SelectCount(Downcast(cb));
        }
        #endregion

        // ===============================================================================
        //                                                                   Entity Select
        //                                                                   =============
        #region Entity Select
        public virtual MyTable SelectEntity(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            if (!cb.HasWhereClause() && cb.FetchSize != 1) { // if no condition for one
                throwSelectEntityConditionNotFoundException(cb);
            }
            int preSafetyMaxResultSize = xcheckSafetyResultAsOne(cb);
            IList<MyTable> ls = null;
            try {
                ls = this.DelegateSelectList(cb);
            } catch (DangerousResultSizeException e) {
                ThrowEntityDuplicatedException("{over safetyMaxResultSize '1'}", cb, e);
                return null; // unreachable
            } finally {
                xrestoreSafetyResult(cb, preSafetyMaxResultSize);
            }
            if (ls.Count == 0) { return null; }
            AssertEntitySelectedAsOne(ls, cb);
            return (MyTable)ls[0];
        }

        protected override Entity DoReadEntity(ConditionBean cb) {
            return SelectEntity(Downcast(cb));
        }

        public virtual MyTable SelectEntityWithDeletedCheck(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            MyTable entity = SelectEntity(cb);
            AssertEntityNotDeleted(entity, cb);
            return entity;
        }

        protected override Entity DoReadEntityWithDeletedCheck(ConditionBean cb) {
            return SelectEntityWithDeletedCheck(Downcast(cb));
        }

        public virtual MyTable SelectByPKValue(int? id) {
            return SelectEntity(BuildPKCB(id));
        }

        public virtual MyTable SelectByPKValueWithDeletedCheck(int? id) {
            return SelectEntityWithDeletedCheck(BuildPKCB(id));
        }

        private MyTableCB BuildPKCB(int? id) {
            AssertObjectNotNull("id", id);
            MyTableCB cb = NewMyConditionBean();
            cb.Query().SetId_Equal(id);
            return cb;            
        }
        #endregion

        // ===============================================================================
        //                                                                     List Select
        //                                                                     ===========
        #region List Select
        public virtual ListResultBean<MyTable> SelectList(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            return new ResultBeanBuilder<MyTable>(TableDbName).BuildListResultBean(cb, this.DelegateSelectList(cb));
        }
        #endregion

        // ===============================================================================
        //                                                                     Page Select
        //                                                                     ===========
        #region Page Select
        public virtual PagingResultBean<MyTable> SelectPage(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            PagingInvoker<MyTable> invoker = new PagingInvoker<MyTable>(TableDbName);
            return invoker.InvokePaging(new InternalSelectPagingHandler(this, cb));
        }

        private class InternalSelectPagingHandler : PagingHandler<MyTable> {
            protected MyTableBhv _bhv; protected MyTableCB _cb;
            public InternalSelectPagingHandler(MyTableBhv bhv, MyTableCB cb) { _bhv = bhv; _cb = cb; }
            public PagingBean PagingBean { get { return _cb; } }
            public int Count() { return _bhv.SelectCount(_cb); }
            public IList<MyTable> Paging() { return _bhv.SelectList(_cb); }
        }
        #endregion

        // ===============================================================================
        //                                                                        Sequence
        //                                                                        ========
        public int? SelectNextVal() {
            return DelegateSelectNextVal();
        }
        protected override void SetupNextValueToPrimaryKey(Entity entity) {// Very Internal
            MyTable myEntity = (MyTable)entity;
            myEntity.Id = SelectNextVal();
        }

        // ===============================================================================
        //                                                                   Load Referrer
        //                                                                   =============
        #region Load Referrer
        #endregion

        // ===============================================================================
        //                                                                Pull out Foreign
        //                                                                ================
        #region Pullout Foreign
        #endregion


        // ===============================================================================
        //                                                                   Entity Update
        //                                                                   =============
        #region Basic Entity Update
        public virtual void Insert(MyTable entity) {
            AssertEntityNotNull(entity);
            this.DelegateInsert(entity);
        }

        protected override void DoCreate(Entity entity) {
            Insert(Downcast(entity));
        }

        public virtual void Update(MyTable entity) {
            AssertEntityNotNull(entity);
            AssertEntityHasVersionNoValue(entity);
            AssertEntityHasUpdateDateValue(entity);
            int updatedCount = this.DelegateUpdate(entity);
            AssertUpdatedEntity(entity, updatedCount);
        }

        protected override void DoModify(Entity entity) {
            Update(Downcast(entity));
        }

        public virtual void UpdateNonstrict(MyTable entity) {
            AssertEntityNotNull(entity);
            int updatedCount = this.DelegateUpdateNonstrict(entity);
            AssertUpdatedEntity(entity, updatedCount);
        }

        public void InsertOrUpdate(MyTable entity) {
            HelpInsertOrUpdateInternally<MyTable, MyTableCB>(entity, new MyInternalInsertOrUpdateCallback(this));
        }
        protected class MyInternalInsertOrUpdateCallback : InternalInsertOrUpdateCallback<MyTable, MyTableCB> {
            protected MyTableBhv _bhv;
            public MyInternalInsertOrUpdateCallback(MyTableBhv bhv) { _bhv = bhv; }
            public void CallbackInsert(MyTable entity) { _bhv.Insert(entity); }
            public void CallbackUpdate(MyTable entity) { _bhv.Update(entity); }
            public MyTableCB CallbackNewMyConditionBean() { return _bhv.NewMyConditionBean(); }
            public void CallbackSetupPrimaryKeyCondition(MyTableCB cb, MyTable entity) {
                cb.Query().SetId_Equal(entity.Id);
            }
            public int CallbackSelectCount(MyTableCB cb) { return _bhv.SelectCount(cb); }
        }

        public void InsertOrUpdateNonstrict(MyTable entity) {
            HelpInsertOrUpdateInternally<MyTable>(entity, new MyInternalInsertOrUpdateNonstrictCallback(this));
        }
        protected class MyInternalInsertOrUpdateNonstrictCallback : InternalInsertOrUpdateNonstrictCallback<MyTable> {
            protected MyTableBhv _bhv;
            public MyInternalInsertOrUpdateNonstrictCallback(MyTableBhv bhv) { _bhv = bhv; }
            public void CallbackInsert(MyTable entity) { _bhv.Insert(entity); }
            public void CallbackUpdateNonstrict(MyTable entity) { _bhv.UpdateNonstrict(entity); }
        }

        public virtual void Delete(MyTable entity) {
            HelpDeleteInternally<MyTable>(entity, new MyInternalDeleteCallback(this));
        }

        protected override void DoRemove(Entity entity) {
            Remove(Downcast(entity));
        }

        protected class MyInternalDeleteCallback : InternalDeleteCallback<MyTable> {
            protected MyTableBhv _bhv;
            public MyInternalDeleteCallback(MyTableBhv bhv) { _bhv = bhv; }
            public int CallbackDelegateDelete(MyTable entity) { return _bhv.DelegateDelete(entity); }
        }

        public virtual void DeleteNonstrict(MyTable entity) {
            HelpDeleteNonstrictInternally<MyTable>(entity, new MyInternalDeleteNonstrictCallback(this));
        }
        protected class MyInternalDeleteNonstrictCallback : InternalDeleteNonstrictCallback<MyTable> {
            protected MyTableBhv _bhv;
            public MyInternalDeleteNonstrictCallback(MyTableBhv bhv) { _bhv = bhv; }
            public int CallbackDelegateDeleteNonstrict(MyTable entity) { return _bhv.DelegateDeleteNonstrict(entity); }
        }

        public virtual void DeleteNonstrictIgnoreDeleted(MyTable entity) {
            HelpDeleteNonstrictIgnoreDeletedInternally<MyTable>(entity, new MyInternalDeleteNonstrictIgnoreDeletedCallback(this));
        }
        protected class MyInternalDeleteNonstrictIgnoreDeletedCallback : InternalDeleteNonstrictIgnoreDeletedCallback<MyTable> {
            protected MyTableBhv _bhv;
            public MyInternalDeleteNonstrictIgnoreDeletedCallback(MyTableBhv bhv) { _bhv = bhv; }
            public int CallbackDelegateDeleteNonstrict(MyTable entity) { return _bhv.DelegateDeleteNonstrict(entity); }
        }
        #endregion

        // ===============================================================================
        //                                                                    Query Update
        //                                                                    ============
        public int QueryUpdate(MyTable myTable, MyTableCB cb) {
            AssertObjectNotNull("myTable", myTable); AssertConditionBeanNotNull(cb);
            SetupCommonColumnOfUpdateIfNeeds(myTable);
            FilterEntityOfUpdate(myTable); AssertEntityOfUpdate(myTable);
            return this.Dao.UpdateByQuery(cb, myTable);
        }

        public int QueryDelete(MyTableCB cb) {
            AssertConditionBeanNotNull(cb);
            return this.Dao.DeleteByQuery(cb);
        }

        // ===============================================================================
        //                                                            Optimistic Lock Info
        //                                                            ====================
        protected override bool HasVersionNoValue(Entity entity) {
            return Downcast(entity).VersionNo != null;
        }

        protected override bool HasUpdateDateValue(Entity entity) {
            return false;
        }

        // ===============================================================================
        //                                                                 Delegate Method
        //                                                                 ===============
        #region Delegate Method
        protected int DelegateSelectCount(MyTableCB cb) { AssertConditionBeanNotNull(cb); return this.Dao.SelectCount(cb); }
        protected IList<MyTable> DelegateSelectList(MyTableCB cb) { AssertConditionBeanNotNull(cb); return this.Dao.SelectList(cb); }
        protected int? DelegateSelectNextVal() { return this.Dao.SelectNextVal(); }

        protected int DelegateInsert(MyTable e) { if (!ProcessBeforeInsert(e)) { return 1; } return this.Dao.Insert(e); }
        protected int DelegateUpdate(MyTable e)
        { if (!ProcessBeforeUpdate(e)) { return 1; } return this.Dao.UpdateModifiedOnly(e); }
        protected int DelegateUpdateNonstrict(MyTable e)
        { if (!ProcessBeforeUpdate(e)) { return 1; } return this.Dao.UpdateNonstrictModifiedOnly(e); }
        protected int DelegateDelete(MyTable e)
        { if (!ProcessBeforeDelete(e)) { return 1; } return this.Dao.Delete(e); }
        protected int DelegateDeleteNonstrict(MyTable e)
        { if (!ProcessBeforeDelete(e)) { return 1; } return this.Dao.DeleteNonstrict(e); }
        #endregion

        // ===============================================================================
        //                                                                 Downcast Helper
        //                                                                 ===============
        protected MyTable Downcast(Entity entity) {
            return (MyTable)entity;
        }

        protected MyTableCB Downcast(ConditionBean cb) {
            return (MyTableCB)cb;
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public virtual MyTableDao Dao { get { return _dao; } set { _dao = value; } }
    }
}
