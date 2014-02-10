
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Annotation {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OutsideSql : Attribute {
        public OutsideSql() {
        }
    }
}
