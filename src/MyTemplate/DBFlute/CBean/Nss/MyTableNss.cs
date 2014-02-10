
using System;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.Nss {

    public class MyTableNss {

        protected MyTableCQ _query;
        public MyTableNss(MyTableCQ query) { _query = query; }
        public bool HasConditionQuery { get { return _query != null; } }

        // ===============================================================================
        //                                                       With Nested Foreign Table
        //                                                       =========================

        // ===============================================================================
        //                                                      With Nested Referrer Table
        //                                                      ==========================
    }
}
