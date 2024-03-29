
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause {

[System.Serializable]
public class SqlClauseOracle : AbstractSqlClause {

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    protected String _fetchScopeSelectHint = "";
    protected String _fetchScopeSqlSuffix = "";
    protected String _lockSqlSuffix = "";

    // ===================================================================================
    //                                                                         Constructor
    //                                                                         ===========
    public SqlClauseOracle(String tableName)
        : base(tableName) {}

    // ===================================================================================
    //                                                                Main Clause Override
    //                                                                ====================
    protected override String prepareUnionClause(String selectClause) {
	    // - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	    // Remove select-hint comment from select clause of union
		// for fetch-scope with union().
	    // - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        selectClause = replaceString(selectClause, SELECT_HINT, "");
        return base.prepareUnionClause(selectClause);
    }

    // ===================================================================================
    //                                                                 FetchScope Override
    //                                                                 ===================
    protected override void doFetchFirst() {
        doFetchPage();
    }

    protected override void doFetchPage() {
        RownumPagingProcessor processor = new RownumPagingProcessor(getRownumExpression()
            , isFetchStartIndexSupported(), isFetchSizeSupported()
            , getPageStartIndex(), getPageEndIndex());
        processor.processRowNumberPaging();
        _fetchScopeSelectHint = processor.getSelectHint();
        _fetchScopeSqlSuffix = processor.getSqlSuffix();
    }

    protected virtual String getRownumExpression() {
        return "rownum";
    }

    protected override void doClearFetchPageClause() {
        _fetchScopeSelectHint = "";
        _fetchScopeSqlSuffix = "";
    }

    // ===================================================================================
    //                                                                       Lock Override
    //                                                                       =============
    public override SqlClause lockForUpdate() {
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(_tableName);
        if (dbmeta.HasPrimaryKey) {
            String primaryKeyColumnName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
            _lockSqlSuffix = " for update of " + _tableName + "." + primaryKeyColumnName;
        } else {
            String randomColumnName = dbmeta.ColumnInfoList.get(0).ColumnDbName;
            _lockSqlSuffix = " for update of " + _tableName + "." + randomColumnName;
        }

        _lockSqlSuffix = " for update";
        return this;
    }

    // ===================================================================================
    //                                                                       Hint Override
    //                                                                       =============
    protected override String createSelectHint() {
        return _fetchScopeSelectHint;
    }

    protected override String createFromBaseTableHint() {
        return "";
    }

    protected override String createFromHint() {
        return "";
    }
	
    protected override String createSqlSuffix() {
        return _fetchScopeSqlSuffix + _lockSqlSuffix;
    }

    // ===================================================================================
    //                                                                 Database Dependency
    //                                                                 ===================
    public virtual SqlClause lockForUpdateNoWait() {
        lockForUpdate();
        _lockSqlSuffix = _lockSqlSuffix + " nowait";
        return this;
    }

    public virtual SqlClause lockForUpdateWait(int waitSec) {
        lockForUpdate();
        _lockSqlSuffix = _lockSqlSuffix + " wait " + waitSec;
        return this;
    }

    // [DBFlute-0.8.9.9]
    // ===================================================================================
    //                                                                       InScope Limit
    //                                                                       =============
    public override int getInScopeLimit() {
        return 1000;
    }
}
	
}
