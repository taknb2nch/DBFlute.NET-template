
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike {

    public class RuntimeException : SystemException {
        public RuntimeException(String msg)
            : base(msg) {
        }
        public RuntimeException(String msg, Exception e)
            : base(msg, e) {
        }
        public Class<RuntimeException> getClass() {
            return new Class<RuntimeException>(GetType());
        }
    }
    public class IllegalArgumentException : SystemException {
        public IllegalArgumentException(String msg)
            : base(msg) {
        }
        public IllegalArgumentException(String msg, Exception e)
            : base(msg, e) {
        }
    }
    public class IllegalStateException : SystemException {
        public IllegalStateException(String msg)
            : base(msg) {
        }
        public IllegalStateException(String msg, Exception e)
            : base(msg, e) {
        }
    }
    public class UnsupportedOperationException : SystemException {
        public UnsupportedOperationException(String msg)
            : base(msg) {
        }
    }
}
