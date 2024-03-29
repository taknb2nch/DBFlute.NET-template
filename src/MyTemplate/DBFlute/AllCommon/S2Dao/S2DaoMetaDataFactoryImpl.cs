
using System;
using System.Data;
using System.Collections;
using System.Reflection;

using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;
using Seasar.Dao;
using Seasar.Dao.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.RsHandler;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlLog;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao {

    public class S2DaoMetaDataFactoryImpl : Seasar.Dao.IDaoMetaDataFactory {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        private readonly Hashtable _daoMetaDataCache = new Hashtable();
        protected readonly IDataSource _dataSource;
        protected readonly ICommandFactory _commandFactory;
        protected readonly IDataReaderFactory _dataReaderFactory;
        protected readonly IAnnotationReaderFactory _readerFactory;
        protected IDatabaseMetaData _dbMetaData;
        protected String _sqlFileEncoding = "UTF-8";
        protected String[] _insertPrefixes;
        protected String[] _updatePrefixes;
        protected String[] _deletePrefixes;
		protected Map<Type, IBeanMetaData> _beanMetaDataCacheMap = new HashMap<Type, IBeanMetaData>();

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public S2DaoMetaDataFactoryImpl(IDataSource dataSource,
            ICommandFactory commandFactory, IAnnotationReaderFactory readerFactory,
            IDataReaderFactory dataReaderFactory) {
            _dataSource = dataSource;
            _commandFactory = commandFactory;
            _readerFactory = readerFactory;
            _dataReaderFactory = dataReaderFactory;

            HandleSqlLogRegistry();
            if (!DBFluteConfig.GetInstance().IsLocked) {
                DBFluteConfig.GetInstance().Lock();
            }
        }

        protected void HandleSqlLogRegistry() {
            if (DBFluteConfig.GetInstance().IsUseSqlLogRegistry) {
                StringBuilder sb = new StringBuilder();
                InternalSqlLogRegistryLocator.Instance = new InternalSqlLogRegistry();
                sb.append("{SqlLog Information}").append(GetLineSeparator());
                sb.append("  [SqlLogRegistry]").append(GetLineSeparator());
                sb.append("    ...Setting up SqlLogRegistry").append(GetLineSeparator());
                sb.append("    because the property 'IsUseSqlLogRegistry' of the config of DBFlute is true");
                _log.Info(sb.toString());
            } else {
                if (InternalSqlLogRegistryLocator.Instance != null) {
                    InternalSqlLogRegistryLocator.Instance = null;
                }
            }
        }

        // ===============================================================================
        //                                                                  Implementation
        //                                                                  ==============
        public IDaoMetaData GetDaoMetaData(Type daoType) {
            lock (this) {
                string key = daoType.FullName;
                IDaoMetaData dmd = (IDaoMetaData)_daoMetaDataCache[key];
                if (dmd != null) {
                    return dmd;
                }
                if (_log.IsDebugEnabled) {
                    _log.Debug("...Creating daoMetaData for '" + daoType.Name + "'.");
                }
                dmd = CreateDaoMetaData(daoType);
                _daoMetaDataCache[key] = dmd;
                return dmd;
            }
        }

        protected virtual IDaoMetaData CreateDaoMetaData(Type daoType) {
            S2DaoMetaDataExtension dmd = NewDaoMetaDataExtension();
            dmd.DaoType = daoType;
            dmd.DataSource = _dataSource;
            dmd.CommandFactory = _commandFactory;
            dmd.DataReaderFactory = _dataReaderFactory;
            dmd.DataReaderHandlerFactory = CreateInternalDataReaderHandlerFactory();
            dmd.AnnotationReaderFactory = _readerFactory;
            if (_dbMetaData == null) {
                _dbMetaData = new DatabaseMetaDataImpl(_dataSource);
            }
            dmd.DatabaseMetaData = _dbMetaData;
            if (_sqlFileEncoding != null) {
                dmd.SqlFileEncoding = _sqlFileEncoding;
            }
            if (_insertPrefixes != null) {
                dmd.InsertPrefixes = _insertPrefixes;
            }
            if (_updatePrefixes != null) {
                dmd.UpdatePrefixes = _updatePrefixes;
            }
            if (_deletePrefixes != null) {
                dmd.DeletePrefixes = _deletePrefixes;
            }
			dmd.BeanMetaDataCacheMap = _beanMetaDataCacheMap;
            dmd.Initialize();
            return dmd;
        }

        protected virtual S2DaoMetaDataExtension NewDaoMetaDataExtension() {
            return new S2DaoMetaDataExtension();
        }

        protected virtual InternalDataReaderHandlerFactory CreateInternalDataReaderHandlerFactory() {
            return new InternalDataReaderHandlerFactory();
        }

        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IDatabaseMetaData DBMetaData {
            set { _dbMetaData = value; }
        }

        public String[] InsertPrefixes {
            set { _insertPrefixes = value; }
        }

        public String[] UpdatePrefixes {
            set { _updatePrefixes = value; }
        }

        public String[] DeletePrefixes {
            set { _deletePrefixes = value; }
        }

        public String SqlFileEncoding {
            set { _sqlFileEncoding = value; }
        }
    }

    // ===================================================================================
    //                                                                           Procedure
    //                                                                           =========
    public class InternalProcedureMetaData {
        protected String _procedureName;
        protected LinkedHashMap<String, InternalProcedureParameterType> parameterTypeMap = new LinkedHashMap<String, InternalProcedureParameterType>();
        protected Type _returnType;
        public InternalProcedureMetaData(String procedureName) {
            _procedureName = procedureName;
        }
        public String ProcedureName { get {
            return _procedureName;
        } set {
            _procedureName = value;
        }}
        public InternalProcedureParameterType GetParameterType(int index) {
            return parameterTypeMap.get(index);
        }
        public InternalProcedureParameterType GetParameterType(String parameterName) {
            return parameterTypeMap.get(parameterName.ToLower());
        }
        public int ParameterTypeSize { get {
            return parameterTypeMap.size();
        }}
        public bool HasReturnParameterType { get {
            return _returnType != null;
        }}
        public Type ReturnParameterType { get {
            return _returnType;
        }}
        public void AddParameterType(InternalProcedureParameterType parameterType) {
            String name = parameterType.ParameterName;
            parameterTypeMap.put(name.ToLower(), parameterType);
            if (parameterType.IsReturnType) {
                _returnType = parameterType.ParameterPropertyType;
            }
        }
    }

    public class InternalProcedureParameterType {
        protected String _parameterName;
        protected PropertyInfo _parameterProperty;
        protected Type _parameterPropertyType;
        protected ParameterDirection _parameterDirectionType;
        protected bool _inType;
        protected bool _outType;
        protected bool _returnType;
        public InternalProcedureParameterType(String parameterName, PropertyInfo parameterProperty) {
            _parameterName = parameterName;
            _parameterProperty = parameterProperty;
            _parameterPropertyType = parameterProperty.PropertyType;
        }
        public Object GetValue(Object dto) {
            return _parameterProperty.GetValue(dto, null);
        }
        public void SetValue(Object dto, Object value) {
            _parameterProperty.SetValue(dto, value, null);
        }
        public String ParameterName { get { return _parameterName; } }
        public PropertyInfo ParameterProperty { get { return _parameterProperty; } }
        public Type ParameterPropertyType { get { return _parameterPropertyType; } }
        public ParameterDirection ParameterDirectionType { get { return _parameterDirectionType; } set { _parameterDirectionType = value; } }
        public bool IsInType { get { return _inType; } set { _inType = value; } }
        public bool IsOutType { get { return _outType; } set { _outType = value; } }
        public bool IsReturnType { get { return _returnType; } set { _returnType = value; } }
    }

    public class InternalProcedureMetaDataFactory {
        protected InternalFieldProcedureAnnotationReader _annotationReader = new InternalFieldProcedureAnnotationReader();
        public InternalProcedureMetaData CreateProcedureMetaData(String procedureName, Type pmbType) {
            InternalProcedureMetaData metaData = new InternalProcedureMetaData(procedureName);
            if (pmbType == null) {
                return metaData;
            } else {
                if (!IsDtoType(pmbType)) {
                    throw new IllegalStateException("The pmb type is Not DTO type: " + pmbType.Name);
                }
            }
            RegisterParameterType(metaData, pmbType, pmbType.GetProperties());
            return metaData;
        }
        protected void RegisterParameterType(InternalProcedureMetaData metaData, Type pmbType, PropertyInfo[] properties) {
            foreach (PropertyInfo property in properties) {
                InternalProcedureParameterType ppt = GetProcedureParameterType(pmbType, property);
                if (ppt == null) {
                    continue;
                }
                metaData.AddParameterType(ppt);
            }
        }
        protected InternalProcedureParameterType GetProcedureParameterType(Type pmbType, PropertyInfo property) {
            InternalProcedureParameterInfo info = _annotationReader.GetProcedureParameter(pmbType, property);
            if (info == null) {
                return null;
            }
            String name = info.ParameterName;
            String type = info.ParameterType;
            type = type.ToLower();
            InternalProcedureParameterType ppt = new InternalProcedureParameterType(name, property);
            if (type.Equals("in")) {
                ppt.IsInType = true;
                ppt.ParameterDirectionType = ParameterDirection.Input;
            } else if (type.Equals("out")) {
                ppt.IsOutType = true;
                ppt.ParameterDirectionType = ParameterDirection.Output;
            } else if (type.Equals("inout")) {
                ppt.IsInType = true;
                ppt.IsOutType = true;
                ppt.ParameterDirectionType = ParameterDirection.InputOutput;
            } else if (type.Equals("return")) {
                // *Set false to IsOutType
                // The return is not out-parameter at ADO.NET!
                // though JDBC treats it as out-parameter.
                // ppt.IsOutType = true;
                ppt.IsReturnType = true;
                ppt.ParameterDirectionType = ParameterDirection.ReturnValue;
            } else {
                throw new IllegalStateException("The parameter type is wrong: type=" + type);
            }
            return ppt;
        }
        protected bool IsDtoType(Type clazz) {
            return true; // No check because no time.
        }
    }

    public class InternalFieldProcedureAnnotationReader {
        protected String PROCEDURE_PARAMETER_SUFFIX;
        public InternalFieldProcedureAnnotationReader() {
            PROCEDURE_PARAMETER_SUFFIX = "_PROCEDURE_PARAMETER";
        }
        public InternalProcedureParameterInfo GetProcedureParameter(Type pmbType, PropertyInfo property) {
            String propertyName = property.Name;
            String annotationName = InitUncap(propertyName) + PROCEDURE_PARAMETER_SUFFIX;
            FieldInfo field = pmbType.GetField(annotationName, BindingFlags.Public | BindingFlags.Static);
            if (field != null) {
                String annotationValue = (String)field.GetValue(null);
                InternalProcedureParameterInfo info = new InternalProcedureParameterInfo();
                String[] values = annotationValue.Split(',');
                if (values.Length != 2) {
                    String msg = "The value of annotation is wrong.";
                    msg = msg + " You should set '[parameterName], [parameterType]'.";
                    msg = msg + " But: annotation=" + annotationName + " value=" + annotationValue;
                    throw new IllegalStateException(msg);
                }
                info.ParameterName = values[0].Trim();
                info.ParameterType = values[1].Trim();
                return info;
            } else {
                return null;
            }
        }
        protected String InitUncap(String str) {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
    }

    public class InternalProcedureParameterInfo {
        protected String _parameterName;
        protected String _parameterType;
        public String ParameterName { get { return _parameterName; } set { _parameterName = value; } }
        public String ParameterType { get { return _parameterType; } set { _parameterType = value; } }
    }

    // ===================================================================================
    //                                                                         Row Creator
    //                                                                         ===========
    public class InternalRowCreator : RowCreatorImpl {

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected DBMeta _dbmeta;
        public DBMeta DBMeta { set { _dbmeta = value; } }

        public static InternalRowCreator CreateInternalRowCreator(Type beanClass) {
            InternalRowCreator rowCreator = new InternalRowCreator();
            if (beanClass != null) {
                rowCreator.DBMeta = FindDBMetaByClass(beanClass);
            }
            return rowCreator;
        }

        protected static DBMeta FindDBMetaByClass(Type beanClass) {
            Object instance = NewInstance(beanClass);
            if (!(instance is Entity)) {
                return null;
            }
            return ((Entity)instance).DBMeta;
        }

        protected static Object NewInstance(Type clazz) {
            return ClassUtil.NewInstance(clazz);
        }

        public override Object CreateRow(IDataReader reader, IColumnMetaData[] columns, Type beanType) {
            if (columns.Length == 0) {
                String msg = "The propertyCache should not be empty: bean=" + beanType.Name;
                throw new IllegalStateException(msg);
            }
            String columnName = null;
            String propertyName = null;
            Object selectedValue = null;
            Map<String, int?> selectIndexMap = GetSelectIndexMap();
            Object row;
            DBMeta dbmeta;
            if (_dbmeta != null) {
                dbmeta = _dbmeta;
                row = dbmeta.NewEntity();
            } else {
                row = NewBean(beanType);
                dbmeta = FindCachedDBMeta(row);
            }
            try {
                if (dbmeta != null) {
                    foreach (IColumnMetaData column in columns) {
                        columnName = column.ColumnName;
                        propertyName = column.PropertyInfo.Name;
                        selectedValue = GetValue(reader, columnName, column.ValueType, selectIndexMap);
                        if (dbmeta.HasEntityPropertySetupper(propertyName)) {
                            dbmeta.SetupEntityProperty(propertyName, row, selectedValue);
                        } else {
                            column.PropertyInfo.SetValue(row, selectedValue, null);
                        }
                    }
                } else {
                    foreach (IColumnMetaData column in columns) {
                        columnName = column.ColumnName;
                        propertyName = column.PropertyInfo.Name;
                        selectedValue = GetValue(reader, columnName, column.ValueType, selectIndexMap);
                        column.PropertyInfo.SetValue(row, selectedValue, null);
                    }
                }
                return row;
            } catch (Exception e) {
                if (_log.IsDebugEnabled) {
                    String msg = "Failed to get selected values while resultSet handling:";
                    msg = msg + " target=" + beanType.Name + "." + propertyName;
                    _log.Debug(msg + " -> " + e.StackTrace); // because C# loses nested stack trace
                }
                throw;
            }
        }

        protected Map<String, int?> GetSelectIndexMap() { // ResourceContext in Java
            if (!ConditionBeanContext.IsExistConditionBeanOnThread()) {
                return null;
            }
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.getSelectIndexMap();
        }

        protected Object GetValue(IDataReader rs, String columnName, IValueType valueType,
                Map<String, int?> selectIndexMap) { // ResourceContext in Java
            // lower because of @see AbstractSqlClause
            int? selectIndex = selectIndexMap != null ? selectIndexMap.get(columnName.ToLower()) : null;
            if (selectIndex != null) {
                return valueType.GetValue(rs, selectIndex.Value);
            } else {
                return valueType.GetValue(rs, columnName);
            }
        }

		public static DBMeta FindCachedDBMeta(Object row) {
		    return DBMetaCacheHandler.FindDBMeta(row);
        }

		public static DBMeta FindCachedDBMeta(Type rowType, String tableName) {
		    return DBMetaCacheHandler.FindDBMeta(rowType, tableName);
        }

        protected override string FindColumnName(IList columnNames, string columnName) {
            columnName = RemoveQuoteIfExists(columnName);
            foreach (string realColumnName in columnNames) {
                if (string.Compare(realColumnName, columnName, true) == 0) {
                    return realColumnName;
                }
            }
            return null;
        }

        protected String RemoveQuoteIfExists(String name) {
            if (name.StartsWith("\"") && name.EndsWith("\"")) {
                name = name.Substring(1);
                name = name.Substring(0, name.Length - 1);
            } else if (name.StartsWith("[") && name.EndsWith("]")) {
                name = name.Substring(1);
                name = name.Substring(0, name.Length - 1);
            }
            return name;
        }
    }

	public class DBMetaCacheHandler {
	    protected static readonly String DBMETA_CACHE_KEY = "df:DBMetaCache";
		
		public static DBMeta FindDBMeta(Object row) {
		    if (!(row is Entity)) {
			    return null;
			}
			Entity entity = (Entity)row;
            DBMeta dbmeta = FindCachedDBMeta(entity.GetType());
			if (dbmeta != null) {
			    return dbmeta;
			}
            dbmeta = entity.DBMeta;
            CacheDBMeta(entity, dbmeta);
		    return dbmeta;
        }
        
        public static DBMeta FindDBMeta(Type rowType, String tableName) {
            DBMeta dbmeta = FindCachedDBMeta(rowType);
            if (dbmeta != null) {
                return dbmeta;
            }
            try {
                dbmeta = DBMetaInstanceHandler.FindDBMeta(tableName);
            } catch (DBMetaNotFoundException) {
                return null;
            }
            CacheDBMeta(rowType, dbmeta);
            return dbmeta;
        }

        protected static DBMeta FindCachedDBMeta(Type rowType) {
            System.Collections.Generic.IDictionary<Type, DBMeta> dbmetaCache = FindDBMetaCache();
            if (dbmetaCache == null) {
                dbmetaCache = new System.Collections.Generic.Dictionary<Type, DBMeta>();
                InternalMapContext.SetObject(DBMETA_CACHE_KEY, dbmetaCache);
            }
            if (dbmetaCache.ContainsKey(rowType)) {
                return dbmetaCache[rowType];
            }
            return null;
        }
        
        protected static void CacheDBMeta(Entity entity, DBMeta dbmeta) {
            CacheDBMeta(entity.GetType(), dbmeta);
        }
        
        protected static void CacheDBMeta(Type type, DBMeta dbmeta) {
            System.Collections.Generic.IDictionary<Type, DBMeta> dbmetaCache = FindDBMetaCache();
            dbmetaCache.Add(type, dbmeta);
        }
        
        protected static System.Collections.Generic.IDictionary<Type, DBMeta> FindDBMetaCache() {
            return (System.Collections.Generic.IDictionary<Type, DBMeta>)InternalMapContext.GetObject(DBMETA_CACHE_KEY);
        }
	}
	
    public class InternalRelationRowCreator : RelationRowCreatorImpl {

        protected override void SetupRelationKeyValue(RelationRowCreationResource res) {
            IRelationPropertyType rpt = res.RelationPropertyType;
            IBeanMetaData bmd = rpt.BeanMetaData;
            DBMeta dbmeta = FindDBMeta(bmd.BeanType, bmd.TableName);
            for (int i = 0; i < rpt.KeySize; ++i) {
                String columnName = rpt.GetMyKey(i) + res.BaseSuffix;

                if (!res.ContainsColumnName(columnName)) {
                    continue;
                }
                if (!res.HasRowInstance()) {
                    Object row;
                    if (dbmeta != null) {
                        row = dbmeta.NewEntity();
                    } else {
                        row = NewRelationRow(rpt);
                    }
                    res.Row = row;
                }
                if (!res.ContainsRelKeyValueIfExists(columnName)) {
                    continue;
                }
                Object value = res.ExtractRelKeyValue(columnName);
                if (value == null) {
                    // basically no way
                    // because this is not called if the referred value
                    // is null (then it must be no relation key)
                    // @see InternalBeanListMetaDataResultSetHandler
                    continue;
                }

                String yourKey = rpt.GetYourKey(i);
                IPropertyType pt = bmd.GetPropertyTypeByColumnName(yourKey);
                PropertyInfo pi = pt.PropertyInfo;
                pi.SetValue(res.Row, value, null);
                continue;
            }
        }

        protected Object CreateRelationRowInstance(DBMeta dbmeta) {
            if (dbmeta != null) {
                return dbmeta.NewEntity();
            }
            return null;
        }

        protected DBMeta FindDBMeta(Type rowType, String tableName) {
            return InternalRowCreator.FindCachedDBMeta(rowType, tableName);
        }

        protected override void SetupRelationAllValue(RelationRowCreationResource res) {
            System.Collections.Generic.IDictionary<String, IPropertyType> propertyCacheElement = res.ExtractPropertyCacheElement();
            System.Collections.Generic.ICollection<String> columnNameCacheElementKeySet = propertyCacheElement.Keys;
            foreach (String columnName in columnNameCacheElementKeySet) {
                IPropertyType pt = propertyCacheElement[columnName];
                res.CurrentPropertyType = pt;
                if (!IsValidRelationPerPropertyLoop(res)) {
                    res.ClearRowInstance();
                    return;
                }
                SetupRelationProperty(res);
            }
            if (!IsValidRelationAfterPropertyLoop(res)) {
                res.ClearRowInstance();
                return;
            }
            res.ClearValidValueCount();
            if (res.HasNextRelationProperty() && (HasConditionBean(res) || res.HasNextRelationLevel())) {
                SetupNextRelationRow(res);
            }
        }

        protected override void RegisterRelationValue(RelationRowCreationResource res, String columnName) {
            IPropertyType pt = res.CurrentPropertyType;
            Object value = null;
            if (res.ContainsRelKeyValueIfExists(columnName)) {
                // if this column is relation key, it gets the value from relation key values
                // for performance and avoiding twice getting same column value
                value = res.ExtractRelKeyValue(columnName);
            } else {
                IValueType valueType = pt.ValueType;
                Map<String, int?> selectIndexMap = GetSelectIndexMap();
                if (selectIndexMap != null) {
                    value = GetValue(res.DataReader, columnName, valueType, selectIndexMap);
                } else {
                    value = valueType.GetValue(res.DataReader, columnName);
                }
            }
            if (value != null) {
                res.IncrementValidValueCount();
				DBMeta dbmeta = FindDBMeta(res.Row);
				String propertyName = pt.PropertyName;
				if (dbmeta != null && dbmeta.HasEntityPropertySetupper(propertyName)) {
				    dbmeta.SetupEntityProperty(propertyName, res.Row, value);
			    } else {
                    PropertyInfo pd = pt.PropertyInfo;
                    pd.SetValue(res.Row, value, null);
				}
            }
        }

        protected Map<String, int?> GetSelectIndexMap() { // RelationRowCreationResource in Java
            if (!ConditionBeanContext.IsExistConditionBeanOnThread()) {
                return null;
            }
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            return cb.SqlClause.getSelectIndexMap();
        }

        protected Object GetValue(IDataReader rs, String columnName, IValueType valueType,
                Map<String, int?> selectIndexMap) { // ResourceContext in Java
            // lower because of @see AbstractSqlClause
            int? selectIndex = selectIndexMap != null ? selectIndexMap.get(columnName.ToLower()) : null;
            if (selectIndex != null) {
                return valueType.GetValue(rs, selectIndex.Value);
            } else {
                return valueType.GetValue(rs, columnName);
            }
        }

		protected DBMeta FindDBMeta(Object row) {
		    return InternalRowCreator.FindCachedDBMeta(row);
        }

        protected override void SetupPropertyCache(RelationRowCreationResource res) {
            // - - - - - - - - - - - 
            // Recursive Call Point!
            // - - - - - - - - - - -
            res.InitializePropertyCacheElement();

            // Do only selected foreign property for performance if condition-bean exists.
            if (HasConditionBean(res) && !HasSelectedForeignInfo(res)) {
                return;
            }

            // Set up property cache about current beanMetaData.
            IBeanMetaData nextBmd = res.GetRelationBeanMetaData();
            for (int i = 0; i < nextBmd.PropertyTypeSize; ++i) {
                IPropertyType pt = nextBmd.GetPropertyType(i);
                res.CurrentPropertyType = pt;
                if (!IsTargetProperty(res)) {
                    continue;
                }
                SetupPropertyCacheElement(res);
            }

            // Set up next relation.
            if (res.HasNextRelationProperty() && (HasConditionBean(res) || res.HasNextRelationLevel())) {
                res.BackupRelationPropertyType();
                res.IncrementCurrentRelationNestLevel();
                try {
                    SetupNextPropertyCache(res, nextBmd);
                } finally {
                    res.RestoreRelationPropertyType();
                    res.DecrementCurrentRelationNestLevel();
                }
            }
        }

        protected override bool IsTargetProperty(RelationRowCreationResource res) {
            IPropertyType pt = res.CurrentPropertyType;
            if (!pt.PropertyInfo.CanWrite) {
                return false;
            }
            if (typeof(System.Collections.Generic.IList<>).IsAssignableFrom(pt.PropertyInfo.GetType())) {
                return false;
            }
            return true;
        }

        protected override bool IsCreateDeadLink() {
            return false;
        }

        protected override int GetLimitRelationNestLevel() {
            return 2;
        }

        protected bool HasConditionBean(RelationRowCreationResource res) {
            return ConditionBeanContext.IsExistConditionBeanOnThread();
        }

        protected bool HasSelectedForeignInfo(RelationRowCreationResource res) {
            ConditionBean cb = ConditionBeanContext.GetConditionBeanOnThread();
            if (cb.SqlClause.hasSelectedForeignInfo(res.RelationNoSuffix)) {
                return true;
            }
            return false;
        }
    }

    // ===================================================================================
    //                                                                 Data Reader Handler
    //                                                                 ===================
    public class InternalDataReaderHandlerFactory : IDataReaderHandlerFactory {
        public virtual IDataReaderHandler GetResultSetHandler(Type beanType, IBeanMetaData bmd, MethodInfo mi) {
            Type retType = mi.ReturnType;
            if (typeof(DataSet).IsAssignableFrom(retType)) {
                return CreateBeanDataSetMetaDataDataReaderHandler(bmd, retType);
            } else if (typeof(DataTable).IsAssignableFrom(retType)) {
                return CreateBeanDataTableMetaDataDataReaderHandler(bmd, retType);
            } else if (retType.IsArray) {
                Type elementType = retType.GetElementType();
                if (AssignTypeUtil.IsSimpleType(elementType)) {
                    return CreateObjectArrayDataReaderHandler(elementType);
                } else {
                    return CreateBeanArrayMetaDataDataReaderHandler(bmd);
                }
            } else if (AssignTypeUtil.IsList(retType)) {
                if (AssignTypeUtil.IsSimpleType(beanType)) {
                    return CreateObjectListDataReaderHandler();
                } else {
                    return CreateBeanListMetaDataDataReaderHandler(bmd);
                }
            } else if (IsBeanTypeAssignable(beanType, retType)) {
                return CreateBeanMetaDataDataReaderHandler(bmd);
            } else if (AssignTypeUtil.IsGenericList(retType)) {
                Type elementType = retType.GetGenericArguments()[0];
                if (AssignTypeUtil.IsSimpleType(elementType)) {
                    return CreateObjectGenericListDataReaderHandler(elementType);
                } else {
                    return CreateBeanGenericListMetaDataDataReaderHandler(bmd);
                }
            } else {
                return CreateObjectDataReaderHandler();
            }
        }

        protected virtual IDataReaderHandler CreateObjectGenericListDataReaderHandler(Type elementType) {
            return new ObjectGenericListDataReaderHandler(elementType);
        }

        protected virtual IDataReaderHandler CreateObjectListDataReaderHandler() {
            return new ObjectListDataReaderHandler();
        }

        protected virtual IDataReaderHandler CreateObjectArrayDataReaderHandler(Type elementType) {
            return new ObjectArrayDataReaderHandler(elementType);
        }

        protected virtual IDataReaderHandler CreateBeanDataSetMetaDataDataReaderHandler(IBeanMetaData bmd, Type returnType) {
            return new BeanDataSetMetaDataDataReaderHandler(returnType);
        }

        protected virtual IDataReaderHandler CreateBeanDataTableMetaDataDataReaderHandler(IBeanMetaData bmd, Type returnType) {
            return new BeanDataTableMetaDataDataReaderHandler(returnType);
        }

        protected virtual IDataReaderHandler CreateBeanListMetaDataDataReaderHandler(IBeanMetaData bmd) {
            return new BeanListMetaDataDataReaderHandler(bmd, CreateRowCreator(), CreateRelationRowCreator());
        }

        protected virtual IDataReaderHandler CreateBeanMetaDataDataReaderHandler(IBeanMetaData bmd) {
            return new BeanMetaDataDataReaderHandler(bmd, CreateRowCreator(), CreateRelationRowCreator());
        }

        // DBFlute Target (but unused actually)
        protected virtual IDataReaderHandler CreateBeanArrayMetaDataDataReaderHandler(IBeanMetaData bmd) {
            InternalRowCreator rowCreator = CreateInternalRowCreator(bmd);
            InternalRelationRowCreator relationRowCreator = CreateInternalRelationRowCreator(bmd);
            return new InternalBeanArrayMetaDataResultSetHandler(bmd, rowCreator, relationRowCreator);
        }

        // DBFlute Target
        protected virtual IDataReaderHandler CreateBeanGenericListMetaDataDataReaderHandler(IBeanMetaData bmd) {
            InternalRowCreator rowCreator = CreateInternalRowCreator(bmd);
            InternalRelationRowCreator relationRowCreator = CreateInternalRelationRowCreator(bmd);
            return new InternalBeanGenericListMetaDataResultSetHandler(bmd, rowCreator, relationRowCreator);
        }

        protected virtual IDataReaderHandler CreateObjectDataReaderHandler() {
            return new ObjectDataReaderHandler();
        }

        protected virtual IRowCreator CreateRowCreator() {
            return CreateInternalRowCreator(null);
        }

        protected virtual IRelationRowCreator CreateRelationRowCreator() {
            return CreateInternalRelationRowCreator(null);
        }

        protected virtual bool IsBeanTypeAssignable(Type beanType, Type type) {
            return beanType.IsAssignableFrom(type) || type.IsAssignableFrom(beanType);
        }

        protected InternalRowCreator CreateInternalRowCreator(IBeanMetaData bmd) {
            Type clazz = bmd != null ? bmd.BeanType : null;
            return InternalRowCreator.CreateInternalRowCreator(clazz);
        }

        protected InternalRelationRowCreator CreateInternalRelationRowCreator(IBeanMetaData bmd) {
            return new InternalRelationRowCreator(); // Not yet implemented about performance tuning!
        }
    }
}
