
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey {

public class ConditionKeyNotInScope : ConditionKey {

    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public ConditionKeyNotInScope() {
        _conditionKey = "notInScope";
        _operand = "not in";
    }

    public override bool isValidRegistration(ConditionValue conditionValue, Object value, String callerName) {
        if (value == null) {
            return false;
        }
        if (value is System.Collections.IList && ((System.Collections.IList)value).Count == 0) {
            return false;
        }
        return true;
    }

    protected override void doAddWhereClause(List<String> conditionList, String columnName, ConditionValue value) {
        if (value.NotInScope == null) {
            return;
        }
        System.Collections.IList valueList = value.NotInScope;
        System.Collections.IList checkedValueList = new System.Collections.Generic.List<Object>();
        foreach (Object checkTargetValue in valueList) {
            if (checkTargetValue == null) {
                continue;
            }
            checkedValueList.Add(checkTargetValue);
        }
        if (checkedValueList.Count == 0) {
            return;
        }
        conditionList.add(buildBindClause(columnName, value.getNotInScopeLocation(), "('a1', 'a2')"));
    }

    protected override void doAddWhereClause(List<String> conditionList, String columnName, ConditionValue value, ConditionOption option) {
        throw new UnsupportedOperationException("doAddWhereClause with condition-option is unsupported!!!");
    }

    protected override void doSetupConditionValue(ConditionValue conditionValue, Object value, String location) {
        conditionValue.NotInScope = ((System.Collections.IList)value);
        conditionValue.setNotInScopeLocation(location);
    }
	
    protected override void doSetupConditionValue(ConditionValue conditionValue, Object value, String location, ConditionOption option) {
        throw new UnsupportedOperationException("doSetupConditionValue with condition-option is unsupported!!!");
    }
}
	
}
