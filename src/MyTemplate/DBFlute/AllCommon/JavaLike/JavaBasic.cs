
using System;
using System.Text;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike {

    public class JString {
        System.String _res;
        public JString(System.String str) {
            _res = str;
        }
        public JString trim() {
            return new JString(_res.Trim());
        }
        public int length() {
            return _res.Length;
        }
        public String getString() {
            return _res;
        }
        public static JString operator +(JString a, JString b) {
            return new JString(a.getString() + b.getString());
        }
    }
	
    public class StringBuilder {
        System.Text.StringBuilder _res;
        public StringBuilder() {
            _res = new System.Text.StringBuilder();
        }
        public StringBuilder(int size) {
            _res = new System.Text.StringBuilder(size);
        }
        public StringBuilder append(Object obj) {
            _res.Append(obj != null ? obj.ToString() : "null");
            return this;
        }
        public StringBuilder insert(int offset, Object obj) {
            _res.Insert(offset, obj);
            return this;
        }
        public StringBuilder delete(int start, int end) {
            _res.Remove(start, end - start);
            return this;
        }
        public int length() {
            return _res.Length;
        }
        public String toString() {
            return _res.ToString();
        }
        public override String ToString() {
            return toString();
        }
        public static String operator +(StringBuilder a, StringBuilder b) {
            return a.toString() + b.toString();
        }
    }
}
