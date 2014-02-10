
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public delegate void SpecifyQuery<CB>(CB cb) where CB : ConditionBean;
}
