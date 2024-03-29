
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.Util {

    public class InternalBindVariableUtil {
    
    	// ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        protected static readonly IDbParameterParser _parser = new BasicDbParameterParser();
        protected static readonly string sqlLogDateFormat = "yyyy-MM-dd";
        protected static readonly string sqlLogDateTimeFormat = "yyyy-MM-dd HH.mm.ss";
		
    	// ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        private InternalBindVariableUtil() {
        }

    	// ===============================================================================
        //                                                                     CompleteSql
        //                                                                     ===========
        public static String GetCompleteSql(String sql, Object[] args) {
            if (args == null || args.Length == 0) {
                return sql;
            }
            _parser.Parse(sql);
            return ReplaceSql(sql, args);
        }

        protected static String ReplaceSql(string sql, object[] args) {
            StringBuilder text = new StringBuilder(sql);
            for (int startIndex = 0, argsIndex = 0; argsIndex < args.Length; ++argsIndex) {
                Match match = _parser.Match(text.ToString(), startIndex);
                if (!match.Success) {
                    break;
                }
                string newValue = GetBindVariableText(args[argsIndex]);
                text.Replace(match.Value, newValue, match.Index, match.Length);
                startIndex = match.Index + newValue.Length;
            }
            return text.ToString();
        }

        public static String GetBindVariableText(Object bindVariable) {
            if (bindVariable is INullable) {
                INullable nullable = bindVariable as INullable;
                if (nullable.IsNull) {
                    return GetBindVariableText(null);
                } else {
                    PropertyInfo pi = bindVariable.GetType().GetProperty("Value");
                    return GetBindVariableText(pi.GetValue(bindVariable, null));
                }
            } else if (bindVariable is string) {
                return "'" + bindVariable + "'";
            } else if (bindVariable == null) {
                return "null";
            } else if (bindVariable.GetType().IsPrimitive) {
                return bindVariable.ToString();
            } else if (bindVariable is decimal) {
                return bindVariable.ToString();
            } else if (bindVariable is DateTime) {
                if ((DateTime) bindVariable == ((DateTime) bindVariable).Date) {
                    return "'" + ((DateTime) bindVariable).ToString(sqlLogDateFormat) + "'";
                } else {
                    return "'" + ((DateTime) bindVariable).ToString(sqlLogDateTimeFormat) + "'";
                }
            } else if (bindVariable is bool) {
                return bindVariable.ToString();
            } else if (bindVariable.GetType().IsEnum) {
                object o = Convert.ChangeType(bindVariable, Enum.GetUnderlyingType(bindVariable.GetType()));
                return o.ToString();
            } else {
                return "'" + bindVariable + "'";
            }
        }
	}
}
