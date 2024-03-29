
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause {

[System.Serializable]
public class OrderByElement {

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    /** The value of alias name. */
    protected String _aliasName;

    /** The value of column name. */
    protected String _columnName;

    /** The value of registered alias name. */
    protected String _registeredAliasName;

    /** The value of registered column name. */
    protected String _registeredColumnName;

    /** The value of ascDesc. */
    protected String _ascDesc = "asc";

	/** The setupper of order-by nulls. */
	protected OrderByNullsSetupper _orderByNullsSetupper;
	
	/** Is nulls ordered first? */
	protected bool _nullsFirst;

    // ===================================================================================
    //                                                                         Main Method
    //                                                                         ===========
    public void setupAsc() {
        _ascDesc = "asc";
    }

    public void setupDesc() {
        _ascDesc = "desc";
    }

    public void reverse() {
        if (_ascDesc == null) {
            String msg = "The attribute[ascDesc] should not be null.";
            throw new IllegalStateException(msg);
        }
        if (_ascDesc.Equals("asc")) {
            _ascDesc = "desc";
        } else if (_ascDesc.Equals("desc")) {
            _ascDesc = "asc";
        } else {
            String msg = "The attribute[ascDesc] should be asc or desc: but ascDesc=" + _ascDesc;
            throw new IllegalStateException(msg);
        }
    }

    public bool isAsc() {
        if (_ascDesc == null) {
            String msg = "The attribute[ascDesc] should not be null.";
            throw new IllegalStateException(msg);
        }
        if (_ascDesc.Equals("asc")) {
            return true;
        } else if (_ascDesc.Equals("desc")) {
            return false;
        } else {
            String msg = "The attribute[ascDesc] should be asc or desc: but ascDesc=" + _ascDesc;
            throw new IllegalStateException(msg);
        }
    }

    public String getColumnFullName() {
        StringBuilder sb = new StringBuilder();
        if (_aliasName != null) {
            sb.append(_aliasName).append(".");
        }
        if (_columnName == null) {
            String msg = "The attribute[columnName] should not be null.";
            throw new IllegalStateException(msg);
        }
        sb.append(_columnName);
        return sb.toString();
    }

    public String getRegisteredColumnFullName() {
        StringBuilder sb = new StringBuilder();
        if (_registeredAliasName != null) {
            sb.append(_registeredAliasName).append(".");
        }
        if (_registeredColumnName == null) {
            String msg = "The attribute[registeredColumnName] should not be null.";
            throw new IllegalStateException(msg);
        }
        sb.append(_registeredColumnName);
        return sb.toString();
    }

    public String getElementClause() {
        if (_ascDesc == null) {
            String msg = "The attribute[ascDesc] should not be null.";
            throw new IllegalStateException(msg);
        }
        StringBuilder sb = new StringBuilder();
        sb.append(getColumnFullName()).append(" ").append(_ascDesc);
		if (_orderByNullsSetupper != null) {
		    return _orderByNullsSetupper.setup(getColumnFullName(), sb.toString(), _nullsFirst);
		} else {
            return sb.toString();
		}
    }
	
    public String getElementClause(Map<String, String> selectClauseRealColumnAliasMap) {
        if (selectClauseRealColumnAliasMap == null) {
            String msg = "The argument[selectClauseRealColumnAliasMap] should not be null.";
            throw new IllegalArgumentException(msg);
        }
        if (_ascDesc == null) {
            String msg = "The attribute[ascDesc] should not be null.";
            throw new IllegalStateException(msg);
        }
        StringBuilder sb = new StringBuilder();
        String columnAlias = selectClauseRealColumnAliasMap.get(getColumnFullName());
        if (columnAlias == null || columnAlias.Trim().Length == 0) {
            String msg = "The selectClauseRealColumnAliasMap should have the value of the key: " + getColumnFullName();
            throw new IllegalStateException(msg + " But the map is " + selectClauseRealColumnAliasMap);
        }
        sb.append(columnAlias).append(" ").append(_ascDesc);
		if (_orderByNullsSetupper != null) {
		    return _orderByNullsSetupper.setup(columnAlias, sb.toString(), _nullsFirst);
		} else {
            return sb.toString();
		}
    }

    // ===================================================================================
    //                                                                      Basic Override
    //                                                                      ==============
    public override String ToString() {
        StringBuilder sb = new StringBuilder();
        sb.append("aliasName=").append(_aliasName);
        sb.append(" columnName=").append(_columnName);
        sb.append(" registeredAliasName=").append(_registeredAliasName);
        sb.append(" registeredColumnName=").append(_registeredColumnName);
        sb.append(" ascDesc=").append(_ascDesc);
        return sb.toString();
    }
	
    // ===================================================================================
    //                                                                            Accessor
    //                                                                            ========	
    public String getAliasName() {
        return _aliasName;
    }
    public void setAliasName(String aliasName) {
        _aliasName = aliasName;
    }
    public String getColumnName() {
        return _columnName;
    }
    public void setColumnName(String columnName) {
        _columnName = columnName;
    }
    public String getRegisteredAliasName() {
        return _registeredAliasName;
    }
    public void setRegisteredAliasName(String registeredAliasName) {
        _registeredAliasName = registeredAliasName;
    }
    public String getRegisteredColumnName() {
        return _registeredColumnName;
    }
    public void setRegisteredColumnName(String registeredColumnName) {
        _registeredColumnName = registeredColumnName;
    }
    public String getAscDesc() {
        return _ascDesc;
    }
    public void setAscDesc(String ascDesc) {
        _ascDesc = ascDesc;
    }
    public void setOrderByNullsSetupper(OrderByNullsSetupper value, bool nullsFirst) {
        _orderByNullsSetupper = value;
		_nullsFirst = nullsFirst;
    }
}

}
