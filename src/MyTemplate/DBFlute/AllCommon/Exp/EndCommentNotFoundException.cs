
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp {

    /// <summary>
    /// The exception of when the end comment is not found about outsideSql.
    /// </summary>
    public class EndCommentNotFoundException : SystemException {

        public EndCommentNotFoundException(String msg)
        : base(msg) {}
    }
}
