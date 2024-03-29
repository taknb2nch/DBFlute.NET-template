
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using Seasar.Framework.Beans;
using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;
using Seasar.Extension.ADO.Types;
using Seasar.Dao;
using Seasar.Dao.Attrs;
using Seasar.Dao.Dbms;
using Seasar.Dao.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Annotation;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlCommand;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.RsHandler;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao {

    public class S2DaoMetaDataExtension : DaoMetaDataImpl {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected Map<Type, IBeanMetaData> _beanMetaDataCacheMap = new HashMap<Type, IBeanMetaData>();

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public S2DaoMetaDataExtension()
            : base() {}

        // ===============================================================================
        //                                                         Initialization Override
        //                                                         =======================
        public override void Initialize() {
            _daoInterface = GetDaoInterface(_daoType);
            _annotationReader = AnnotationReaderFactory.CreateDaoAnnotationReader(_daoType);
            _beanType = _annotationReader.GetBeanType();
            _dbms = GetDbms(_dataSource);
            
            // Initialize bean-meta-data!
            BeanMetaDataCacheHandler handler = new BeanMetaDataCacheHandler(_beanMetaDataCacheMap);
            _beanMetaData = handler.FindOrCreateCachedMetaIfNeeds(_beanType, _annotationReaderFactory, _dbMetaData, _dbms);
            if (_beanMetaData == null) {
                _beanMetaData = new BeanMetaDataCacheExtension(_beanType, _annotationReaderFactory, false);// Don't use cache!
            }

            // For lazy initializing methods!
            // SetupSqlCommand();
        }

        public override ISqlCommand GetSqlCommand(String methodName) {
            ISqlCommand cmd = (ISqlCommand)_sqlCommands[methodName];
            if (cmd != null) {
                return cmd;
            }
            lock (this) {
                cmd = (ISqlCommand)_sqlCommands[methodName];
                if (cmd != null) {
                    return cmd;
                }
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Initializing sqlCommand for " + methodName + "().");
                }
                return InitializeSqlCommand(methodName);
            }
        }

        protected ISqlCommand InitializeSqlCommand(String methodName) {
            if (OutsideSqlContext.IsExistOutsideSqlContextOnThread()) {
                OutsideSqlContext outsideSqlContext = OutsideSqlContext.GetOutsideSqlContextOnThread();
                if (outsideSqlContext != null && outsideSqlContext.IsSpecifiedOutsideSql) {
                    return InitializeSpecifiedOutsideSqlCommand(methodName, outsideSqlContext);
                }
            }
            MethodInfo method = _daoInterface.GetMethod(methodName);
            if (method != null && method.IsAbstract) {
                SetupMethod(method);
            }
            ISqlCommand cmd = (ISqlCommand) _sqlCommands[methodName];
            if (cmd != null) {
                return cmd;
            }
            throw new MethodNotFoundRuntimeException(_daoType, methodName, null);
        }

        protected ISqlCommand InitializeSpecifiedOutsideSqlCommand(String sqlCommandKey, OutsideSqlContext outsideSqlContext) {
            MethodInfo method = _daoInterface.GetMethod(outsideSqlContext.MethodName);// By real method name.
            if (method != null && method.IsAbstract) {
                if (IsOutsideSqlDaoMethodSelect(method)) {
                    SetupSpecifiedOutsideSqlSelectCommand(sqlCommandKey, method, outsideSqlContext);
                } else if (IsOutsideSqlDaoMethodCall(method)) {
                    SetupSpecifiedOutsideSqlCallCommand(sqlCommandKey, method, outsideSqlContext);
                } else {
                    SetupSpecifiedOutsideSqlExecuteCommand(sqlCommandKey, method, outsideSqlContext);
                }
            }
            ISqlCommand cmd = (ISqlCommand) _sqlCommands[sqlCommandKey];
            if (cmd != null) {
                return cmd;
            }
            String msg = "Internal Error! The sql-command is not found:";
            msg = msg + " sqlCommandKey=" + sqlCommandKey;
            msg = msg + " sqlCommands=" + _sqlCommands;
            throw new SystemException(msg);
        }

        protected bool IsOutsideSqlDaoMethodSelect(MethodInfo method) {
            return method.Name.StartsWith("Select");
        }

        protected bool IsOutsideSqlDaoMethodCall(MethodInfo method) {
            return method.Name.StartsWith("Call");
        }

        // ===============================================================================
        //                                                                 Assert Override
        //                                                                 ===============
        protected override void SetupMethodByAttribute(MethodInfo methodInfo) {
            String sql = _annotationReader.GetSql(methodInfo.Name, _dbms);
            AssertSQLAnnotationUnsupported(methodInfo, sql);
            base.SetupMethodByAttribute(methodInfo);
        }

        protected void AssertSQLAnnotationUnsupported(MethodInfo methodInfo, String sql) {
            if (sql != null) {
                String msg = "Sorry! The SQL annotation of S2Dao is unsupported on DBFlute:";
                msg = msg + " sql=" + sql + " method=" + methodInfo;
                throw new NotSupportedException(msg);
            }
        }

        protected override void SetupMethodByAuto(MethodInfo mi) {
            OutsideSql outsideSql = Attribute.GetCustomAttribute(mi, typeof(OutsideSql)) as OutsideSql;
            if (outsideSql != null) {
                String msg = "This method '" + mi.Name + "()' should use Outside Sql but the file was not found!";
                msg = msg + " Expected sql file name is '" + mi.DeclaringType.Name + "_" + mi.Name + ".sql'";
                throw new SystemException(msg);
            }
            base.SetupMethodByAuto(mi);
        }

        // ===============================================================================
        //                                                          ConditionBean Override
        //                                                          ======================
        protected override void SetupSelectMethodByAuto(MethodInfo methodInfo) {
            if (SetupInternalSelectMethodSequenceNextVal(methodInfo)) {
                return;
            }

            // Assert unsupported
            String query = _annotationReader.GetQuery(methodInfo.Name);
            AssertQueryAnnotationUnsupported(methodInfo, query);

            IDataReaderHandler handler = CreateDataReaderHandler(methodInfo);
            String[] argNames = MethodUtil.GetParameterNames(methodInfo);
            Type[] argTypes = MethodUtil.GetParameterTypes(methodInfo);
            SelectDynamicCommand cmd = CreateSelectDynamicCommand(handler);
            if (argTypes.Length == 1 && ValueTypes.GetValueType(argTypes[0]) == ValueTypes.OBJECT) {
                argNames = new String[] { "pmb" };
                AssertAutoQueryByDtoUnsupported(methodInfo, argTypes);
                S2DaoSelectDynamicCommand dynamicCommand = CreateCustomizeSelectDynamicCommand(handler);
                cmd = dynamicCommand;
            } else {
                HandleAutoQueryByArgsAnnotationUnsupported(methodInfo, argNames);
            }
            cmd.ArgNames = argNames;
            cmd.ArgTypes = argTypes;
            _sqlCommands[methodInfo.Name] = cmd;
        }

        protected bool SetupInternalSelectMethodSequenceNextVal(MethodInfo methodInfo) {
            if (!"SelectNextVal".Equals(methodInfo.Name)) {
                return false;
            }
            DBMeta dbmeta = FindDBMeta();
            if (!dbmeta.HasSequence) {
                String msg = "If the method 'SelectNextVal()' exists, DBMeta.HasSequence should return true:";
                msg = msg + " dbmeta.HasSequence=" + dbmeta.HasSequence + " method=" + methodInfo;
                throw new SystemException(msg);
            }
            String nextValueSql = dbmeta.SequenceNextValSql;
            if (nextValueSql == null) {
                String msg = "If the method 'SelectNextVal()' exists, nextValueSql should not be null:";
                msg = msg + " nextValueSql=" + nextValueSql + " method=" + methodInfo;
                throw new SystemException(msg);
            }
            SetupSelectMethodByManual(methodInfo, nextValueSql);
            return true;
        }

        protected void AssertQueryAnnotationUnsupported(MethodInfo methodInfo, String query) {
            if (query != null) {
                String msg = "Sorry! The QUERY annotation of S2Dao is unsupported on DBFlute:";
                msg = msg + " query=" + query + " method=" + methodInfo;
                throw new NotSupportedException(msg);
            }
        }

        protected void HandleAutoQueryByArgsAnnotationUnsupported(MethodInfo methodInfo, String[] argNames) {
            String msg = "Sorry! The auto query by ARGS annotation of S2Dao is unsupported on DBFlute:";
            msg = msg + " argNames=" + argNames + " method=" + methodInfo;
            throw new NotSupportedException(msg);
        }

        protected void AssertAutoQueryByDtoUnsupported(MethodInfo methodInfo, Type[] argTypes) {
            Type firstArgType = argTypes[0];
            if (!ConditionBeanContext.IsTheTypeConditionBean(firstArgType)) {
                String msg = "Sorry! The auto query by DTO of S2Dao is unsupported on DBFlute:";
                msg = msg + " dto=" + firstArgType + " method=" + methodInfo;
                throw new NotSupportedException(msg);
            }
        }

        // ===============================================================================
        //                                   Insert and Update and Delete By Auto Override
        //                                   =============================================
        protected override AbstractSqlCommand CreateInsertAutoDynamicCommand(MethodInfo methodInfo, IDataSource dataSource, ICommandFactory commandFactory, IBeanMetaData beanMetaData, string[] propertyNames) {
            return new InternalInsertAutoDynamicCommand(dataSource, commandFactory, beanMetaData, propertyNames);
        }
        
        protected override void SetupUpdateMethodByAuto(MethodInfo mi) {
            if (IsFirstArgumentConditionBean(mi)) {
                ISqlCommand cmd = new InternalUpdateQueryAutoDynamicCommand(_dataSource, _commandFactory);
                _sqlCommands.Add(mi.Name, cmd);
                return;
            }
            base.SetupUpdateMethodByAuto(mi);
        }

        protected override AbstractSqlCommand CreateUpdateAutoDynamicCommand(MethodInfo method, IDataSource dataSource, ICommandFactory commandFactory, IBeanMetaData beanMetaData, string[] propertyNames) {
            InternalUpdateAutoDynamicCommand cmd = new InternalUpdateAutoDynamicCommand(dataSource, commandFactory, CreateBeanMetaData4UpdateDeleteByAuto(method), propertyNames);
            cmd.VersionNoAutoIncrementOnMemory = IsUpdateVersionNoAutoIncrementOnMemory(method);
            cmd.IsCheckSingleRowUpdate = !IsNonstrictMethod(method);
            return cmd;
        }

        protected override AbstractSqlCommand CreateUpdateModifiedOnlyCommand(MethodInfo method, IDataSource dataSource, ICommandFactory commandFactory, IBeanMetaData beanMetaData, string[] propertyNames) {
            InternalUpdateModifiedOnlyCommand cmd = new InternalUpdateModifiedOnlyCommand(dataSource, commandFactory, CreateBeanMetaData4UpdateDeleteByAuto(method), propertyNames);
            cmd.VersionNoAutoIncrementOnMemory = IsUpdateVersionNoAutoIncrementOnMemory(method);
            cmd.IsCheckSingleRowUpdate = !IsNonstrictMethod(method);
            return cmd;
        }

        protected override void SetupDeleteMethodByAuto(MethodInfo mi) {
            if (IsFirstArgumentConditionBean(mi)) {
                ISqlCommand cmd = new InternalDeleteQueryAutoDynamicCommand(_dataSource, _commandFactory);
                _sqlCommands.Add(mi.Name, cmd);
                return;
            }
            base.SetupDeleteMethodByAuto(mi);
        }

        protected override AbstractSqlCommand CreateDeleteAutoStaticCommand(MethodInfo method, IDataSource dataSource, ICommandFactory commandFactory, IBeanMetaData beanMetaData, string[] propertyNames) {
            InternalDeleteAutoStaticCommand cmd = new InternalDeleteAutoStaticCommand(dataSource, commandFactory, CreateBeanMetaData4UpdateDeleteByAuto(method), propertyNames);
            cmd.IsCheckSingleRowUpdate = !IsNonstrictMethod(method);
            return cmd;
        }
        
        // -------------------------------------------------
        //                                     Common Helper
        //                                     -------------
        protected IBeanMetaData CreateBeanMetaData4UpdateDeleteByAuto(MethodInfo method) {
            if (IsNonstrictMethod(method)) {
                BeanMetaDataForUpdateNonConcurrency bmdNonConcurrency = new BeanMetaDataForUpdateNonConcurrency(_beanType, _annotationReaderFactory, false);
                bmdNonConcurrency.Initialize(_dbMetaData, _dbms);
                return bmdNonConcurrency;
            } else {
                return this.BeanMetaData;
            }
        }

        protected bool IsUpdateVersionNoAutoIncrementOnMemory(MethodInfo mi) {
            return !IsNonstrictMethod(mi);
        }

        protected bool IsNonstrictMethod(MethodInfo mi) {
            return mi.Name.Contains("Nonstrict");
        }

        protected bool IsFirstArgumentConditionBean(MethodInfo mi) {
            Type[] pmbTypes = MethodUtil.GetParameterTypes(mi);
            return pmbTypes.Length > 0 && typeof(ConditionBean).IsAssignableFrom(pmbTypes[0]);
        }

        // ===============================================================================
        //                                                             OutsideSql Override
        //                                                             ===================
        // -------------------------------------------------
        //                            Traditional OutsideSql
        //                            ----------------------
        protected override void SetupSelectMethodByManual(MethodInfo mi, string sql) {
            string[] parameterNames = MethodUtil.GetParameterNames(mi);
            Type[] parameterTypes = MethodUtil.GetParameterTypes(mi);
            string[] filteredParameterNames = null;
            Type[] filteredParameterTypes = null;
            if (parameterTypes != null && parameterTypes.Length > 0
                    && typeof(CursorHandler).IsAssignableFrom(parameterTypes[parameterTypes.Length - 1])) {
                filteredParameterNames = new string[parameterTypes.Length - 1];
                filteredParameterTypes = new Type[parameterTypes.Length - 1];
                for (int i = 0; i < parameterTypes.Length - 1; i++) {
                    filteredParameterNames[i] = parameterNames[i];
                    filteredParameterTypes[i] = parameterTypes[i];
                }
            } else {
                filteredParameterNames = parameterNames;
                filteredParameterTypes = parameterTypes;
            }
            IBeanMetaData myMetaData = GetOutsideSqlBeanMetaData(mi, _dbMetaData, _dbms);
            IDataReaderHandler myDataReaderHandler = CreateDataReaderHandler(mi, myMetaData);
            RegisterSqlCommand(mi.Name, mi, sql, filteredParameterNames, filteredParameterTypes, myDataReaderHandler);
        }

        protected IBeanMetaData GetOutsideSqlBeanMetaData(MethodInfo mi, IDatabaseMetaData databaseMetaData, Seasar.Dao.IDbms dbInfo) {
            Type beanClass4SelectMethodByManual = GetOutsideSqlDefaultBeanClass(mi);
            if (beanClass4SelectMethodByManual.Equals(_beanType)) {
                return _beanMetaData;
            }
            BeanMetaDataCacheExtension bmdExt = new BeanMetaDataCacheExtension(beanClass4SelectMethodByManual, _annotationReaderFactory, false);
            bmdExt.Initialize(databaseMetaData, dbInfo);// Don't use cache!
            return bmdExt;
        }

        // -------------------------------------------------
        //                              Specified OutsideSql
        //                              --------------------
        // - - - - - - - - - -
        //              Select
        //               - - -
        protected void SetupSpecifiedOutsideSqlSelectCommand(String sqlCommandKey, MethodInfo method, OutsideSqlContext outsideSqlContext) {
            // - - - - - - - - - - - - - - - - - - - - - - -
            // The attribute of Specified-OutsideSqlContext.
            // - - - - - - - - - - - - - - - - - - - - - - -
            String sql = outsideSqlContext.ReadFilteredOutsideSql(this.SqlFileEncoding, _dbms.Suffix);
            Object pmb = outsideSqlContext.ParameterBean;

            // - - - - - - - - - - - - - - -
            // The attribute of SqlCommand.
            // - - - - - - - - - - - - - - -
            String[] argNames = (pmb != null ? new String[] {"pmb"} : new String[]{});
            Type[] argTypes = (pmb != null ? new Type[] {pmb.GetType()} : new Type[]{});

            // - - - - - - - - - - - - - - - -
            // Create customized BeanMetaData.
            // - - - - - - - - - - - - - - - -
            Type lastestArguementType = method.GetParameters()[method.GetParameters().Length - 1].ParameterType;
            IDataReaderHandler myDataReaderHandler;
            if (typeof(Type).IsAssignableFrom(lastestArguementType)) {
                // - - - - - - - -
                // EntityHandling
                // - - - - - - - -
                Type customizeEntityType = outsideSqlContext.ResultType;
                IBeanMetaData myBeanMetaData = CreateSpecifiedOutsideSqlCustomizeBeanMetaData(customizeEntityType);
                Type retType = method.ReturnType;
                if (retType.IsGenericType && (retType.GetGenericTypeDefinition().Equals(typeof(System.Collections.Generic.IList<>)))) {
                    myDataReaderHandler = CreateSpecifiedOutsideSqlCustomizeBeanListResultSetHandler(myBeanMetaData, customizeEntityType);
                } else if (retType.Equals(typeof(System.Collections.IList))) {
                    // For the problem about DynamicProxy unsupporting generic method!
                    myDataReaderHandler = CreateSpecifiedOutsideSqlCustomizeBeanListResultSetHandler(myBeanMetaData, customizeEntityType);
                } else {
                    throw new NotSupportedException("The return type of method is unsupported: method.ReturnType=" + method.ReturnType);
                }
            } else if (typeof(CursorHandler).IsAssignableFrom(lastestArguementType)) {
                // - - - - - - - -
                // CursorHandling
                // - - - - - - - -
                IBeanMetaData myBeanMetaData = CreateSpecifiedOutsideSqlCursorBeanMetaData(method);
                myDataReaderHandler = CreateSpecifiedOutsideSqlCursorResultSetHandler(myBeanMetaData);
            } else {
                String msg = "The lastestArguementType is unsupported:";
                msg = msg + " lastestArguementType=" + lastestArguementType;
                msg = msg + " method=" + method;
                throw new SystemException(msg);
            }

            // - - - - - - - - - - -
            // Register Sql-Command.
            // - - - - - - - - - - -
            RegisterSqlCommand(sqlCommandKey, method, sql, argNames, argTypes, myDataReaderHandler);
        }

        protected IBeanMetaData CreateSpecifiedOutsideSqlCustomizeBeanMetaData(Type clazz) {
            BeanMetaDataCacheExtension bmdExt = new BeanMetaDataCacheExtension(clazz, this.AnnotationReaderFactory, false);
            bmdExt.Initialize(_dbMetaData, _dbms);// Don't use cache!
            return bmdExt;
        }

        protected IDataReaderHandler CreateSpecifiedOutsideSqlCustomizeBeanListResultSetHandler(IBeanMetaData specifiedBeanMetaData, Type customizeEntityType) {
            IValueType valueType = ValueTypes.GetValueType(customizeEntityType);
            if (valueType == null || !valueType.Equals(ValueTypes.OBJECT)) {
                // Non generic because it cannot add the null value to generic list by abstract type.
                return new InternalObjectListResultSetHandler(customizeEntityType, valueType);
            }
            InternalRowCreator rowCreator = CreateInternalRowCreator(specifiedBeanMetaData); // For performance turning!
            InternalRelationRowCreator relationRowCreator = CreateInternalRelationRowCreator(specifiedBeanMetaData);
            return new InternalBeanGenericListMetaDataResultSetHandler(specifiedBeanMetaData, rowCreator, relationRowCreator);
        }

        protected InternalRowCreator CreateInternalRowCreator(IBeanMetaData bmd) {
            Type clazz = bmd != null ? bmd.BeanType : null;
            return InternalRowCreator.CreateInternalRowCreator(clazz);
        }

        protected InternalRelationRowCreator CreateInternalRelationRowCreator(IBeanMetaData bmd) {
            return new InternalRelationRowCreator();
        }

        public class InternalObjectListResultSetHandler : IDataReaderHandler {
            private Type _beanType;
            private IValueType _valueType;
            public InternalObjectListResultSetHandler(Type beanType, IValueType valueType) {
                this._beanType = beanType;
                this._valueType = valueType;
            }
            public object Handle(IDataReader dataReader) {
                System.Collections.IList resultList = new System.Collections.ArrayList();
                while (dataReader.Read()) {
                    resultList.Add(_valueType.GetValue(dataReader, 0)); // It's zero origin.
                }
                return resultList;
            }
        }

        public class InternalObjectGenericListResultSetHandler : IDataReaderHandler {
            private Type _beanType;
            private IValueType _valueType;
            public InternalObjectGenericListResultSetHandler(Type beanType, IValueType valueType) {
                this._beanType = beanType;
                this._valueType = valueType;
            }
            public object Handle(IDataReader dataReader) {
                Type generic = typeof(System.Collections.Generic.List<>);
                Type constructed = generic.MakeGenericType(_beanType);
                System.Collections.IList resultList = (System.Collections.IList) Activator.CreateInstance(constructed);
                while (dataReader.Read()) {
                    resultList.Add(_valueType.GetValue(dataReader, 0)); // It's zero origin.
                }
                return resultList;
            }
        }

        protected IBeanMetaData CreateSpecifiedOutsideSqlCursorBeanMetaData(MethodInfo method) {
            BeanMetaDataCacheExtension bmdExt = new BeanMetaDataCacheExtension(GetOutsideSqlDefaultBeanClass(method), this.AnnotationReaderFactory, false);
            bmdExt.Initialize(_dbMetaData, _dbms);// Don't use cache!
            return bmdExt;
        }

        protected IDataReaderHandler CreateSpecifiedOutsideSqlCursorResultSetHandler(IBeanMetaData specifiedBeanMetaData) {
            return new ObjectDataReaderHandler();// This is dummy for cursor handling!
        }

        // - - - - - - - - - -
        //             Execute
        //             - - - -
        protected void SetupSpecifiedOutsideSqlExecuteCommand(String sqlCommandKey, MethodInfo method, OutsideSqlContext outsideSqlContext) {
            // - - - - - - - - - - - - - - - - - - - - - - -
            // The attribute of Specified-OutsideSqlContext.
            // - - - - - - - - - - - - - - - - - - - - - - -
            String sql = outsideSqlContext.ReadFilteredOutsideSql(this.SqlFileEncoding, _dbms.Suffix);
            Object pmb = outsideSqlContext.ParameterBean;

            // - - - - - - - - - - - - - - -
            // The attribute of SqlCommand.
            // - - - - - - - - - - - - - - -
            String[] argNames = (pmb != null ? new String[] {"pmb"} : new String[]{});
            Type[] argTypes = (pmb != null ? new Type[] {pmb.GetType()} : new Type[]{});

            InternalSpecifiedOusideSqlUpdateDynamicCommand cmd = new InternalSpecifiedOusideSqlUpdateDynamicCommand(_dataSource, _commandFactory);
            RegisterSqlCommand(sqlCommandKey, method, sql, argNames, argTypes, cmd);
        }

        protected class InternalSpecifiedOusideSqlUpdateDynamicCommand : InternalUpdateDynamicCommand {
            public InternalSpecifiedOusideSqlUpdateDynamicCommand(IDataSource dataSource, ICommandFactory factory)
                : base(dataSource, factory) {
            }
            public override Object Execute(Object[] args) {
                if (args.Length != 3) {
                    String msg = "Internal Error! OutsideSqlDao.execute() should have 3 arguements: args.length=" + args.Length;
                    throw new SystemException(msg);
                }
                Object arg = args[1];
                return base.Execute(new Object[]{arg});
            }
        }

        // - - - - - - - - - - - -
        //          Call Procedure
        //           - - - - - - -
        protected void SetupSpecifiedOutsideSqlCallCommand(String sqlCommandKey, MethodInfo method, OutsideSqlContext outsideSqlContext) {
            // - - - - - - - - - - - - - - - - - - - - - - -
            // The attribute of Specified-OutsideSqlContext.
            // - - - - - - - - - - - - - - - - - - - - - - -
            Object pmb = outsideSqlContext.ParameterBean;
            String procedureName = outsideSqlContext.OutsideSqlPath;

            // - - - - - - - - - - - - - - -
            // The attribute of SqlCommand.
            // - - - - - - - - - - - - - - -
            InternalProcedureMetaDataFactory myProcedureMetaDataFactory = new InternalProcedureMetaDataFactory();
            Type pmbType = pmb != null ? pmb.GetType() : null;
            InternalProcedureMetaData metaData = myProcedureMetaDataFactory.CreateProcedureMetaData(procedureName, pmbType);
            InternalProcedureCommand cmd = CreateInternalProcedureCommand(method, metaData);
            _sqlCommands.Add(sqlCommandKey, cmd);
        }

        protected InternalProcedureCommand CreateInternalProcedureCommand(MethodInfo method, InternalProcedureMetaData metaData) {
            return new InternalProcedureCommand(_dataSource, CreateDataReaderHandler(method), _commandFactory, _dataReaderFactory, metaData);
        }

        // -------------------------------------------------
        //                              Common of OutsideSql
        //                              --------------------
        protected Type GetOutsideSqlDefaultBeanClass(MethodInfo mi) {
            Type retType = mi.ReturnType;
            if (retType.IsGenericType && (retType.GetGenericTypeDefinition().Equals(typeof(System.Collections.Generic.IList<>)))) {
                return retType.GetGenericArguments()[0];
            } else if (!retType.IsGenericType && typeof(System.Collections.IList).IsAssignableFrom(retType)) {
                return _beanType;
            } else if (retType.IsArray) {
                return retType.GetElementType();
            } else if (ValueTypes.GetValueType(retType) != ValueTypes.OBJECT) {
                return _beanType;
            } else {
                return retType;
            }
        }

        protected void RegisterSqlCommand(String sqlCommandKey, MethodInfo method, String sql, String[] argNames, Type[] argTypes, IDataReaderHandler myDataReaderHandler) {
            SelectDynamicCommand cmd = CreateSelectDynamicCommand(myDataReaderHandler);
            RegisterSqlCommand(sqlCommandKey, method, sql, argNames, argTypes, cmd);
        }

        protected void RegisterSqlCommand(String sqlCommandKey, MethodInfo method, String sql, String[] argNames, Type[] argTypes, SelectDynamicCommand cmd) {
            cmd.Sql = sql;
            cmd.ArgNames = argNames;
            cmd.ArgTypes = argTypes;
            _sqlCommands.Add(sqlCommandKey, cmd);
        }

        protected void RegisterSqlCommand(String sqlCommandKey, MethodInfo method, String sql, String[] argNames, Type[] argTypes, InternalUpdateDynamicCommand cmd) {
            cmd.Sql = sql;
            cmd.ArgNames = argNames;
            cmd.ArgTypes = argTypes;
            _sqlCommands.Add(sqlCommandKey, cmd);
        }

        // ===============================================================================
        //                                                              Extension Override
        //                                                              ==================
        protected override SelectDynamicCommand CreateSelectDynamicCommand(IDataReaderHandler handler) {
            return CreateCustomizeSelectDynamicCommand(handler);
        }

        protected virtual S2DaoSelectDynamicCommand CreateCustomizeSelectDynamicCommand(IDataReaderHandler handler) {
            FetchNarrowingResultSetFactory customizeResultSetFactory = new FetchNarrowingResultSetFactory();
            return new S2DaoSelectDynamicCommand(_dataSource, _commandFactory, handler, customizeResultSetFactory);
        }

        protected override IRowCreator CreateRowCreator() {
            return new InternalRowCreator();
        }

        protected override IRelationRowCreator CreateRelationRowCreator() {
            return new InternalRelationRowCreator();
        }
        
        // ===============================================================================
        //                                                               ADO.NET Delegator
        //                                                               =================
        protected IDbms GetDbms(IDataSource dataSource) {
            try {
                return DbmsManager.GetDbms(dataSource);
            } catch (Exception e) {
                new DbExceptionHandler().HandleDbException(e, null);
                return null; // Unreachable!
            }
        }

        // ===============================================================================
        //                                                                   Assist Helper
        //                                                                   =============
        protected DBMeta FindDBMeta() {
            Type beanType = this.BeanType;
            if (beanType == null) {
                return null;
            }
            if (!typeof(Entity).IsAssignableFrom(beanType)) {
                return null;
            }
            Entity entity = (Entity)Activator.CreateInstance(beanType);
            return entity.DBMeta;
        }

        // ===============================================================================
        //                                                                BeanMetaData Map
        //                                                                ================
        public Map<Type, IBeanMetaData> BeanMetaDataCacheMap {
            get { return _beanMetaDataCacheMap; }
            set { _beanMetaDataCacheMap = value; }
        }
    }

    public class BeanMetaDataDBFluteDBMetaExtension : BeanMetaDataImpl {
        protected Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.List<IPropertyType> _primaryKeyList = new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.ArrayList<IPropertyType>();
        public BeanMetaDataDBFluteDBMetaExtension() 
            : base() {
        }
        protected override void SetupProperty(Type beanType, IDatabaseMetaData dbMetaData, IDbms dbms) {
            if (!IsEntity(beanType)) {
                base.SetupProperty(beanType, dbMetaData, dbms);
                return;
            }
            Entity entity = (Entity)ClassUtil.NewInstance(beanType);
            DBMeta dbmeta = entity.DBMeta;
            foreach (PropertyInfo pi in beanType.GetProperties()) {
                IPropertyType pt = null;
                RelnoAttribute relnoAttr = _beanAnnotationReader.GetRelnoAttribute(pi);
                if (relnoAttr != null) {
                    if (!_relation) {
                        IRelationPropertyType rpt = CreateRelationPropertyType(beanType, pi, relnoAttr, dbMetaData, dbms);
                        AddRelationPropertyType(rpt);
                    }
                } else {
                    if (pi.CanWrite) {
                        pt = CreatePropertyTypeExtension(pi, dbmeta);
                        if (pt != null) {
                            AddPropertyType(pt);
                            if (pt.IsPrimaryKey) {
                                _primaryKeyList.add(pt);
                            }
                        }
                    }
                }
                if (IdentifierGenerator == null) {
                    IDAttribute idAttr = _beanAnnotationReader.GetIdAttribute(pi, dbms);
                    if (idAttr != null) {
                        _identifierGenerator = Seasar.Dao.Id.IdentifierGeneratorFactory.CreateIdentifierGenerator(pi.Name, dbms, idAttr);
                        if (pt != null) {
                            _primaryKeys = new string[] { pt.ColumnName };
                            pt.IsPrimaryKey = true;
                        }
                    }
                }
            }
        }
        protected IPropertyType CreatePropertyTypeExtension(PropertyInfo pi, DBMeta dbmeta) {
            if (IsRelationProperty(pi, dbmeta)) {
                return null;
            }
            String columnName = GetPropertyTypeColumnName(pi);
            IValueType valueType = ValueTypes.GetValueType(pi.PropertyType);
            IPropertyType pt = new PropertyTypeImpl(pi, valueType, columnName);
            if (dbmeta.HasPrimaryKey && dbmeta.HasColumn(pt.ColumnName)) {
                if (dbmeta.FindColumnInfo(pt.ColumnName).IsPrimary) {
                    pt.IsPrimaryKey = true;
                }
            }
            pt.IsPersistent = IsPersistentProperty(pi, dbmeta);
            return pt;
        }
        protected String GetPropertyTypeColumnName(PropertyInfo pi) {
            String columnName = _beanAnnotationReader.GetColumn(pi);
            columnName = (columnName != null ? columnName : pi.Name);
            return columnName;
        }
        protected bool IsRelationProperty(PropertyInfo pi, DBMeta dbmeta) {
            String propertyName = pi.Name;
            if (dbmeta.HasForeign(propertyName) || dbmeta.HasReferrer(propertyName)) {
                return true;
            }
            return _beanAnnotationReader.GetRelnoAttribute(pi) != null;
        }
        protected bool IsPersistentProperty(PropertyInfo pi, DBMeta dbmeta) {
            String propertyName = pi.Name;
            if (dbmeta.HasColumn(propertyName) || _beanAnnotationReader.GetColumn(pi) != null) {
                if (!IsElementOfNoPersistentProps(pi)) {
                    return true;
                }
            }
            return false;
        }
        protected bool IsElementOfNoPersistentProps(PropertyInfo pi) {
            String propertyName = pi.Name;
            String[] props = _beanAnnotationReader.GetNoPersisteneProps();
            if (props != null && props.Length >= 0) {
                for (int i = 0; i < props.Length; ++i) {
                    if (props[i].Equals(propertyName)) {
                        return true;
                    }
                }
            }
            return false;
        }
        protected override void SetupDatabaseMetaData(Type beanType, IDatabaseMetaData dbMetaData, IDbms dbms) {
            if (IsEntity(beanType)) {
                SetupPrimaryKeyExtension(beanType, dbMetaData, dbms);
                return;
            }
            base.SetupDatabaseMetaData(beanType, dbMetaData, dbms);
        }
        protected void SetupPrimaryKeyExtension(Type beanType, IDatabaseMetaData dbMetaData, IDbms dbms) {
            // = = = = = = = = = = = = = = = = = = = = = = =
            // Set up _primaryKeys and _identifierGenerator!
            // = = = = = = = = = = = = = = = = = = = = = = =
            if (_primaryKeys == null || _primaryKeys.Length == 0) {
                _primaryKeys = new String[_primaryKeyList.size()];
                int index = 0;
                foreach (IPropertyType pt in _primaryKeyList) {
                    _primaryKeys[index] = pt.ColumnName;
                    ++index;
                }
            }
            if (_identifierGenerator == null) {
                _identifierGenerator = Seasar.Dao.Id.IdentifierGeneratorFactory.CreateIdentifierGenerator(null, dbms);
            }
        }
        protected bool IsEntity(Type beanType) {
            return typeof(Entity).IsAssignableFrom(beanType);
        }
        public override bool HasModifiedPropertyNamesPropertyName {
            get {
                return BeanType.GetProperty(_modifiedPropertyNamesPropertyName) != null;
            }
        }
        public override bool HasClearModifiedPropertyNamesMethodName {
            get {
                return BeanType.GetMethod(_clearModifiedPropertyNamesMethodName) != null;
            }
        }
        public override IDictionary GetModifiedPropertyNames(object bean) {
            String propertyName = _modifiedPropertyNamesPropertyName;
            if (!HasModifiedPropertyNamesPropertyName) {
                throw new NotFoundModifiedPropertiesRuntimeException(bean.GetType().Name, propertyName);
            }
            PropertyInfo modifiedPropertyType = BeanType.GetProperty(propertyName);
            object value = modifiedPropertyType.GetValue(bean, null);
            IDictionary names = (IDictionary)value;
            return names;
        }
        public override void ClearModifiedPropertyNames(object bean) {
            if (HasClearModifiedPropertyNamesMethodName) {
                MethodInfo mi = BeanType.GetMethod(ClearModifiedPropertyNamesMethodName);
                mi.Invoke(bean, null);
            } else if (HasModifiedPropertyNamesPropertyName) {
                PropertyInfo pi = BeanType.GetProperty(ModifiedPropertyNamesPropertyName);
                IDictionary modifiedPropertyNames = (IDictionary)pi.GetValue(bean, null);
                modifiedPropertyNames.Clear();
            }
        }
    }
    
    public class BeanMetaDataCacheExtension : BeanMetaDataDBFluteDBMetaExtension {
        protected Map<Type, IBeanMetaData> _metaMap;
        protected int _nestNo;
        
        public BeanMetaDataCacheExtension(Type beanType, IAnnotationReaderFactory annotationReaderFactory, bool relation) 
            : base() {
            this.BeanType = beanType;
            this._relation = relation;
            this.AnnotationReaderFactory = annotationReaderFactory;
        }
        
        public override void Initialize(IDatabaseMetaData dbMetaData, IDbms dbms) {
            if (_metaMap != null) {
                BeanMetaDataCacheHandler handler = new BeanMetaDataCacheHandler(_metaMap);
                Type myBeanClass = BeanType;
                if (handler.IsDBFluteEntity(myBeanClass)) {
                    IBeanMetaData cachedMeta = handler.GetMetaFromCache(myBeanClass);
                    if (cachedMeta == null) {
                        handler.AddMetaFromCache(myBeanClass, this);
                    }
                }
            }
            base.Initialize(dbMetaData, dbms);
        }

        protected override IBeanMetaData CreateRelationBeanMetaData(PropertyInfo propertyInfo, IDatabaseMetaData dbMetaData, IDbms dbms) {
            if (_metaMap != null) {
                BeanMetaDataCacheHandler handler = new BeanMetaDataCacheHandler(_metaMap);
                IBeanMetaData cachedBmd = handler.FindOrCreateCachedMetaIfNeeds(propertyInfo.PropertyType, AnnotationReaderFactory, dbMetaData, dbms);
                if (cachedBmd != null) {
                    return cachedBmd;
                }
            }
            bool isRelation = false;
            if (this.NestNo > 0) {
                isRelation = true;
            }
            BeanMetaDataCacheExtension bmdExt = new BeanMetaDataCacheExtension(propertyInfo.PropertyType, this.AnnotationReaderFactory, isRelation);
            bmdExt.NestNo = this.NestNo + 1;
            bmdExt.Initialize(dbMetaData, dbms);
            return bmdExt;
        }
        
        public Map<Type, IBeanMetaData> MetaMap {
            get { return _metaMap; }
            set { _metaMap = value; }
        }
        public int NestNo {
            get { return _nestNo; }
            set { _nestNo = value; }
        }
    }

    public class BeanMetaDataForUpdateNonConcurrency : BeanMetaDataDBFluteDBMetaExtension {
        public BeanMetaDataForUpdateNonConcurrency(Type beanType, IAnnotationReaderFactory annotationReaderFactory, bool relation) 
            : base() {
            this.BeanType = beanType;
            this._relation = relation;
            this.AnnotationReaderFactory = annotationReaderFactory;
        }
        public override bool HasVersionNoPropertyType {
            get { return false; }
        }

        public override bool HasTimestampPropertyType {
            get { return false; }
        }
    }

    public class BeanMetaDataCacheHandler {
        protected Map<Type, IBeanMetaData> _metaMap;
        public BeanMetaDataCacheHandler(Map<Type, IBeanMetaData> metaMap) {
            _metaMap = metaMap;
        }
        public IBeanMetaData FindOrCreateCachedMetaIfNeeds(Type beanClass, IAnnotationReaderFactory factory, IDatabaseMetaData dbMetaData, IDbms dbms) {
            if (IsDBFluteEntity(beanClass)) {
                IBeanMetaData cachedMeta = GetMetaFromCache(beanClass);
                if (cachedMeta != null) {
                    return cachedMeta;
                } else {
                    BeanMetaDataCacheExtension bmdExt = new BeanMetaDataCacheExtension(beanClass, factory, false);
                    bmdExt.MetaMap = _metaMap;
                    bmdExt.Initialize(dbMetaData, dbms);
                    return bmdExt;
                }
            }
            return null;
        }

        public IBeanMetaData FindCachedMeta(Type beanClass) {
            if (IsDBFluteEntity(beanClass)) {
                IBeanMetaData cachedMeta = GetMetaFromCache(beanClass);
                if (cachedMeta != null) {
                    return cachedMeta;
                }
            }
            return null;
        }

        public bool IsDBFluteEntity(Type beanClass) {
            return typeof(Entity).IsAssignableFrom(beanClass);
        }

        public IBeanMetaData GetMetaFromCache(Type beanClass) {
            lock (_metaMap) {
                return _metaMap.get(beanClass);
            }
        }
        public IBeanMetaData AddMetaFromCache(Type beanClass, IBeanMetaData metaData) {
            lock (_metaMap) {
                return _metaMap.put(beanClass, metaData);
            }
        }
    }
}
