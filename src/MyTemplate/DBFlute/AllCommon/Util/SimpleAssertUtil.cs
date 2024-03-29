
using System;
using System.Text;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

    public class SimpleAssertUtil {
    
        // ===================================================================================
        //                                                                              Assert
        //                                                                              ======
        // -----------------------------------------------------
        //                                         Assert Object
        //                                         -------------
        public static void AssertObjectNotNull(String variableName, Object value) {
            if (variableName == null) {
                String msg = "The value should not be null: variableName=" + variableName + " value=" + value;
                throw new SystemException(msg);
            }
            if (value == null) {
                String msg = "The value should not be null: variableName=" + variableName;
                throw new SystemException(msg);
            }
        }
    
        // -----------------------------------------------------
        //                                         Assert String
        //                                         -------------
        public static void AssertStringNotNullAndNotTrimmedEmpty(String variableName, String value) {
            AssertObjectNotNull("variableName", variableName);
            AssertObjectNotNull("value", value);
            if (value.Trim().Length == 0) {
                String msg = "The value should not be empty: variableName=" + variableName + " value=" + value;
                throw new SystemException(msg);
            }
        }
    }
}
