
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

public class QLog {

    // ===================================================================================
    //                                                                          Definition
    //                                                                          ==========
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    // ===================================================================================
    //                                                                             Logging
    //                                                                             =======
	public static void Log(String sql) {// Very Internal
		if (IsQueryLogLevelInfo()) {
	        _log.Info(sql);
		} else {
	        _log.Debug(sql);
		}
	}
	
	public static bool IsLogEnabled() {
		if (IsQueryLogLevelInfo()) {
	        return _log.IsInfoEnabled;
		} else {
	        return _log.IsDebugEnabled;
		}
	}
	
	protected static bool IsQueryLogLevelInfo() {
	    return DBFluteConfig.GetInstance().IsQueryLogLevelInfo;
	}
}

}
