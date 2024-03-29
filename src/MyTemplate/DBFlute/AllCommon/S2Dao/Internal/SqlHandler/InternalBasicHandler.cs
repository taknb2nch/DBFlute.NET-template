
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

using Seasar.Framework.Exceptions;
using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Types;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlLog;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler {

    public class InternalBasicHandler {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        private IDataSource _dataSource;
        private String _sql;
        private ICommandFactory _commandFactory;
        private Object[] _loggingMessageSqlArgs;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalBasicHandler(IDataSource ds, String sql, ICommandFactory commandFactory) {
            DataSource = ds;
            Sql = sql;
            CommandFactory = commandFactory;
        }

        // ===============================================================================
        //                                                                    Common Logic
        //                                                                    ============
        // -------------------------------------------------
        //                                     Args Handling
        //                                     -------------
        protected virtual void BindArgs(IDbCommand command, Object[] args, Type[] argTypes) {
            if (_loggingMessageSqlArgs == null) { _loggingMessageSqlArgs = args; } // Save arguments for logging.
            if (args == null) return;
            string[] argNames = _commandFactory.GetArgNames(command, args);
            for (int i = 0; i < args.Length; ++i) {
                IValueType valueType = ValueTypes.GetValueType(argTypes[i]);
                try {
                    valueType.BindValue(command, argNames[i], args[i]);
                } catch (Exception e) {
                    HandleDbException(e, command);
                }
            }
        }

        protected virtual Type[] GetArgTypes(Object[] args) {
            if (args == null) {
                return null;
            }
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; ++i) {
                object arg = args[i];
                if (arg != null) {
                    argTypes[i] = arg.GetType();
                }
            }
            return argTypes;
        }

        // -------------------------------------------------
        //                                       SQL Logging
        //                                       -----------
        protected virtual void LogSql(Object[] args, Type[] argTypes) {
            // [SqlLogHandler]
            SqlLogHandler sqlLogHandler = GetSqlLogHander();
            bool existsSqlLogHandler = sqlLogHandler != null;

            // [SqlResultHandler]
            SqlResultHandler sqlResultHandler = GetSqlResultHander();
            bool existsSqlResultHandler = sqlResultHandler != null;

            // [SqlLogRegistry]
            InternalSqlLogRegistry sqlLogRegistry = InternalSqlLogRegistryLocator.Instance;
            bool existsSqlLogRegistry = sqlLogRegistry != null;

            if (IsLogEnabled() || existsSqlLogHandler || existsSqlResultHandler || existsSqlLogRegistry) {
                String displaySql = GetCompleteSql(args);
                if (IsLogEnabled()) {
                    Log((IsContainsLineSeparatorInSql() ? GetLineSeparator() : "") + displaySql);
                }
                if (existsSqlLogHandler) { // DBFlute provides
                    sqlLogHandler.Invoke(this.Sql, displaySql, args, argTypes);
                }
                if (existsSqlLogRegistry) { // S2Container provides (But Actually DBFlute provides at CSharp)
                    sqlLogRegistry.Add(CreateInternalSqlLog(displaySql, args, argTypes));
                }
                PutObjectToMapContext("df:DisplaySql", displaySql);
            }
        }

        protected void PutObjectToMapContext(String key, Object value) {
            InternalMapContext.SetObject(key, value);
        }

        protected virtual bool IsLogEnabled() {
            return QLog.IsLogEnabled();
        }

        protected virtual void Log(String msg) {
            QLog.Log(msg);
        }

        protected virtual String GetCompleteSql(Object[] args) {
            return InternalBindVariableUtil.GetCompleteSql(_sql, args);
        }

        protected virtual SqlLogHandler GetSqlLogHander() {
            if (!CallbackContext.IsExistCallbackContextOnThread()) {
                return null;
            }
            return CallbackContext.GetCallbackContextOnThread().SqlLogHandler;
        }

        protected virtual SqlResultHandler GetSqlResultHander() {
            if (!CallbackContext.IsExistCallbackContextOnThread()) {
                return null;
            }
            return CallbackContext.GetCallbackContextOnThread().SqlResultHandler;
        }

        protected virtual bool IsContainsLineSeparatorInSql() {
            return Sql != null ? Sql.Contains(GetLineSeparator()) : false;
        }
        
        protected virtual InternalSqlLog CreateInternalSqlLog(String displaySql, Object[] args, Type[] argTypes) {
            InternalSqlLog sqlLog = new InternalSqlLog();
            sqlLog.RawSql = this.Sql;
            sqlLog.CompleteSql = displaySql;
            sqlLog.BindArgs = args;
            sqlLog.BindArgTypes = argTypes;
            return sqlLog;
        }

        // -------------------------------------------------
        //                                           Various
        //                                           -------
        protected virtual String GetBindVariableText(Object bindVariable) {
            return InternalBindVariableUtil.GetBindVariableText(bindVariable);
        }

        // ===============================================================================
        //                                                               Exception Handler
        //                                                               =================
        protected void HandleDbException(Exception e, IDbCommand cmd) {
            HandleDbException(e, cmd, false);
        }

        protected void HandleDbException(Exception e, IDbCommand cmd, bool uniqueConstraintValid) {
            String displaySql = BuildLoggingMessageSql();
            new DbExceptionHandler().HandleDbException(e, cmd, uniqueConstraintValid, displaySql);
        }

        protected virtual String BuildLoggingMessageSql() {
            String displaySql = null;
            if (_sql != null && _loggingMessageSqlArgs != null) {
                try {
                    displaySql = GetCompleteSql(_loggingMessageSqlArgs);
                } catch (Exception) {
                }
            }
            return displaySql;
        }
        
        // ===============================================================================
        //                                                               ADO.NET Delegator
        //                                                               =================
        protected virtual Object ExecuteScalar(IDbCommand cmd) {
            try {
                return CommandFactory.ExecuteScalar(DataSource, cmd);
            } catch (Exception e) {
                HandleDbException(e, cmd);
                return null; // Unreachable!
            }
        }

        protected virtual Object ExecuteNonQuery(IDbCommand cmd) {
            try {
                return CommandFactory.ExecuteNonQuery(DataSource, cmd);
            } catch (Exception e) {
                HandleDbException(e, cmd);
                return null; // Unreachable!
            }
        }

        protected virtual int ExecuteUpdate(IDbCommand cmd) {
            try {
                return CommandFactory.ExecuteNonQuery(DataSource, cmd);
            } catch (Exception e) {
                HandleDbException(e, cmd, true);
                return 0; // Unreachable!
            }
        }

        protected virtual void Close(IDbConnection conn) {
            try {
                DataSource.CloseConnection(conn);
            } catch (Exception e) {
                HandleDbException(e, null);
            }
        }

        protected virtual void Close(IDbCommand cmd) {
            try {
                CommandUtil.Close(cmd);
            } catch (Exception e) {
                HandleDbException(e, cmd);
            }
        }

        protected virtual void Close(IDataReader dataReader) {
            try {
                DataReaderUtil.Close(dataReader);
            } catch (Exception e) {
                HandleDbException(e, null);
            }
        }

        protected IDbConnection Connection {
            get {
                if (_dataSource == null) {
                    throw new SystemException("The dataSource should not be null at InternalBasicHandler!");
                }
                try {
                    return DataSourceUtil.GetConnection(_dataSource);
                } catch (Exception e) {
                    HandleDbException(e, null);
                    return null; // Unreachable!
                }
            }
        }

        protected virtual IDbCommand Command(IDbConnection connection) {
            if (_sql == null) {
                throw new SystemException("The sql should not be null at InternalBasicHandler!!");
            }
            try {
                return _commandFactory.CreateCommand(connection, _sql);
            } catch (Exception e) {
                HandleDbException(e, null);
                return null; // Unreachable!
            }
        }

        // ===================================================================================
        //                                                                       Assist Helper
        //                                                                       =============
        // It needs this method if the target database does not support line comment.
        protected String RemoveLineComment(String sql) { // With removing CR!
            if (sql == null || sql.Trim().Length == 0) {
                return sql;
            }
            StringBuilder sb = new StringBuilder();
            String[] lines = sql.Split('\n');
            foreach (String line in lines) {
                if (line == null) {
                    continue;
                }
                String filteredLine = line.Replace("\r", ""); // Remove CR!
                if (filteredLine.StartsWith("--")) {
                    continue;
                }
                sb.Append(filteredLine).Append("\n");
            }
            String filteredSql = sb.ToString();
            return filteredSql.Substring(0, filteredSql.LastIndexOf("\n"));
        }

        // ===============================================================================
        //                                                                  General Helper
        //                                                                  ==============
        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IDataSource DataSource {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        public String Sql {
            get { return _sql; }
            set { _sql = value; }
        }

        public ICommandFactory CommandFactory {
            get { return _commandFactory; }
            set { _commandFactory = value; }
        }

        public Object[] LoggingMessageSqlArgs {
            get { return _loggingMessageSqlArgs; }
            set { _loggingMessageSqlArgs = value; }
        }
    }

    public class DbExceptionHandler {

        public void HandleDbException(Exception e, IDbCommand cmd) {
            HandleDbException(e, cmd, false);
        }

        public void HandleDbException(Exception e, IDbCommand cmd, bool uniqueConstraintValid) {
            // [UnderReview]: How do I get unique constraint on ADO.NET?
            // if (uniqueConstraintValid && IsUniqueConstraintException(e)) {
            //     ThrowEntityAlreadyExistsException(e, cmd, resource);
            // }
            HandleDbException(e, cmd, false, null);
        }

        public void HandleDbException(Exception e, IDbCommand cmd, bool uniqueConstraintValid, String displaySql) {
            // [UnderReview]: How do I get unique constraint on ADO.NET?
            // if (uniqueConstraintValid && IsUniqueConstraintException(e)) {
            //     ThrowEntityAlreadyExistsException(e, cmd, resource);
            // }
            if (e is DangerousResultSizeException) {
                throw e; // because SQLException on C# is runtime exception 
            }
            ThrowSQLFailureException(e, cmd, displaySql);
        }

        protected bool IsUniqueConstraintException(DbException e) {
            // [UnderReview]: How do I get unique constraint on ADO.NET?
            // return ConditionBeanContext.IsUniqueConstraintException(ExtractSQLState(e), e.ErrorCode);
            return false;
        }

        protected void ThrowSQLFailureException(Exception e, IDbCommand cmd, String displaySql) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The SQL failed to execute!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the Exception message." + GetLineSeparator();
            msg = msg + GetLineSeparator();
            // msg = msg + "[SQLState]" + GetLineSeparator() + ExtractSQLState(e) + GetLineSeparator();
            // msg = msg + GetLineSeparator();
            // msg = msg + "[ErrorCode]" + GetLineSeparator() + e.ErrorCode + GetLineSeparator();
            // msg = msg + GetLineSeparator();
            msg = msg + "[Exception]" + GetLineSeparator() + e.GetType().FullName + GetLineSeparator();
            msg = msg + e.Message + GetLineSeparator();
            int? errorCode = ExtractErrorCode(e);
            if (errorCode != null) {
                msg = msg + "  ErrorCode = " + errorCode + GetLineSeparator();
            }
            if (e is DbException) {
                msg = msg + "  HelpLink  = " + ((DbException)e).HelpLink + GetLineSeparator();
            }
            Exception nextEx = e.InnerException;
            if (nextEx != null) {
                msg = msg + GetLineSeparator();
                msg = msg + "[NextException]" + GetLineSeparator();
                msg = msg + nextEx.GetType().FullName + GetLineSeparator();
                msg = msg + nextEx.Message + GetLineSeparator();
                errorCode = ExtractErrorCode(nextEx);
                if (errorCode != null) {
                    msg = msg + "  ErrorCode = " + errorCode + GetLineSeparator();
                }
                if (nextEx is DbException) {
                    msg = msg + "  HelpLink  = " + ((DbException)nextEx).HelpLink + GetLineSeparator();
                }
                Exception nextNextEx = nextEx.InnerException;
                if (nextNextEx != null) {
                    msg = msg + GetLineSeparator();
                    msg = msg + "[NextNextException]" + GetLineSeparator();
                    msg = msg + nextNextEx.GetType().FullName + GetLineSeparator();
                    msg = msg + nextNextEx.Message + GetLineSeparator();
                    errorCode = ExtractErrorCode(nextNextEx);
                    if (errorCode != null) {
                        msg = msg + "  ErrorCode = " + errorCode + GetLineSeparator();
                    }
                    if (nextNextEx is DbException) {
                        msg = msg + "  HelpLink  = " + ((DbException)nextNextEx).HelpLink + GetLineSeparator();
                    }
                }
            }
            Object invokeName = ExtractBehaviorInvokeName();
            if (invokeName != null) {
                msg = msg + GetLineSeparator();
                msg = msg + "[Behavior]" + GetLineSeparator();
                msg = msg + invokeName + GetLineSeparator();
            }
            if (HasConditionBean()) {
                msg = msg + GetLineSeparator();
                msg = msg + "[ConditionBean]" + GetLineSeparator();
                msg = msg + GetConditionBean().GetType().FullName + GetLineSeparator();
            }
            if (HasOutsideSqlContext()) {
                msg = msg + GetLineSeparator();
                msg = msg + "[OutsideSqlPath]" + GetLineSeparator();
                msg = msg + GetOutsideSqlContext().OutsideSqlPath + GetLineSeparator();
                msg = msg + GetLineSeparator();
                msg = msg + "[ParameterBean]" + GetLineSeparator();
                Object pmb = GetOutsideSqlContext().ParameterBean;
                if (pmb != null) {
                    msg = msg + pmb.GetType().FullName + GetLineSeparator();
                    msg = msg + pmb + GetLineSeparator();
                } else {
                    msg = msg + pmb + GetLineSeparator();
                }
            }
            if (cmd != null) {
                msg = msg + GetLineSeparator();
                msg = msg + "[Statement]" + GetLineSeparator() + cmd + GetLineSeparator();
            }
            if (displaySql != null) {
                msg = msg + GetLineSeparator();
                msg = msg + "[Display SQL]" + GetLineSeparator();
                msg = msg + displaySql + GetLineSeparator();
            }
            msg = msg + "* * * * * * * * * */";
            throw new SQLFailureException(msg, e);
        }

        // protected String ExtractSQLState(DbException e) {
            // return "" +  e.ErrorCode; // [UnderReview]: Where is SQLState?

            // // Next
            // DbException nextEx = e.InnerException;
            // if (nextEx == null) {
            //     return null;
            // }
            // sqlState = nextEx.getSQLState();
            // if (sqlState != null) {
            //     return sqlState;
            // }

            // // Next Next
            // DbException nextNextEx = nextEx.InnerException;
            // if (nextNextEx == null) {
            //     return null;
            // }
            // sqlState = nextNextEx.getSQLState();
            // if (sqlState != null) {
            //     return sqlState;
            // }

            // // Next Next Next
            // DbException nextNextNextEx = nextNextEx.InnerException;
            // if (nextNextNextEx == null) {
            //     return null;
            // }
            // sqlState = nextNextNextEx.getSQLState();
            // if (sqlState != null) {
            //     return sqlState;
            // }

            // // It doesn't use recursive call by design because JDBC is unpredictable fellow.
            // return null;
        // }

        protected int? ExtractErrorCode(Exception e) {
            String expName = e.GetType().Name;
            if (expName.Contains("Oracle") || expName.Contains("MySQL")) {
                try {
                    PropertyInfo pi = e.GetType().GetProperty("Number");
                    if (pi != null) {
                        Object result = pi.GetValue(e, null);
                        if (result != null && result is int) {
                            return (int)result;
                        }
                    }
                } catch (Exception) {
                }
            }
            if (e is DbException) {
                return ((DbException)e).ErrorCode;
            }
            return null;
        }

        protected String ExtractBehaviorInvokeName() {
            Object behaviorInvokeName = InternalMapContext.GetObject("df:BehaviorInvokeName");
            if (behaviorInvokeName == null) {
                return null;
            }
            Object clientInvokeName = InternalMapContext.GetObject("df:ClientInvokeName");
            Object byPassInvokeName = InternalMapContext.GetObject("df:ByPassInvokeName");
            StringBuilder sb = new StringBuilder();
            bool existsPath = false;
            if (clientInvokeName != null) {
                existsPath = true;
                sb.Append(clientInvokeName);
            }
            if (byPassInvokeName != null) {
                existsPath = true;
                sb.Append(byPassInvokeName);
            }
            sb.Append(behaviorInvokeName);
            if (existsPath) {
                sb.Append("...");
            }
            return sb.ToString();
        }

        protected bool HasConditionBean() {
            return ConditionBeanContext.IsExistConditionBeanOnThread();
        }

        protected ConditionBean GetConditionBean() {
            return ConditionBeanContext.GetConditionBeanOnThread();
        }

        protected bool HasOutsideSqlContext() {
            return OutsideSqlContext.IsExistOutsideSqlContextOnThread();
        }

        protected OutsideSqlContext GetOutsideSqlContext() {
            return OutsideSqlContext.GetOutsideSqlContextOnThread();
        }

        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }
    }
}
