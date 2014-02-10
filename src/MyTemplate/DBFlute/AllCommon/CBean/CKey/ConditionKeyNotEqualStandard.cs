
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey {

public class ConditionKeyNotEqualStandard : ConditionKeyNotEqual {

    protected override String defineOperand() {
        return "<>"; // is SQL standard operand
    }
}
	
}
