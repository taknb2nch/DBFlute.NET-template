
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    public static class CDef {

        /**
         * フラグを示す
         */
        public class Flag {
            /** はい: 有効を示す */
            public static readonly Flag True = new Flag("1", "True", "はい");
            /** いいえ: 無効を示す */
            public static readonly Flag False = new Flag("0", "False", "いいえ");
            private static readonly Map<String, Flag> _codeValueMap = new LinkedHashMap<String, Flag>();
            static Flag() {
                _codeValueMap.put(True.Code.ToLower(), True);
                _codeValueMap.put(False.Code.ToLower(), False);
            }
            protected String _code; protected String _name; protected String _alias;
            public Flag(String code, String name, String alias) {
                _code = code; _name = name; _alias = alias;
            }
            public String Code { get { return _code; } }
            public String Name { get { return _name; } }
            public String Alias { get { return _alias; } }
            public static Flag CodeOf(Object code) {
                if (code == null) { return null; } if (code is Flag) { return (Flag)code; }
                return _codeValueMap.get(code.ToString().ToLower());
            }
            public static Flag[] Values { get {
                Flag[] values = new Flag[_codeValueMap.size()];
                int index = 0;
                foreach (Flag flg in _codeValueMap.values()) {
                    values[index] = flg;
                    ++index;
                }
                return values;
            }}
            public override int GetHashCode() { return 7 + _code.GetHashCode(); }
            public override bool Equals(Object obj) {
                if (!(obj is Flag)) { return false; }
                Flag cls = (Flag)obj;
                return _code.ToLower().Equals(cls.Code.ToLower());
            }
            public override String ToString() { return this.Code; }
        }

    }

}
