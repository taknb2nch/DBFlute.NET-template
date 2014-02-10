
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey {

public class ConditionKeyNotLikeSearch : ConditionKey {

    public ConditionKeyNotLikeSearch() {
        _conditionKey = "notLikeSearch";
        _operand = "not like";
    }

    public override bool isValidRegistration(ConditionValue conditionValue, Object value, String callerName) {
        if (value == null) {
            return false;
        }
        return true;
    }

    protected override void doAddWhereClause(List<String> conditionList, String columnName, ConditionValue value) {
        throw new UnsupportedOperationException("doAddWhereClause without condition-option is unsupported!!!");
    }

    protected override void doAddWhereClause(List<String> conditionList, String columnName, ConditionValue value, ConditionOption option) {
        if (option == null) {
            String msg = "The argument[option] should not be null: columnName=" + columnName + " value=" + value;
            throw new IllegalArgumentException(msg);
        }
        if (!(option is LikeSearchOption)) {
            String msg = "The argument[option] should be LikeSearchOption: columnName=" + columnName + " value=" + value;
            throw new IllegalArgumentException(msg);
        }
        LikeSearchOption myOption = (LikeSearchOption)option;
        conditionList.add(buildBindClauseWithRearOption(columnName, value.getNotLikeSearchLocation(), myOption.getRearOption()));
    }

    protected override void doSetupConditionValue(ConditionValue conditionValue, Object value, String location) {
        throw new UnsupportedOperationException("doSetupConditionValue without condition-option is unsupported!!!");
    }

    protected override void doSetupConditionValue(ConditionValue conditionValue, Object value, String location, ConditionOption option) {
        conditionValue.setNotLikeSearch((String)value, (LikeSearchOption)option).setNotLikeSearchLocation(location);
    }
}
	
}
