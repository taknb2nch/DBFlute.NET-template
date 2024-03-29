
using System;
using System.Reflection;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Types;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

public class DBFluteConfig {

    // ===================================================================================
    //                                                                          Definition
    //                                                                          ==========
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly DBFluteConfig _instance = new DBFluteConfig();

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
	protected StatementConfig _defaultStatementConfig;
	protected bool _queryLogLevelInfo;
	protected bool _executeStatusLogLevelInfo;
	protected bool _useSqlLogRegistry;
    protected String _outsideSqlPackage;
    protected IDbParameterParser _dbParameterParser;
    protected String _omitDirectoryPackage = "Aaa.Bbb.Ccc";
    protected String _defaultPackage = "Aaa.Bbb.Ccc";
    protected String _omitResourcePathPackage;
    protected String _flatDirectoryPackage = "Aaa.Bbb.Ccc.DBFlute";
    protected String _omitFileSystemPathPackage;
    protected AdditionalAssembly _additionalAssemblyProvider;
    protected FileSystemPath _fileSystemPathResolver;
	protected bool _internalDebug;
	protected bool _locked = true; // at first locked

    // ===================================================================================
    //                                                                         Constructor
    //                                                                         ===========
    private DBFluteConfig() {
    }

    // ===================================================================================
    //                                                                           Singleton
    //                                                                           =========
    /**
     * Get instance.
     * @return Singleton instance. (NotNull)
     */
    public static DBFluteConfig GetInstance() {
        return _instance;
    }

    // ===================================================================================
    //                                                            Default Statement Config
    //                                                            ========================
    public StatementConfig DefaultStatementConfig {
        get {
            return _defaultStatementConfig;
        }
        set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting defaultStatementConfig: " + value);
		    }
            _defaultStatementConfig = value;
        }
    }
	
    // ===================================================================================
    //                                                                Query Log Level Info
    //                                                                ====================
	public bool IsQueryLogLevelInfo {
	    get {
	        return _queryLogLevelInfo;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting queryLogLevelInfo: " + value);
		    }
	        _queryLogLevelInfo = value;
		}
	}
	
    // ===================================================================================
    //                                                              Execute Log Level Info
    //                                                              ======================
	public bool IsExecuteStatusLogLevelInfo {
	    get {
	        return _executeStatusLogLevelInfo;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting executeStatusLogLevelInfo: " + value);
		    }
	        _executeStatusLogLevelInfo = value;
		}
	}
	
    // ===================================================================================
    //                                                                    Sql Log Registry
    //                                                                    ================
	public bool IsUseSqlLogRegistry {
	    get {
	        return _useSqlLogRegistry;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting useSqlLogRegistry: " + value);
		    }
	        _useSqlLogRegistry = value;
		}
	}
	
    // ===================================================================================
    //                                                                  OutsideSql Package
    //                                                                  ==================
	public String OutsideSqlPackage {
	    get {
	        return _outsideSqlPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting outsideSqlPackage: " + value);
		    }
	        _outsideSqlPackage = value;
		}
	}

    // ===================================================================================
    //                                                                          Value Type
    //                                                                          ==========
	public void RegisterBasicValueType(Type keyType, IValueType valueType) {
	    AssertNotLocked();
		if (_log.IsInfoEnabled) {
		    _log.Info("...Registering basic valueType: keyType=" + keyType + " valueType=" + valueType);
		}
	    ValueTypes.RegisterValueType(keyType, valueType);
	}

    // ===================================================================================
    //                                                                 DB Parameter Parser
    //                                                                 ===================
	public IDbParameterParser DbParameterParser { // CSharp only
	    get {
	        return _dbParameterParser;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting dbParameterParser: " + value);
		    }
	        _dbParameterParser = value;
		}
	}

    // ===================================================================================
    //                                                              Omit Directory Package
    //                                                              ======================
	public String OmitDirectoryPackage { // CSharp only
	    get {
	        return _omitDirectoryPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting omitDirectoryPackage: " + value);
		    }
	        _omitDirectoryPackage = value;
		}
	}

    // ===================================================================================
    //                                                                     Default Package
    //                                                                     ===============
	public String DefaultPackage { // CSharp only
	    get {
	        return _defaultPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting defaultPackage: " + value);
		    }
	        _defaultPackage = value;
		}
	}

    // ===================================================================================
    //                                                          Omit Resource Path Package
    //                                                          ==========================
	public String OmitResourcePathPackage { // CSharp only
	    get {
	        return _omitResourcePathPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting omitResourcePathPackage: " + value);
		    }
	        _omitResourcePathPackage = value;
		}
	}

    // ===================================================================================
    //                                                              Flat Directory Package
    //                                                              ======================
	public String FlatDirectoryPackage { // CSharp only
	    get {
	        return _flatDirectoryPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting flatDirectoryPackage: " + value);
		    }
	        _flatDirectoryPackage = value;
		}
	}

    // ===================================================================================
    //                                                       Omit File System Path Package
    //                                                       =============================
	public String OmitFileSystemPathPackage { // CSharp only
	    get {
	        return _omitFileSystemPathPackage;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting omitFileSystemPathPackage: " + value);
		    }
	        _omitFileSystemPathPackage = value;
		}
	}

    // ===================================================================================
    //                                                        Additional Assembly Provider
    //                                                        ============================
	public AdditionalAssembly AdditionalAssemblyProvider { // CSharp only
	    get {
	        return _additionalAssemblyProvider;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting additionalAssemblyProvider: " + value);
		    }
	        _additionalAssemblyProvider = value;
		}
	}

    // ===================================================================================
    //                                                           File System Path Resolver
    //                                                           =========================
	public FileSystemPath FileSystemPathResolver { // CSharp only
	    get {
	        return _fileSystemPathResolver;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting fileSystemPathResolver: " + value);
		    }
	        _fileSystemPathResolver = value;
		}
	}

    // ===================================================================================
    //                                                                      Internal Debug
    //                                                                      ==============
	public bool IsInternalDebug {
	    get {
	        return _internalDebug;
		}
		set {
	        AssertNotLocked();
		    if (_log.IsInfoEnabled) {
		        _log.Info("...Setting internalDebug: " + value);
		    }
	        _internalDebug = value;
		}
	}

    // ===================================================================================
    //                                                                         Config Lock
    //                                                                         ===========
	public bool IsLocked {
	    get {
	        return _locked;
		}
	}
	
	public void Lock() {
        if (_locked) {
            return;
        }
		if (_log.IsInfoEnabled) {
		    _log.Info("...Locking the config of dbflute");
		}
	    _locked = true;
	}
	
	public void Unlock() {
        if (!_locked) {
            return;
        }
		if (_log.IsInfoEnabled) {
		    _log.Info("...Unlocking the config of dbflute");
		}
	    _locked = false;
	}
	
	protected void AssertNotLocked() {
	    if (!IsLocked) {
		    return;
		}
		String msg = "The configuration of dbflute is locked.";
		throw new SystemException(msg);
	}
}

}
