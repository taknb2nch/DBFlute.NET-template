
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause {

[System.Serializable]
public class SqlClauseSqlServer : AbstractSqlClause {

    protected String _fetchFirstSelectHint = "";
    protected String _lockFromHint = "";

    public SqlClauseSqlServer(String tableName)
        : base(tableName) {}

    protected override bool isUnionNormalSelectEnclosingRequired() {
        return true;
    }

    protected override void appendSelectHint(StringBuilder sb) {
        if (needsUnionNormalSelectEnclosing()) {
            return; // because clause should be enclosed when union normal select
        }
        base.appendSelectHint(sb);
    }

	protected override OrderByNullsSetupper createOrderByNullsSetupper() {
	    return new OrderByNullsSetupperByCaseWhen();
	}
	
    protected override void doFetchFirst() {
        if (this.isFetchSizeSupported()) {
            _fetchFirstSelectHint = " top " + this.getFetchSize();
        }
    }

    protected override void doFetchPage() {
        if (this.isFetchSizeSupported()) {
            if (this.isFetchStartIndexSupported()) {
                _fetchFirstSelectHint = " top " + this.getFetchSize();
            } else {
                _fetchFirstSelectHint = " top " + this.getPageEndIndex();
            }
        }
    }

    protected override void doClearFetchPageClause() {
        _fetchFirstSelectHint = "";
    }

    public override bool isFetchStartIndexSupported() {
        return false; // Default
    }

    public override SqlClause lockForUpdate() {
        _lockFromHint = " with (updlock)";
        return this;
    }

    protected override String createSelectHint() {
        return _fetchFirstSelectHint;
    }

    protected override String createFromBaseTableHint() {
        return _lockFromHint;
    }

    protected override String createFromHint() {
        return "";
    }

    protected override String createSqlSuffix() {
        return "";
    }
}

}
