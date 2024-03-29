
using System;
using System.Collections.Generic;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public class SimplePagingBean : PagingBean, MapParameterBean {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected readonly SqlClause _sqlClause;
        protected int _safetyMaxResultSize;
        protected bool _paging = true;
        protected bool _countLater;
        protected bool _canPagingReSelect = true;
        protected bool _fetchNarrowing = true;
        protected IDictionary<String, Object> _parameterMap;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public SimplePagingBean() {
            _sqlClause = new SqlClausePostgreSql("dummy");
        }

        // ===============================================================================
        //                                                    Implementation of PagingBean
        //                                                    ============================
        // -------------------------------------------------
        //                              Paging Determination
        //                              --------------------
        // * * * * * * * *
        // For SQL Comment
        // * * * * * * * *
        public virtual bool IsPaging { get { return _paging; } }

        // * * * * * * * *
        // For Framework
        // * * * * * * * *
        public virtual bool IsCountLater { get { return _countLater; } }

        // -------------------------------------------------
        //                                    Paging Setting
        //                                    --------------
        public void XSetPaging(bool paging) {// Very Internal!
            if (paging) {
                this.SqlClause.makeFetchScopeEffective();
            } else {
                this.SqlClause.ignoreFetchScope();
            }
            this._paging = paging;
        }

		public void Paging(int pageSize, int pageNumber) {
		    FetchFirst(pageSize);
		    FetchPage(pageNumber);
		}

        public void DisablePagingReSelect() { _canPagingReSelect = false; }
        public virtual bool CanPagingReSelect { get { return _canPagingReSelect; } }
        
        // -------------------------------------------------
        //                                     Fetch Setting
        //                                     -------------
        public PagingBean FetchFirst(int fetchSize) {
            this.SqlClause.fetchFirst(fetchSize);
            return this;
        }

        public PagingBean FetchScope(int fetchStartIndex, int fetchSize) {
            this.SqlClause.fetchScope(fetchStartIndex, fetchSize);
            return this;
        }

        public PagingBean FetchPage(int fetchPageNumber) {
            this.SqlClause.fetchPage(fetchPageNumber);
            return this;
        }

        // -------------------------------------------------
        //                                    Fetch Property
        //                                    --------------
        public int FetchStartIndex { get { return this.SqlClause.getFetchStartIndex(); } }
        public int FetchSize { get { return this.SqlClause.getFetchSize(); } }
        public int FetchPageNumber { get { return this.SqlClause.getFetchPageNumber(); } }
        public int PageStartIndex { get { return this.SqlClause.getPageStartIndex(); } }
        public int PageEndIndex { get { return this.SqlClause.getPageEndIndex(); } }
        public bool IsFetchScopeEffective { get { return this.SqlClause.isFetchScopeEffective(); } }

        // -------------------------------------------------
        //                                     Hint Property
        //                                     -------------
        public String SelectHint { get { return this.SqlClause.getSelectHint(); } }
        public String FromBaseTableHint { get { return this.SqlClause.getFromBaseTableHint(); } }
        public String FromHint { get { return this.SqlClause.getFromHint(); } }
        public String SqlSuffix { get { return this.SqlClause.getSqlSuffix(); } }
        public String OrderByClause { get { return this.SqlClause.getOrderByClause(); } }

        // ===============================================================================
        //                                            Implementation of FetchNarrowingBean
        //                                            ====================================
        public int FetchNarrowingSkipStartIndex { get { return this.SqlClause.getFetchNarrowingSkipStartIndex(); } }
        public int FetchNarrowingLoopCount { get { return this.SqlClause.getFetchNarrowingLoopCount(); } }
        public bool IsFetchNarrowingSkipStartIndexEffective { get { return !this.SqlClause.isFetchStartIndexSupported(); } }
        public bool IsFetchNarrowingLoopCountEffective { get { return !this.SqlClause.isFetchSizeSupported(); } }
        public bool IsFetchNarrowingEffective { get { return this.SqlClause.isFetchNarrowingEffective(); } }
        public void IgnoreFetchNarrowing() { _fetchNarrowing = false; }
        public void RestoreIgnoredFetchNarrowing() { _fetchNarrowing = true; }
        public int SafetyMaxResultSize { get { return _safetyMaxResultSize; } }

        // ===============================================================================
        //                                                   Implementation of OrderByBean
        //                                                   =============================
        public OrderByClause SqlComponentOfOrderByClause {
            get { return this.SqlClause.getSqlComponentOfOrderByClause(); }
        }

        public OrderByBean ClearOrderBy() {
            this.SqlClause.clearOrderBy();
            return this;
        }

        public OrderByBean IgnoreOrderBy() {
            this.SqlClause.ignoreOrderBy();
            return this;
        }

        public OrderByBean MakeOrderByEffective() {
            this.SqlClause.makeOrderByEffective();
            return this;
        }

        // ===============================================================================
        //                                                Implementation of SelectResource
        //                                                ================================
        public void CheckSafetyResult(int safetyMaxResultSize) {
            this._safetyMaxResultSize = safetyMaxResultSize;
        }

        // ===============================================================================
        //                                              Implementation of MapParameterBean
        //                                              ==================================
        public IDictionary<String, Object> ParameterMap {
            get {
                if (_parameterMap == null) {
                    _parameterMap = new Dictionary<String, Object>();
                }
                return _parameterMap;
            }
        }

        public void AddParameter(String key, Object value) {
            if (_parameterMap == null) {
                _parameterMap = new Dictionary<String, Object>();
            }
            _parameterMap.Add(key, value);
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        protected SqlClause SqlClause {
            get { return _sqlClause; }
        }
    }
}
