
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp {

    /// <summary>
    /// The exception of when the condition of IF comment is not found about outsideSql.
    /// </summary>
    public class IfCommentConditionNotFoundException : SystemException {

        public IfCommentConditionNotFoundException(String msg)
        : base(msg) {}
    }
}
