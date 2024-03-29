
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Seasar.Framework.Aop;
using Seasar.Framework.Aop.Interceptors;
using Seasar.Framework.Beans;
using Seasar.Dao;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao {

    public class S2DaoInterceptor : AbstractInterceptor {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        private IDaoMetaDataFactory _daoMetaDataFactory;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public S2DaoInterceptor(IDaoMetaDataFactory daoMetaDataFactory) {
            _daoMetaDataFactory = daoMetaDataFactory;
        }
		
        // ===============================================================================
        //                                                              Execute Status Log
        //                                                              ==================
        protected void Log(String msg) {
            XLog.Log(msg);
    	}
    	
        protected bool IsLogEnabled() {
    	    return XLog.IsLogEnabled();
    	}

        // ===============================================================================
        //                                                                          Invoke
        //                                                                          ======
        public override object Invoke(IMethodInvocation invocation) {
            InitializeContext();
            try {
                return DispatchInvoking(invocation);
            } finally {
                CloseContext();
            }
        }

        protected virtual object DispatchInvoking(IMethodInvocation invocation) {
            MethodBase method = invocation.Method;
            if (!method.IsAbstract) {
                return invocation.Proceed();
            }
            bool logEnabled = IsLogEnabled();
        
            // - - - - - - - - - - - - -
            // Initialize DAO meta data
            // - - - - - - - - - - - - -
            if (method.Name.Equals("InitializeDaoMetaData")) {
                InitializeSqlCommand(invocation);
                return null; // The end! (Initilization Only)
            }

            // - - - - - - - - - - -
            // Preprocess outsideSql
            // - - - - - - - - - - -
            PreprocessOutsideSql(invocation);

            // - - - - - - - - - - - - -
            // Preprocess conditionBean
            // - - - - - - - - - - - - -
            ConditionBean cb = PreprocessConditionBean(invocation);

            // - - - - - - - - -
            // Set up sqlCommand
            // - - - - - - - - -
            ISqlCommand cmd = null;
            try {
                DateTime? beforeCmd = null;
                if (logEnabled) {
                    beforeCmd = DateTime.Now;
                }
				cmd = FindSqlCommand(invocation);
                if (logEnabled) {
                    DateTime afterCmd = DateTime.Now;
                    if (!afterCmd.Equals(beforeCmd.Value)) {
                        LogSqlCommand(invocation, cmd, beforeCmd.Value, afterCmd);
                    }
                }
            } finally {
                if (IsLogEnabled()) {
                    LogInvocation(invocation);
                }
			}

            SqlResultHandler sqlResultHandler = GetSqlResultHander();
            bool existsSqlResultHandler = sqlResultHandler != null;
            DateTime? before = null;
            if (logEnabled || existsSqlResultHandler) {
                before = DateTime.Now; // for performance view
            }

            // - - - - - - - - - -
            // Execute sqlCommand!
            // - - - - - - - - - -
            object ret = null;
            try {
                ret = cmd.Execute(invocation.Arguments);
            } catch (Exception e) {
                if (e.GetType().Equals(typeof(NotSingleRowUpdatedRuntimeException))) {
                    throw new EntityAlreadyUpdatedException((NotSingleRowUpdatedRuntimeException)e);
                }
                throw;
            } finally {
                PostprocessConditionBean(invocation, cb);
            }

            // - - - - - - - - - -
            // Convert and Return!
            // - - - - - - - - - -
            Type retType = ((MethodInfo) method).ReturnType;
            ret = Seasar.Framework.Util.ConversionUtil.ConvertTargetType(ret, retType);

            if (logEnabled || existsSqlResultHandler) {
                DateTime after = DateTime.Now; // for performance view
                if (logEnabled) {
                    LogReturn(invocation, retType, ret, before.Value, after);
                }
                if (existsSqlResultHandler) {
                    SqlResultInfo info = new SqlResultInfo();
                    info.Result = ret;
                    info.CommandName = method.Name;
                    info.DisplaySql = (String)InternalMapContext.GetObject("df:DisplaySql");
                    info.BeforeDateTime = before.Value;
                    info.AfterDateTime = after;
                    sqlResultHandler.Invoke(info);
                }
            }
            return ret;
        }

        // ===============================================================================
        //                                                                      SqlCommand
        //                                                                      ==========
        protected void InitializeSqlCommand(IMethodInvocation invocation) {
            Type targetType = GetComponentDef(invocation).ComponentType;
            IDaoMetaData dmd = _daoMetaDataFactory.GetDaoMetaData(targetType);
            if (typeof(OutsideSqlDao).IsAssignableFrom(targetType)) {
                return; // Do nothing!
            } else {
                Object[] arguments = invocation.Arguments;
                if (arguments != null && arguments.Length > 0 && arguments[0] is String) {
                    String methodName = (String)arguments[0];
                    try {
                        dmd.GetSqlCommand(methodName);
                    } catch (MethodNotFoundRuntimeException ignored) {
                        // Do nothing!
                        if (IsLogEnabled()) {
                            Log("Not Found the method: " + methodName + " msg=" + ignored.Message);
                        }
                    }
                    return;
                } else {
                    String msg = "The method should have one string argument as method name: " + invocation;
                    throw new SystemException(msg);
                }
            }
        }

        protected ISqlCommand FindSqlCommand(IMethodInvocation invocation) {
            ISqlCommand cmd;
            Type targetType = GetComponentDef(invocation).ComponentType;
            IDaoMetaData dmd = _daoMetaDataFactory.GetDaoMetaData(targetType);
            if (typeof(OutsideSqlDao).IsAssignableFrom(targetType)) {
                cmd = dmd.GetSqlCommand(GenerateSpecifiedOutsideSqlUniqueKey(invocation));
            } else {
                cmd = dmd.GetSqlCommand(invocation.Method.Name);
            }
            return cmd;
        }

        protected String GenerateSpecifiedOutsideSqlUniqueKey(IMethodInvocation invocation) {
            Object[] args = invocation.Arguments;
            String path = (String)args[0];
            Object pmb = args[1];
            OutsideSqlOption option = (OutsideSqlOption)args[2];
            Type resultType = null;
            if (args.Length > 3) {
                resultType = args[3] is Type ? (Type)args[3] : args[3].GetType();
            }
            return OutsideSqlContext.GenerateSpecifiedOutsideSqlUniqueKey(invocation.Method.Name, path, pmb, option, resultType);
        }

        // ===============================================================================
        //                                                                  Log Invocation
        //                                                                  ==============
        protected void LogInvocation(IMethodInvocation invocation) {
            StackTrace stackTrace = new StackTrace();
            MethodBase method = invocation.Method;
            InvokeNameExtractingResult behaviorResult = null;
            try {
                behaviorResult = extractBehaviorInvokeName(stackTrace);
            } catch (Exception ignored) {
                Log("Failed to extract behavior name: msg=" + ignored.Message);
            }
            String invokeName = null;
            if (behaviorResult != null && behaviorResult.getSimpleClassName() != null) {
                String invokeClassName = behaviorResult.getSimpleClassName();
                String invokeMethodName = behaviorResult.getMethodName();
                invokeName = buildInvocationExpressionWithoutKakko(invocation, invokeClassName, invokeMethodName);
            } else {
                invokeName = BuildSimpleExpressionFromDaoName(method.DeclaringType.Name) + "." + method.Name;
            }

            // Save behavior invoke name for error message.
            PutObjectToMapContext("df:BehaviorInvokeName", invokeName + "()");

            int length = invokeName.Length;
            StringBuilder sb = new StringBuilder();
            for (int i=0; i < length; i++) {
                sb.Append("=");
            }
            Log("/=====================================================" + sb.ToString() + "==");
            Log("                                                      " + invokeName + "()");
            Log("                                                      " + sb.ToString() + "=/");

            LogPath(invocation, stackTrace, behaviorResult);

            // Specified OutsideSql
            if (IsSpecifiedOutsideSql(invocation)) {
                Object[] args = invocation.Arguments;
                OutsideSqlContext outsideSqlContext = GetOutsideSqlContext();
                if (outsideSqlContext != null) {
                    Log("path: " + outsideSqlContext.OutsideSqlPath);
                } else {
                    Log("path: " + GetOutsideSqlPath(args));
				}
                Log("option: " + GetOutsideSqlOption(args));
            }
        }

        protected String buildInvocationExpressionWithoutKakko(IMethodInvocation invocation, String invokeClassName, String invokeMethodName) {
            if (invokeClassName.Contains("OutsideSql") && invokeClassName.Contains("Executor")) { // OutsideSql Executor Handling and CSharp Only Contains
                try {
                    String originalName = invokeClassName;
                    if (IsSpecifiedOutsideSql()) {
                        OutsideSqlContext outsideSqlContext = GetOutsideSqlContext();
                        String tableDbName = outsideSqlContext.TableDbName;
                        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(tableDbName);
                        String behaviorTypeName = dbmeta.BehaviorTypeName;
                        String behaviorClassName = behaviorTypeName.Substring(behaviorTypeName.LastIndexOf(".") + ".".Length);
                        invokeClassName = behaviorClassName + ".OutsideSql()";
                        if (originalName.Contains("OutsideSqlEntityExecutor")) { // CSharp Only Contains
                            invokeClassName = invokeClassName + ".EntityHandling()";
                        } else if (originalName.Contains("OutsideSqlPagingExecutor")) { // CSharp Only Contains
                            if (outsideSqlContext.IsAutoPagingLogging) {
                                invokeClassName = invokeClassName + ".AutoPaging()";
                            } else {
                                invokeClassName = invokeClassName + ".ManualPaging()";
                            }
                        } else if (originalName.Contains("OutsideSqlCursorExecutor")) { // CSharp Only Contains
                            invokeClassName = invokeClassName + ".CursorHandling()";
                        }
                    } else {
                        invokeClassName = "OutsideSql";
                    }
                } catch (Exception ignored) {
                    Log("Ignored exception occurred: msg=" + ignored.Message);
                }
            }
            String invocationExpressionWithoutKakko = invokeClassName  + "." + invokeMethodName;
            if ("SelectPage".Equals(invokeMethodName)) { // Special Handling!
                bool resultTypeInteger = false;
                if (IsSpecifiedOutsideSql()) {
                    OutsideSqlContext outsideSqlContext = GetOutsideSqlContext();
                    Type resultType = outsideSqlContext.ResultType;
                    if (resultType != null) {
                        if (typeof(int).IsAssignableFrom(resultType)) {
                            resultTypeInteger = true;
                        }
                    }
                }
                if (resultTypeInteger || "SelectCount".Equals(invocation.Method.Name)) {
                    invocationExpressionWithoutKakko = invocationExpressionWithoutKakko + "():count";
                } else {
                    invocationExpressionWithoutKakko = invocationExpressionWithoutKakko + "():paging";
                }
            }
            return invocationExpressionWithoutKakko;
        }

        protected String BuildSimpleExpressionFromDaoName(String name) {
    		return InvokeNameStringUtil.removeBasePrefixFromSimpleClassName(name);
        }

        protected void LogPath(IMethodInvocation invocation, StackTrace stackTrace, InvokeNameExtractingResult behaviorResult) {
            try {
                int bhvNextIndex = behaviorResult.getNextStartIndex();
                InvokeNameExtractingResult clientResult = extractClientInvokeName(stackTrace, bhvNextIndex);
                int clientFirstIndex = clientResult.getFoundFirstIndex();
                InvokeNameExtractingResult byPassResult = extractByPassInvokeName(stackTrace, bhvNextIndex, clientFirstIndex - bhvNextIndex);
                String clientInvokeName = clientResult.getInvokeName();
                String byPassInvokeName = byPassResult.getInvokeName();
                String behaviorInvokeName = behaviorResult.getInvokeName();

                // Save client invoke name for error message.
                if (clientResult.getSimpleClassName() != null) {
                    PutObjectToMapContext("df:ClientInvokeName", clientInvokeName);
                }
                // Save by-pass invoke name for error message.
                if (byPassResult.getSimpleClassName() != null) {
                    PutObjectToMapContext("df:ByPassInvokeName", byPassInvokeName);
                }

                if (clientInvokeName.Trim().Length == 0 && byPassInvokeName.Trim().Length == 0) {
                    return;
                }
                Log(clientInvokeName + byPassInvokeName + behaviorInvokeName + "...");
            } catch (Exception ignored) {
                Log("Failed to extract client or by-pass name: " + ignored.Message);
            }
        }

        protected InvokeNameExtractingResult extractClientInvokeName(StackTrace stackTrace, int startIndex) {
            InvokeNameExtractingCallback callback = new ClientInvokeNameExtractingCallback(startIndex);
            return extractInvokeName(callback, stackTrace);
        }

        protected class ClientInvokeNameExtractingCallback : InvokeNameExtractingCallback {
            protected int _startIndex;
            public ClientInvokeNameExtractingCallback(int startIndex) {
                _startIndex = startIndex;
            }
            public bool isTargetElement(String className, String methodName) {
                return isClassNameEndsWith(className);
            }
            public String filterSimpleClassName(String simpleClassName) {
                return simpleClassName;
            }
            public bool isUseAdditionalInfo() {
                return true;
            }
            public int getStartIndex() {
                return _startIndex;
            }
            public int getLoopSize() {
                return 25;
            }

            protected bool isClassNameEndsWith(String className) {
                String[] suffixArray = new String[]{"Page", "Action"};
                foreach (String suffix in suffixArray) { if (className.EndsWith(suffix)) { return true; } }
                return false;
            }
        }

        protected InvokeNameExtractingResult extractByPassInvokeName(StackTrace stackTrace, int startIndex, int loopSize) {
            InvokeNameExtractingCallback callback = new ByPassInvokeNameExtractingCallback(startIndex, loopSize);
            return extractInvokeName(callback, stackTrace);
        }

        protected class ByPassInvokeNameExtractingCallback : InvokeNameExtractingCallback {
            protected int _startIndex;
            protected int _loopSize;
            public ByPassInvokeNameExtractingCallback(int startIndex, int loopSize) {
                _startIndex = startIndex;
                _loopSize = loopSize;
            }
            public bool isTargetElement(String className, String methodName) {
                return isClassNameEndsWith(className);
            }
            public String filterSimpleClassName(String simpleClassName) {
                return simpleClassName;
            }
            public bool isUseAdditionalInfo() {
                return true;
            }
            public int getStartIndex() {
                return _startIndex;
            }
            public int getLoopSize() {
                return _loopSize >= 0 ? _loopSize : 25;
            }

            protected bool isClassNameEndsWith(String className) {
                String[] suffixArray = new String[]{"Service", "ServiceImpl", "Facade", "FacadeImpl"};
                foreach (String suffix in suffixArray) { if (className.EndsWith(suffix)) { return true; } }
                return false;
            }
        }

        protected InvokeNameExtractingResult extractBehaviorInvokeName(StackTrace stackTrace) {
            InvokeNameExtractingCallback callback = new BehaviorInvokeNameExtractingCallback();
            return extractInvokeName(callback, stackTrace);
        }

        protected class BehaviorInvokeNameExtractingCallback : InvokeNameExtractingCallback {
            public bool isTargetElement(String className, String methodName) {
                if (isClassNameEndsWith(className)) {
                    return true;
                }
                if (isClassNameContains(className)) {
                    return true;
                }
                if (isClassNameOutsideSqlExecutor(className)) {
                    return true;
                }
                return false;
            }
            public String filterSimpleClassName(String simpleClassName) {
                return removeBasePrefixFromSimpleClassName(simpleClassName);
            }
            public bool isUseAdditionalInfo() {
                return false;
            }
            public int getStartIndex() {
                return 0;
            }
            public int getLoopSize() {
                return 25;
            }

            protected bool isClassNameEndsWith(String className) {
                String[] suffixArray = new String[]{"Bhv", "BhvAp", "BehaviorReadable", "BehaviorWritable"};
                foreach (String suffix in suffixArray) { if (className.EndsWith(suffix)) { return true; } }
                return false;
            }

            protected bool isClassNameContains(String className) {
                String[] keywordArray = new String[]{"PagingInvoker"};
                foreach (String keyword in keywordArray) { if (className.Contains(keyword)) { return true; } }
                return false;
            }

            protected bool isClassNameOutsideSqlExecutor(String className) {
                String[] keywordArray1 = new String[]{"OutsideSql"};
                String[] keywordArray2 = new String[]{"Executor"};
                bool containsOutsideSql = false;
                bool containsExecutor = false; // CSharp Only Contains because of unknown suffix found
                foreach (String keyword in keywordArray1) { if (className.Contains(keyword)) { containsOutsideSql = true; break; } }
                foreach (String keyword in keywordArray2) { if (className.Contains(keyword)) { containsExecutor = true; break; } }
                return containsOutsideSql && containsExecutor;
            }

            protected String removeBasePrefixFromSimpleClassName(String simpleClassName) {
                return InvokeNameStringUtil.removeBasePrefixFromSimpleClassName(simpleClassName);
            }
        }

        protected InvokeNameExtractingResult extractInvokeName(InvokeNameExtractingCallback callback, StackTrace stackTrace) {
            String targetSimpleClassName = null;
            String targetMethodName = null;
            int lineNumber = 0;
            int foundIndex = -1;// The minus one means 'Not Found'.
            int foundFirstIndex = -1;// The minus one means 'Not Found'.
            bool onTarget = false;
            for (int i = 0; i < stackTrace.FrameCount; i++) {
                StackFrame stackFrame = stackTrace.GetFrame(i); // The inner class is not appeared.
                MethodBase method = stackFrame.GetMethod();
                if (method == null) {
                    break; // End
                }
                if (method.DeclaringType == null) {
                    break; // If you use WCF, it may return null. 
                }
                String className = method.DeclaringType.Name; // This is simple class name at CSharp.
                String methodName = method.Name;

                if (i > callback.getStartIndex() + callback.getLoopSize()) {
                    break;
                }
                if (className.StartsWith("System.")) { // This is system package of CSharp.
                    if (onTarget) { // But the class name is simple so this 'if' statement is waste... 
                        break;
                    }
                    continue;
                }
                if (callback.isTargetElement(className, methodName)) {
                    if (methodName.Equals("Invoke")) {
                        continue;
                    }
                    targetSimpleClassName = className.Substring(className.LastIndexOf(".") + 1);
                    targetMethodName = methodName;
                    if (callback.isUseAdditionalInfo()) {
                        try {
                            lineNumber = stackFrame.GetFileLineNumber();
                        } catch (Exception) {
                            // Ignored
                        }
                    }
                    foundIndex = i;
                    if (foundFirstIndex == -1) {
                        foundFirstIndex = i;
                    }
                    onTarget = true;
                    continue;
                }
                if (onTarget) {
                    break;
                }
            }
            InvokeNameExtractingResult result = new InvokeNameExtractingResult();
            if (targetSimpleClassName == null) {
                result.setInvokeName("");// Not Found! It sets empty string as default.
                return result;
            }
            String filteredClassName = callback.filterSimpleClassName(targetSimpleClassName);
            result.setSimpleClassName(callback.filterSimpleClassName(targetSimpleClassName));
            result.setMethodName(targetMethodName);
            if (lineNumber > 0) {
                result.setInvokeName(filteredClassName + "." + targetMethodName + "():" + lineNumber + " --> ");
            } else {
                result.setInvokeName(filteredClassName + "." + targetMethodName + "() --> ");
            }
            result.setFoundIndex(foundIndex);
            result.setFoundFirstIndex(foundFirstIndex);
            return result;
        }

        protected interface InvokeNameExtractingCallback {
            bool isTargetElement(String className, String methodName);
            String filterSimpleClassName(String simpleClassName);
            bool isUseAdditionalInfo();
            int getStartIndex();
            int getLoopSize();
        }

        protected class InvokeNameExtractingResult {
            protected String _simpleClassName;
            protected String _methodName;
            protected String _invokeName;
            protected int _foundIndex;
            protected int _foundFirstIndex;

            public int getNextStartIndex() {
                return _foundIndex + 1;
            }

            public String getSimpleClassName() {
                return _simpleClassName;
            }
            public void setSimpleClassName(String simpleClassName) {
                _simpleClassName = simpleClassName;
            }
            public String getMethodName() {
                return _methodName;
            }
            public void setMethodName(String methodName) {
                _methodName = methodName;
            }
            public String getInvokeName() {
                return _invokeName;
            }
            public void setInvokeName(String invokeName) {
                _invokeName = invokeName;
            }
            public int getFoundIndex() {
                return _foundIndex;
            }
            public void setFoundIndex(int foundIndex) {
                _foundIndex = foundIndex;
            }
            public int getFoundFirstIndex() {
                return _foundFirstIndex;
            }
            public void setFoundFirstIndex(int foundFirstIndex) {
                _foundFirstIndex = foundFirstIndex;
            }
        }

        protected static class InvokeNameStringUtil {
            public static String removeBasePrefixFromSimpleClassName(String simpleClassName) {
                if (!simpleClassName.StartsWith("Bs")) {
                    return simpleClassName;
                }
                int prefixLength = "Bs".Length;
                // [Under Review]: I want someone to teach me how to do this by CSharp!
                // if (!Character.isUpperCase(simpleClassName.substring(prefixLength).charAt(0))) {
                //     return simpleClassName;
                // }
                if (simpleClassName.Length <= prefixLength) {
                    return simpleClassName;
                }
                return "" + simpleClassName.Substring(prefixLength);
            }
        }

        // ===============================================================================
        //                                                                  Log SqlCommand
        //                                                                  ==============
        protected void LogSqlCommand(IMethodInvocation invocation, ISqlCommand cmd, DateTime beforeCmd, DateTime afterCmd) {
            Log("SqlCommand Initialization Cost: [" + TraceViewUtil.ConvertToPerformanceView(beforeCmd, afterCmd) + "]");
        }

        // ===============================================================================
        //                                                                      Log Return
        //                                                                      ==========
        protected void LogReturn(IMethodInvocation invocation, Type retType, Object ret, DateTime before, DateTime after) {
            MethodBase method = invocation.Method;
            try {
                String daoResultPrefix = "===========/ [" + TraceViewUtil.ConvertToPerformanceView(before, after) + " ";
                if (typeof(System.Collections.IList).IsAssignableFrom(retType) || (ret != null && ret is System.Collections.IList)) {
                    if (ret == null) {
                        Log(daoResultPrefix + "(null)]");
                    } else {
					    System.Collections.IList ls = (System.Collections.IList)ret;
                        if (ls.Count == 0) {
                            Log(daoResultPrefix + "(0)]");
                        } else if (ls.Count == 1) {
                            Log(daoResultPrefix + "(1) result=" + BuildResultString(ls[0]) + "]");
                        } else {
                            Log(daoResultPrefix + "(" + ls.Count + ") first=" + BuildResultString(ls[0]) + "]");
                        }
                    }
                } else if (typeof(Entity).IsAssignableFrom(retType)) {
                    if (ret == null) {
                        Log(daoResultPrefix + "(null)" + "]");
                    } else {
                        Entity entity = (Entity)ret;
                        Log(daoResultPrefix + "(1) result=" + BuildResultString(entity) + "]");
                    }
                } else {
                    Log(daoResultPrefix + "result=" + ret + "]");
                }
                Log(" ");
            } catch (Exception e) {
                String msg = "Result object debug threw the exception: methodName=" + method.Name + " retType=" + retType;
                msg = msg + " ret=" + ret;
                _log.Warn(msg, e);
                throw;
            }
        }

        protected String BuildResultString(Object obj) {
            if (obj is Entity) {
                Entity entity = (Entity) obj;

                // The name for display is null
                // because you can know it other execute status logs.
                return entity.BuildDisplayString(null, true, true);
            } else {
                return obj != null ? obj.ToString() : "null";
            }
        }

        // ===============================================================================
        //                                                                Pre Post Process
        //                                                                ================
        // -------------------------------------------------
        //                                        OutsideSql
        //                                        ----------
        protected void PreprocessOutsideSql(IMethodInvocation invocation) {
            // Specified OutsideSql
            if (IsSpecifiedOutsideSql(invocation)) {
                if (IsOutsideSqlDaoMethodSelect(invocation)) {
                    SetupOutsideSqlContextSelect(invocation);
                } else {
                    SetupOutsideSqlContextExecute(invocation);
                }
                return;
            }
        }
	
    	protected bool IsSpecifiedOutsideSql(IMethodInvocation invocation) {
    	    return typeof(OutsideSqlDao).IsAssignableFrom(GetComponentDef(invocation).ComponentType);
    	}
		
        // - - - - - - - - - -
        //              Select
        //               - - -
        protected bool IsOutsideSqlDaoMethodSelect(IMethodInvocation invocation) {
            return invocation.Method.Name.StartsWith("Select");
        }

        protected void SetupOutsideSqlContextSelect(IMethodInvocation invocation) {
            Object[] args = invocation.Arguments;
            if (args.Length != 4) {
                String msg = "Internal Error! OutsideSqlDao.selectXxx() should have 4 arguements: args.Length=" + args.Length;
                throw new SystemException(msg);
            }
            String path = GetOutsideSqlPath(args);
            Object pmb = GetOutsideSqlParameterBean(args);
            OutsideSqlOption option = GetOutsideSqlOption(args);
            Type resultType = args[3] is Type ? (Type)args[3] : args[3].GetType();
            bool autoPagingLogging = (option.IsAutoPaging || option.IsSourcePagingRequestTypeAuto);
            OutsideSqlContext outsideSqlContext = new OutsideSqlContext();
            outsideSqlContext.OutsideSqlPath = path;
            outsideSqlContext.ParameterBean = pmb;
            outsideSqlContext.ResultType = resultType;
            outsideSqlContext.MethodName = invocation.Method.Name;
			outsideSqlContext.StatementConfig = option.StatementConfig;
			outsideSqlContext.TableDbName = option.TableDbName;
            outsideSqlContext.IsDynamicBinding = option.IsDynamicBinding;
            outsideSqlContext.IsOffsetByCursorForcedly = option.IsAutoPaging;
            outsideSqlContext.IsLimitByCursorForcedly = option.IsAutoPaging;
            outsideSqlContext.IsAutoPagingLogging = autoPagingLogging; // for logging
			outsideSqlContext.SetupBehaviorQueryPathIfNeeds();
            OutsideSqlContext.SetOutsideSqlContextOnThread(outsideSqlContext);

            // Set up fetchNarrowingBean.
            SetupOutsideSqlFetchNarrowingBean(pmb, option);
        }

        // - - - - - - - - - -
        //             Execute
        //             - - - -
        protected void SetupOutsideSqlContextExecute(IMethodInvocation invocation) {
            Object[] args = invocation.Arguments;
            if (args.Length != 3) {
                String msg = "Internal Error! OutsideSqlDao.execute() should have 3 arguements: args.Length=" + args.Length;
                throw new SystemException(msg);
            }
            String path = GetOutsideSqlPath(args);
            Object pmb = GetOutsideSqlParameterBean(args);
            OutsideSqlOption option = GetOutsideSqlOption(args);
            OutsideSqlContext outsideSqlContext = new OutsideSqlContext();
            outsideSqlContext.IsDynamicBinding = option.IsDynamicBinding;
            outsideSqlContext.IsOffsetByCursorForcedly = option.IsAutoPaging;
            outsideSqlContext.IsLimitByCursorForcedly = option.IsAutoPaging;
            outsideSqlContext.OutsideSqlPath = path;
            outsideSqlContext.ParameterBean = pmb;
            outsideSqlContext.MethodName = invocation.Method.Name;
            outsideSqlContext.StatementConfig = option.StatementConfig;
			outsideSqlContext.TableDbName = option.TableDbName;
			outsideSqlContext.SetupBehaviorQueryPathIfNeeds();
            OutsideSqlContext.SetOutsideSqlContextOnThread(outsideSqlContext);

            // Set up fetchNarrowingBean.
            SetupOutsideSqlFetchNarrowingBean(pmb, option);
        }

        // - - - - - - - - - -
        //              Common
        //               - - -
    	protected String GetOutsideSqlPath(Object[] args) {
            return (String)args[0];
    	}
    	protected Object GetOutsideSqlParameterBean(Object[] args) {
            return args[1];
    	}
    	protected OutsideSqlOption GetOutsideSqlOption(Object[] args) {
            return (OutsideSqlOption)args[2];
    	}
	
        protected void SetupOutsideSqlFetchNarrowingBean(Object pmb, OutsideSqlOption option) {
            if (pmb == null || !FetchNarrowingBeanContext.IsTheTypeFetchNarrowingBean(pmb.GetType())) {
                return;
            }
            FetchNarrowingBean fetchNarrowingBean = (FetchNarrowingBean)pmb;
            if (option.IsManualPaging) {
                fetchNarrowingBean.IgnoreFetchNarrowing();
            }
            FetchNarrowingBeanContext.SetFetchNarrowingBeanOnThread(fetchNarrowingBean);
        }

        // -------------------------------------------------
        //                                     ConditionBean
        //                                     -------------
        protected ConditionBean PreprocessConditionBean(IMethodInvocation invocation) {
            OutsideSqlContext outsideSqlContext = GetOutsideSqlContext();
            if (outsideSqlContext != null) {
                return null; // Because it has already finished setting up fetchNarrowingBean for outsideSql here.
            }
			
            ConditionBean cb = null;
            {
                Object[] args = invocation.Arguments;
                if (args == null || !(args.Length >= 1)) {
                    return null;
                }
                Object arg0 = args[0];
                if (arg0 == null) {
                    return null;
                }

                if (!ConditionBeanContext.IsTheArgumentConditionBean(arg0)) {// The argument is not condition-bean...
                    if (FetchNarrowingBeanContext.IsTheArgumentFetchNarrowingBean(arg0) && !IsSelectCountIgnoreFetchScopeMethod(invocation)) {
                        // Fetch-narrowing-bean and Not select count!
                        FetchNarrowingBeanContext.SetFetchNarrowingBeanOnThread((FetchNarrowingBean)arg0);
                    }
                    return null;
                }

                cb = (ConditionBean)arg0;
            }

            if (IsSelectCountIgnoreFetchScopeMethod(invocation)) {
                cb.xsetupSelectCountIgnoreFetchScope();
            } else {
                FetchNarrowingBeanContext.SetFetchNarrowingBeanOnThread(cb);
            }

            ConditionBeanContext.SetConditionBeanOnThread(cb);
            return cb;
        }

        public void PostprocessConditionBean(IMethodInvocation invocation, ConditionBean cb) {
            if (cb == null) {
                return;
            }
            if (IsSelectCountIgnoreFetchScopeMethod(invocation)) {
                cb.xafterCareSelectCountIgnoreFetchScope();
            }
        }

        // ===============================================================================
        //                                                                  Context Helper
        //                                                                  ==============
        protected void InitializeContext() {
            if (ConditionBeanContext.IsExistConditionBeanOnThread()
                || OutsideSqlContext.IsExistOutsideSqlContextOnThread()
                || FetchNarrowingBeanContext.IsExistFetchNarrowingBeanOnThread()
                || InternalMapContext.IsExistInternalMapOnThread()) { // means recursive invoking
                SaveAllContextOnThread();
            }
            ClearAllCurrentContext();
        }

        protected void CloseContext() {
            if (FetchNarrowingBeanContext.IsExistFetchNarrowingBeanOnThread()) {
                // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                // Because there is possible that fetch narrowing has been ignored for manualPaging of outsideSql.
                // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                FetchNarrowingBeanContext.GetFetchNarrowingBeanOnThread().RestoreIgnoredFetchNarrowing();
            }
            ClearAllCurrentContext();
            RestoreAllContextOnThreadIfExists();
        }


        protected void SaveAllContextOnThread() {
            ContextStack.SaveAllContextOnThread();
        }

        protected void RestoreAllContextOnThreadIfExists() {
            ContextStack.RestoreAllContextOnThreadIfExists();
        }

        protected void ClearAllCurrentContext() {
            ContextStack.ClearAllCurrentContext();
        }

        protected void ClearCurrentContext() {
            if (ConditionBeanContext.IsExistConditionBeanOnThread()) {
                ConditionBeanContext.ClearConditionBeanOnThread();
            }
            if (OutsideSqlContext.IsExistOutsideSqlContextOnThread()) {
                OutsideSqlContext.ClearOutsideSqlContextOnThread();
            }
            if (FetchNarrowingBeanContext.IsExistFetchNarrowingBeanOnThread()) {
                FetchNarrowingBeanContext.ClearFetchNarrowingBeanOnThread();
            }
            if (InternalMapContext.IsExistInternalMapOnThread()) {
                InternalMapContext.ClearInternalMapOnThread();
            }
        }

        protected OutsideSqlContext GetOutsideSqlContext() {
            if (!OutsideSqlContext.IsExistOutsideSqlContextOnThread()) {
                return null;
            }
            return OutsideSqlContext.GetOutsideSqlContextOnThread();
        }
        
        protected bool IsSpecifiedOutsideSql() {
            OutsideSqlContext outsideSqlContext = GetOutsideSqlContext();
            return outsideSqlContext != null && outsideSqlContext.IsSpecifiedOutsideSql;
        }

        protected virtual SqlResultHandler GetSqlResultHander() {
            if (!CallbackContext.IsExistCallbackContextOnThread()) {
                return null;
            }
            return CallbackContext.GetCallbackContextOnThread().SqlResultHandler;
        }

        protected void PutObjectToMapContext(String key, Object value) {
            InternalMapContext.SetObject(key, value);
        }

        // ===============================================================================
        //                                                                   Determination
        //                                                                   =============
        protected bool IsSelectCountIgnoreFetchScopeMethod(IMethodInvocation invocation) {
            String name = invocation.Method.Name;
            return name.StartsWith("ReadCount") || name.StartsWith("SelectCount");
        }

        // ===============================================================================
        //                                                                          Helper
        //                                                                          ======
        protected virtual String GetLineSeparator() {
            return Environment.NewLine;
        }
    }
}
