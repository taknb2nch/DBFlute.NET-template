
using System;
using System.Collections.Generic;
using System.Text;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;

namespace Aaa.Bbb.Ccc.DBFlute.ExDao.PmBean {

    /// <summary>
    /// The parametaer-bean of MyTableExPmb.
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    [System.Serializable]
    public partial class MyTableExPmb {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected int? _age;
        protected string _attendanceFlagTrue = CDef.Flag.True.Code;
    
        // ===============================================================================
        //                                                                   Assist Helper
        //                                                                   =============
        protected String ConvertEmptyToNullIfString(String value) {
            return FilterRemoveEmptyString(value);
        }

        protected String FilterRemoveEmptyString(String value) {
            return ((value != null && !"".Equals(value)) ? value : null);
        }

        protected String FormatByteArray(byte[] bytes) {
            return "byte[" + (bytes != null ? bytes.Length.ToString() : "null") + "]";
        }

        // ===============================================================================
        //                                                                  Basic Override
        //                                                                  ==============
        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("MyTableExPmb:");
            sb.Append(xbuildColumnString());
            return sb.ToString();
        }
        private String xbuildColumnString() {
            String c = ", ";
            StringBuilder sb = new StringBuilder();
            sb.Append(c).Append(_age);
            sb.Append(c).Append(_attendanceFlagTrue);
            if (sb.Length > 0) { sb.Remove(0, c.Length); }
            sb.Insert(0, "{").Append("}");
            return sb.ToString();
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public int? Age {
            get { return _age; }
            set { _age = value; }
        }

        public string AttendanceFlagTrue {
            get { return _attendanceFlagTrue; }
        }

    }
}
