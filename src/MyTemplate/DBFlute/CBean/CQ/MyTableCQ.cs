
using System;
using System.Collections;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.CBean.CQ.BS;

namespace Aaa.Bbb.Ccc.DBFlute.CBean.CQ {

    [System.Serializable]
    public class MyTableCQ : BsMyTableCQ {

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        // You should NOT touch with this constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="childQuery">Child query as interface. (NullAllowed: If null, this is base instance.)</param>
        /// <param name="sqlClause">SQL clause instance. (NotNull)</param>
        /// <param name="aliasName">My alias name. (NotNull)</param>
        /// <param name="nestLevel">Nest level.</param>
        public MyTableCQ(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel)
            : base(childQuery, sqlClause, aliasName, nestLevel) {}

        // ===============================================================================
        //                                                                   Arrange Query
        //                                                                   =============
        // You can make your arranged query methods here.
        //public void ArrangeXxx() {
        //    ...
        //}
    }
}
