
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

    public class SimpleStringUtil {
    
        // ===================================================================================
        //                                                                              String
        //                                                                              ======
        public static String Replace(String text, String fromText, String toText) {
            if(text == null || fromText == null || toText == null) {
                return null;
    		}
    		return text.Replace(fromText, toText);
        }
    	
        public static String InitCap(String str) {
            AssertObjectNotNull("str", str);
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
    	
        public static String InitUncap(String str) {
            AssertObjectNotNull("str", str);
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        // -----------------------------------------------------
        //                                         Assert Object
        //                                         -------------
        protected static void AssertObjectNotNull(String variableName, Object value) {
            if (variableName == null) {
                String msg = "The value should not be null: variableName=" + variableName + " value=" + value;
                throw new SystemException(msg);
            }
            if (value == null) {
                String msg = "The value should not be null: variableName=" + variableName;
                throw new SystemException(msg);
            }
        }
    }

}
