
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public delegate void ScalarQuery<CB>(CB cb) where CB : ConditionBean;
}
