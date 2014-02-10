
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

public class DfSystemUtil {

    // ===================================================================================
    //                                                                              System
    //                                                                              ======
    public static String getLineSeparator() {
        // /- - - - - - - - - - - - - - - - - - - - - -
        // Because 'CR + LF' caused many trouble!
		// And Now 'LF' have little trouble. 
		// 
        // return Environment.NewLine;
        // - - - - - - - - - -/
		return "\n";
    }
}

}
