
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp {

    /// <summary>
    /// The exception of when the value of embedded value is null.
    /// </summary>
    public class EmbeddedValueParameterNullValueException : SystemException {

        public EmbeddedValueParameterNullValueException(String msg)
        : base(msg) {}
    }
}
