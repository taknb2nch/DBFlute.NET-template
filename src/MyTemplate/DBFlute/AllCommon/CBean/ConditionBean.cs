
using System;
using System.Collections;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public interface ConditionBean : PagingBean {

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        String TableDbName { get; }

        // ===============================================================================
        //                                                                          DBMeta
        //                                                                          ======
        DBMeta DBMeta { get; }

        // ===============================================================================
        //                                                                       SqlClause
        //                                                                       =========
        SqlClause SqlClause { get; }
        String GetClause();

        // ===============================================================================
        //                                                             PrimaryKey Handling
        //                                                             ===================
        ConditionBean AddOrderBy_PK_Asc();
        ConditionBean AddOrderBy_PK_Desc();
		
        // ===============================================================================
        //                                                                           Query
        //                                                                           =====
        ConditionQuery LocalCQ { get; }

        // ===============================================================================
        //                                                                    Lock Setting
        //                                                                    ============
        ConditionBean LockForUpdate();

        // ===============================================================================
        //                                                                    Select Count
        //                                                                    ============
        ConditionBean xsetupSelectCountIgnoreFetchScope();
        ConditionBean xafterCareSelectCountIgnoreFetchScope();
        bool IsSelectCountIgnoreFetchScope();

        // ===============================================================================
        //                                                                    InvalidQuery
        //                                                                    ============
        void CheckInvalidQuery();

        // ===============================================================================
        //                                                                Statement Config
        //                                                                ================
        void Configure(StatementConfig statementConfig);
        StatementConfig StatementConfig { get; }

        // ===============================================================================
        //                                                                  Entity Mapping
        //                                                                  ==============
        void DisableRelationMappingCache();
        bool CanRelationMappingCache();

        // ===============================================================================
        //                                                                     Display SQL
        //                                                                     ===========
    	String ToDisplaySql();

        // ===============================================================================
        //                                                      Basic Status Determination
        //                                                      ==========================
    	bool HasWhereClause();
    	bool HasOrderByClause();
        bool HasUnionQueryOrUnionAllQuery();
        
        // ===============================================================================
        //                                                              Query Synchronizer
        //                                                              ==================
        void xregisterUnionQuerySynchronizer(UnionQuery<ConditionBean> unionQuerySynchronizer);
    }
}
