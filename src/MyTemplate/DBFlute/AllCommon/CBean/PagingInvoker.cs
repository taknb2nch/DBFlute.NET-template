
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public class PagingInvoker<ENTITY> {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected String _tableDbName;
        protected bool _countLater;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public PagingInvoker(String tableDbName) {
            this._tableDbName = tableDbName;
        }

        // ===============================================================================
        //                                                                          Invoke
        //                                                                          ======
        public PagingResultBean<ENTITY> InvokePaging(PagingHandler<ENTITY> handler) {
            AssertObjectNotNull("handler", handler);
            PagingBean pagingBean = handler.PagingBean;
            AssertObjectNotNull("handler.getPagingBean()", pagingBean);
            if (!pagingBean.IsFetchScopeEffective) {
                String msg = "The paging bean is not effective about fetch-scope!";
                msg = msg + " When you select page, you should set up fetch-scope of paging bean(Should invoke fetchFirst() and fetchPage()!).";
                msg = msg + " The paging bean is: " + pagingBean;
                throw new SystemException(msg);
            }
            int allRecordCount;
            IList<ENTITY> selectedList;
            if (_countLater) { // not implemented about performance optimization
                selectedList = handler.Paging();
                allRecordCount = handler.Count();
            } else {
                allRecordCount = handler.Count();
                selectedList = handler.Paging();
            }
            PagingResultBean<ENTITY> rb = new ResultBeanBuilder<ENTITY>(_tableDbName).BuildPagingResultBean(pagingBean, allRecordCount, selectedList);
            if (pagingBean.CanPagingReSelect && IsNecessaryToReadPageAgain(rb)) {
                pagingBean.FetchPage(rb.AllPageCount);
                int reAllRecordCount = handler.Count();
                IList<ENTITY> reSelectedList = handler.Paging();
                return new ResultBeanBuilder<ENTITY>(_tableDbName).BuildPagingResultBean(pagingBean, reAllRecordCount, reSelectedList);
            } else {
                return rb;
            }
        }

        protected bool IsNecessaryToReadPageAgain(PagingResultBean<ENTITY> rb) {
            return rb.AllRecordCount > 0 && rb.SelectedList.Count == 0;
        }

        // ===============================================================================
        //                                                                          Option
        //                                                                          ======
        public PagingInvoker<ENTITY> CountLater() {
            _countLater = true; return this;
        }

        // ===============================================================================
        //                                                                          Helper
        //                                                                          ======
        protected void AssertObjectNotNull(String variableName, Object value) {
            if (variableName == null) {
                String msg = "The value should not be null: variableName=" + variableName + " value=" + value;
                throw new SystemException(msg);
            }
            if (value == null) {
                String msg = "The value should not be null: variableName=" + variableName;
                throw new SystemException(msg);
            }
        }
    }
}
