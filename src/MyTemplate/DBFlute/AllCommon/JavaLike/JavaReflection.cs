
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike {

    public class Class<TYPE> {
        protected Type _res;
        public Class(Type type) {
            _res = type;
        }
        public String getName() {
            return _res.Name;
        }
        public String getSimpleName() {
            return _res.Name.Substring(_res.Name.LastIndexOf(".") + ".".Length);
        }
    }
}
