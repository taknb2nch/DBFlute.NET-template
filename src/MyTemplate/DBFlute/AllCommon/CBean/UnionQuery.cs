
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public delegate void UnionQuery<UNION_CB>(UNION_CB unionCB) where UNION_CB : ConditionBean;
}
