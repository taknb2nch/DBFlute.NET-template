
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Seasar.Framework.Util;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql {

    public class OutsideSqlContext {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        /** Log instance. */
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        // ===============================================================================
        //                                                                    Thread Local
        //                                                                    ============
        private static readonly LocalDataStoreSlot _slot = Thread.AllocateDataSlot();

        public static OutsideSqlContext GetOutsideSqlContextOnThread() {
            return (OutsideSqlContext)Thread.GetData(_slot);
        }

        public static void SetOutsideSqlContextOnThread(OutsideSqlContext outsideSqlContext) {
            if (outsideSqlContext == null) {
                String msg = "The argument[outsideSqlContext] must not be null.";
                throw new SystemException(msg);
            }
            Thread.SetData(_slot, outsideSqlContext);
        }

        public static bool IsExistOutsideSqlContextOnThread() {
            return (Thread.GetData(_slot) != null);
        }

        public static void ClearOutsideSqlContextOnThread() {
            Thread.SetData(_slot, null);
        }

        // ===============================================================================
        //                                                                      Unique Key
        //                                                                      ==========
        public static String GenerateSpecifiedOutsideSqlUniqueKey(String methodName, String path, Object pmb, OutsideSqlOption option, Type resultType) {
            String pmbKey = (pmb != null ? pmb.GetType().Name : "null");
            String resultKey = (resultType != null ? resultType.ToString() : "null");
            String tableDbName = option.TableDbName;
            String generatedUniqueKey = option.GenerateUniqueKey();
            return tableDbName + ":" + methodName + "():" + path + ":" + pmbKey + ":" + generatedUniqueKey + ":" + resultKey;
        }

        // ===============================================================================
        //                                                              Exception Handling
        //                                                              ==================
        public static void ThrowOutsideSqlNotFoundException(String path) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The outsideSql was Not Found!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the existence of your target file of outsideSql on your classpath." + GetLineSeparator();
            msg = msg + "And please confirm the file name and the file path STRICTLY!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified OutsideSql Path]" + GetLineSeparator() + path + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.OutsideSqlNotFoundException(msg);
        }

        protected static String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }

        protected static String ReplaceString(String text, String fromText, String toText) {
            return SimpleStringUtil.Replace(text, fromText, toText);
        }

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        // -------------------------------------------------
        //                                             Basic
        //                                             -----
        /// <summary>The path of outside-sql. (The mark of Specified-OutsideSql)</summary>
        protected String _outsideSqlPath;

        protected Object _parameterBean;

        protected Type _resultType;

        protected String _methodName;
        
        protected StatementConfig _statementConfig;

        protected String _tableDbName;

        // -------------------------------------------------
        //                                            Option
        //                                            ------
        protected bool _dynamicBinding;

        protected bool _offsetByCursorForcedly;

        protected bool _limitByCursorForcedly;

        protected bool _autoPagingLogging;

        // ===============================================================================
        //                                                                        Read SQL
        //                                                                        ========
        public String ReadFilteredOutsideSql(String sqlFileEncoding, String dbmsSuffix) { // Entry Point!
            String sql = ReadOutsideSql(sqlFileEncoding, dbmsSuffix);
            sql = ReplaceOutsideSqlBindCharacterOnLineComment(sql);
            return ReplaceOldStypePagingDeterminationForCompatible(sql);
        }
        
        protected String ReplaceOldStypePagingDeterminationForCompatible(String sql) {
            return sql.Replace("/*IF pmb.Paging*/", "/*IF pmb.IsPaging*/");
        }

        protected String ReplaceOutsideSqlBindCharacterOnLineComment(String sql) {
            String bindCharacter = "?";
            if (sql.IndexOf(bindCharacter) < 0) {
                return sql;
            }
            String lineSeparator = "\n";
            if (sql.IndexOf(lineSeparator) < 0) {
                return sql;
            }
            String lineCommentMark = "--";
            if (sql.IndexOf(lineCommentMark) < 0) {
                return sql;
            }
            StringBuilder sb = new StringBuilder();
            String[] lines = sql.Split(lineSeparator.ToCharArray());
            foreach (String line in lines) {
                int lineCommentIndex = line.IndexOf("--");
                if (lineCommentIndex < 0) {
                    sb.Append(line).Append(lineSeparator);
                    continue;
                }
                String lineComment = line.Substring(lineCommentIndex);
                if (lineComment.Contains("ELSE") || !lineComment.Contains(bindCharacter)) {
                    sb.Append(line).Append(lineSeparator);
                    continue;
                }
                
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Replacing bind character on line comment: " + lineComment);
                }
                String filteredLineComment = ReplaceString(lineComment, bindCharacter, "Q");
                sb.Append(line.Substring(0, lineCommentIndex)).Append(filteredLineComment).Append(lineSeparator);
            }
            return sb.ToString();
        }
    
        public String ReadOutsideSql(String sqlFileEncoding, String dbmsSuffix) { // Non Filtered
            String standardPath = _outsideSqlPath.Replace("/", "."); // Replacing is required for CSharp!
            String dbmsPath = BuildDbmsPath(standardPath, dbmsSuffix);
            Assembly asm = GetType().Assembly; // In same assembly!
            AdditionalAssembly additionalAssemblyProvider = DBFluteConfig.GetInstance().AdditionalAssemblyProvider;

            // = = = = = = = = = = = = = =
            // From file system at first!
            // = = = = = = = = = =/
            String fileSystemPath = BuildFileSystemPath(standardPath);
            String text = ReadTextFromFileSystem(fileSystemPath, sqlFileEncoding);
            if (text != null) {
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Searching from file system: (Found) " + fileSystemPath);
                }
                return text;
            } else {
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Searching from file system: (Not Found) " + fileSystemPath);
                }
            }

            // = = = = = = = = = = = = = =
            // From embedded resource next!
            // = = = = = = = = = =/
            // by standard assembly
            text = ReadOutsideSql(sqlFileEncoding, dbmsSuffix, standardPath, dbmsPath, asm);
            if (text != null) {
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Searching from embedded: (Found) " + asm.FullName);
                }
                return text;
            }

            // by additional assembly
            if (additionalAssemblyProvider != null) {
                Assembly additionalAssembly = additionalAssemblyProvider.Invoke();
                if (additionalAssembly != null && additionalAssembly != asm) {
                    text = ReadOutsideSql(sqlFileEncoding, dbmsSuffix, standardPath, dbmsPath, additionalAssembly);
                    if (text != null) {
                        if (_log.IsDebugEnabled) {
                            _log.Debug("...Searching from embedded: (Found) " + additionalAssembly.FullName);
                        }
                        return text;
                    }

                    // Re-try after resolving path for the assembly of result type. 
                    String resolvedStandardPath = ResolvePathForResultTypeAssembly(standardPath, asm, additionalAssembly);
                    String resolvedDbmsPath = ResolvePathForResultTypeAssembly(dbmsPath, asm, additionalAssembly);
                    text = ReadOutsideSql(sqlFileEncoding, dbmsSuffix, resolvedStandardPath, resolvedDbmsPath, additionalAssembly);
                    if (text != null) {
                        if (_log.IsDebugEnabled) {
                            _log.Debug("...Searching from embedded: (Found) " + resolvedStandardPath);
                        }
                        return text;
                    }
                }
            }

            // After all the outside SQL was not found!
            ThrowOutsideSqlNotFoundException(standardPath);
            return null; // Unreachable!
        }
        
        protected String ReadOutsideSql(String sqlFileEncoding, String dbmsSuffix
                                      , String standardPath, String dbmsPath, Assembly asm) {
            if (IsExistResource(dbmsPath, asm)) {
                return ReadText(dbmsPath, sqlFileEncoding, asm);
            } else if ("_postgresql".Equals(dbmsSuffix) && IsExistResource("_postgre", asm)) {
                return ReadText("_postgre", sqlFileEncoding, asm); // Patch for name difference
            } else if ("_sqlserver".Equals(dbmsSuffix) && IsExistResource("_mssql", asm)) {
                return ReadText("_mssql", sqlFileEncoding, asm); // Patch for name difference
            } else if (IsExistResource(standardPath, asm)) {
                return ReadText(standardPath, sqlFileEncoding, asm);
            }
            return null;
        }

        protected String BuildDbmsPath(String standardPath, String dbmsSuffix) {
            String dbmsPath;
            int lastIndexOfDot = standardPath.LastIndexOf(".");
            if (lastIndexOfDot >= 0 && !standardPath.Substring(lastIndexOfDot).Contains("/")) {
                String basePath = standardPath.Substring(0, lastIndexOfDot);
                dbmsPath = basePath + dbmsSuffix + standardPath.Substring(lastIndexOfDot);
            } else {
                dbmsPath = standardPath + dbmsSuffix;
            }
            return dbmsPath;
        }

        protected String ResolvePathForResultTypeAssembly(String path, Assembly asmDbflute, Assembly asmResultType) {
            String asmDbfluteRootName = System.IO.Path.GetFileNameWithoutExtension(asmDbflute.Location);
            String asmResultTypeRootName = System.IO.Path.GetFileNameWithoutExtension(asmResultType.Location);
            return path.Replace(asmDbfluteRootName, asmResultTypeRootName);
        }
  
        // ===============================================================================
        //                                                                     Set up Path
        //                                                                     ===========
        public void SetupBehaviorQueryPathIfNeeds() {
            if (!IsBehaviorQueryPathEnabled) {
                return;
            }
            if (_outsideSqlPath.Contains(":")) {
                String subDirectoryValue = _outsideSqlPath.Substring(0, _outsideSqlPath.LastIndexOf(":"));
                String subDirectoryPath = subDirectoryValue.Replace(":", "/");
                String behaviorQueryPath = _outsideSqlPath.Substring(_outsideSqlPath.LastIndexOf(":") + ":".Length);
                String behaviorClassPath = BuildBehaviorSqlPackageName().Replace(".", "/");
                String behaviorPackagePath = behaviorClassPath.Substring(0, behaviorClassPath.LastIndexOf("/"));
                String behaviorClassName = behaviorClassPath.Substring(behaviorClassPath.LastIndexOf("/") + "/".Length);
                _outsideSqlPath = behaviorPackagePath + "/" + subDirectoryPath + "/" + behaviorClassName + "_" + behaviorQueryPath + ".sql";
                _outsideSqlPath = _outsideSqlPath.Replace("/", ".");// Replacing is required for csharp!
            } else {
                _outsideSqlPath = BuildBehaviorSqlPackageName() + "_" + _outsideSqlPath + ".sql";
            }
        }

        protected String BuildBehaviorSqlPackageName() {
            DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(_tableDbName);
            String behaviorTypeName = dbmeta.BehaviorTypeName;
            String outsideSqlPackage = DBFluteConfig.GetInstance().OutsideSqlPackage;
            if (outsideSqlPackage != null && outsideSqlPackage.Trim().Length > 0) {
                String behaviorClassName = behaviorTypeName.Substring(behaviorTypeName.LastIndexOf(".") + ".".Length);
                String tmp = behaviorTypeName.Substring(0, behaviorTypeName.LastIndexOf("."));
                String exbhvName = tmp.Contains(".") ? tmp.Substring(tmp.LastIndexOf(".") + ".".Length) : tmp;
                return FilterBehaviorSqlPackageName(outsideSqlPackage + "." + exbhvName + "." + behaviorClassName);
            } else {
                return FilterBehaviorSqlPackageName(behaviorTypeName);
            }
        }

        protected String FilterBehaviorSqlPackageName(String name) { // CSharp only
            return FilterAboutDefaultPackage(FilterAboutOmitResourcePathPackage(FilterAboutOmitDirectoryPackage(name)));
        }

        // It is unnecessary of filtering about flat directory package.
        public String FilterAboutOmitDirectoryPackage(String pckge) { // CSharp only
            String omitDirectoryPackage = DBFluteConfig.GetInstance().OmitDirectoryPackage;
            if (omitDirectoryPackage != null && omitDirectoryPackage.Trim().Length > 0) {
                pckge = RemoveOmitPackage(pckge, omitDirectoryPackage);
            }
            return pckge;
        }

        public String FilterAboutOmitResourcePathPackage(String pckge) { // CSharp only
            String omitResourcePathPackage = DBFluteConfig.GetInstance().OmitResourcePathPackage;
            if (omitResourcePathPackage != null && omitResourcePathPackage.Trim().Length > 0) {
                pckge = RemoveOmitPackage(pckge, omitResourcePathPackage);
            }
            return pckge;
        }

        protected String RemoveOmitPackage(String pckge, String omitName) { // CSharp only
            if (pckge.StartsWith(omitName)) {
                return pckge.Replace(omitName + ".", "");
            } else if (pckge.EndsWith(omitName)) {
                return pckge.Replace("." + omitName, "");
            } else {
                return pckge.Replace("." + omitName + ".", ".");
            }
        }

        protected String FilterAboutDefaultPackage(String pckge) { // CSharp only
            String defaultPackage = DBFluteConfig.GetInstance().DefaultPackage;
            if (defaultPackage != null && defaultPackage.Trim().Length > 0) {
                return defaultPackage + "." + pckge;
            } else {
                return pckge;
            }
        }

        protected bool IsBehaviorQueryPathEnabled {
            get {
                if (IsProcedure) {// [DBFlute-0.8.0]
                    return false;
                }
                return _outsideSqlPath != null && !_outsideSqlPath.Contains("/") && !_outsideSqlPath.Contains(".") && _tableDbName != null;
            }
        }

        // ===============================================================================
        //                                                                     File System
        //                                                                     ===========
        protected virtual String ReadTextFromFileSystem(String fileSystemPath, String sqlFileEncoding) {
            if (!File.Exists(fileSystemPath)) {
                return null;
            }
            using (Stream stream = new FileStream(fileSystemPath, FileMode.Open)) {
                using (TextReader reader = new StreamReader(stream, Encoding.GetEncoding(sqlFileEncoding))) {
                    return reader.ReadToEnd();
                }
            }
        }

        protected virtual String BuildFileSystemPath(String standardPath) {
            String path = standardPath;
            String omitPathPackage = DBFluteConfig.GetInstance().OmitFileSystemPathPackage;
            if (omitPathPackage != null && omitPathPackage.Trim().Length > 0 && path.StartsWith(omitPathPackage)) {
                path = path.Substring(omitPathPackage.Length + ".".Length); // Remove omit package.
            } else {
                path = path.Substring(path.IndexOf(".") + ".".Length); // Remove project package.
            }
            String fileSeparator = "/";
            String flatDirectory = DBFluteConfig.GetInstance().FlatDirectoryPackage;
            if (flatDirectory != null && flatDirectory.Trim().Length > 0 && path.Contains(flatDirectory)) {
                String front = path.Substring(0, path.IndexOf(flatDirectory));
                String rear = path.Substring(path.IndexOf(flatDirectory) + flatDirectory.Length);
                front = front.Replace(".", fileSeparator);
                rear = rear.Replace(".", fileSeparator);
                path = front + flatDirectory + rear;
            } else {
                path = path.Replace(".", fileSeparator);
            }
            if (path.EndsWith(fileSeparator + "sql")) {
                path = path.Replace(fileSeparator + "sql", ".sql");
            }
            FileSystemPath resolver = DBFluteConfig.GetInstance().FileSystemPathResolver;
            if (resolver != null) {
                return resolver.Invoke(path, standardPath, this);
            }
            return path;
        }

        // ===============================================================================
        //                                                                   Determination
        //                                                                   =============
        public bool IsSpecifiedOutsideSql {
            get { return _outsideSqlPath != null; }
        }

        // [DBFlute-0.8.0]
        public bool IsProcedure {
            get { return _methodName != null && _methodName.StartsWith("Call"); }
        }

        // ===============================================================================
        //                                                                  General Helper
        //                                                                  ==============
        protected virtual bool IsExistResource(String path, Assembly asm) {
            return ResourceUtil.IsExist(path, asm);
        }

        protected virtual String ReadText(String path, String sqlFileEncoding, Assembly asm) {
            using (Stream stream = ResourceUtil.GetResourceAsStream(path, asm)) {
                using (TextReader reader = new StreamReader(stream, Encoding.GetEncoding(sqlFileEncoding))) {
                    return reader.ReadToEnd();
                }
            }
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        // -------------------------------------------------
        //                                             Basic
        //                                             -----
        public String OutsideSqlPath {
            get { return _outsideSqlPath; }
            set { this._outsideSqlPath = value; }
        }

        public Object ParameterBean {
            get { return _parameterBean; }
            set { this._parameterBean = value; }
        }

        public Type ResultType {
            get { return _resultType; }
            set { this._resultType = value; }
        }

        public String MethodName {
            get { return _methodName; }
            set { this._methodName = value; }
        }

        public StatementConfig StatementConfig {
            get { return _statementConfig; }
            set { this._statementConfig = value; }
        }

        public String TableDbName {
            get { return _tableDbName; }
            set { this._tableDbName = value; }
        }

        // -------------------------------------------------
        //                                            Option
        //                                            ------
        public bool IsDynamicBinding {
            get { return _dynamicBinding; }
            set { this._dynamicBinding = value; }
        }

        public bool IsOffsetByCursorForcedly {
            get { return _offsetByCursorForcedly; }
            set { this._offsetByCursorForcedly = value; }
        }
        
        public bool IsLimitByCursorForcedly {
            get { return _limitByCursorForcedly; }
            set { this._limitByCursorForcedly = value; }
        }

        public bool IsAutoPagingLogging { // for logging
            get { return _autoPagingLogging; }
            set { this._autoPagingLogging = value; }
        }
    }

    public delegate Assembly AdditionalAssembly();
    public delegate String FileSystemPath(String relativePath, String standardPath, OutsideSqlContext context);
}
