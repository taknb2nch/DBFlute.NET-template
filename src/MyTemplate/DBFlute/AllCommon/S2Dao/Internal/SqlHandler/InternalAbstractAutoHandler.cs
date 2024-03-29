
using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;
using Seasar.Framework.Log;
using Seasar.Framework.Util;
using Seasar.Dao;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler {

    public abstract class InternalAbstractAutoHandler : InternalBasicHandler {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected readonly IBeanMetaData _beanMetaData;
        protected Object[] _bindVariables;
        protected Type[] _bindVariableTypes;
        protected DateTime _timestamp = DateTime.MinValue;
        protected Int32 _versionNo = Int32.MinValue;
        protected IPropertyType[] _propertyTypes;
        protected bool _versionNoAutoIncrementOnMemory = true;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalAbstractAutoHandler(IDataSource dataSource, ICommandFactory commandFactory,
            IBeanMetaData beanMetaData, IPropertyType[] propertyTypes) : base(dataSource, null, commandFactory) {
            // The property of Sql is treated as lazy setting at this class!
            DataSource = dataSource;// This is for this new method! 
            _beanMetaData = beanMetaData;
            _propertyTypes = propertyTypes;
        }

        // ===============================================================================
        //                                                                         Execute
        //                                                                         =======
        public int Execute(Object[] args) {
            IDbConnection connection = Connection;
            try {
                return Execute(connection, args[0]);
            } finally {
                ConnectionHolderDataSource holderDataSoure = DataSource as ConnectionHolderDataSource;
                holderDataSoure.ReleaseConnection();
                holderDataSoure.CloseConnection(connection);
            }
        }

        public virtual int Execute(Object[] args, Type[] argTypes) {
            return Execute(args);
        }

        public virtual int Execute(Object[] args, Type[] argTypes, String[] argNames) {
            return Execute(args);
        }

        protected virtual int Execute(IDbConnection connection, object bean) {
            PreUpdateBean(bean);
            SetupBindVariables(bean);
            LogSql(_bindVariables, _bindVariableTypes);
            IDbCommand cmd = Command(connection);
            int ret = -1;
            try {
                BindArgs(cmd, _bindVariables, _bindVariableTypes);
                ret = ExecuteUpdate(cmd);
            } finally {
                Close(cmd);
            }
            PostUpdateBean(bean, ret);
            return ret;
        }

        // ===============================================================================
        //                                                                   Assist Helper
        //                                                                   =============
        protected virtual void PreUpdateBean(Object bean) {
        }

        protected virtual void PostUpdateBean(Object bean, int ret) {
        }

        protected abstract void SetupBindVariables(object bean);

        protected void SetupInsertBindVariables(object bean) {
            ArrayList varList = new ArrayList();
            ArrayList varTypeList = new ArrayList();
            for (int i = 0; i < _propertyTypes.Length; ++i) {
                IPropertyType pt = _propertyTypes[i];
                if (string.Compare(pt.PropertyName, BeanMetaData.TimestampPropertyName, true) == 0) {
                    Timestamp = DateTime.Now;
                    SetupTimestampVariableList(varList, pt);
                } else if (pt.PropertyName.Equals(BeanMetaData.VersionNoPropertyName)) {
                    VersionNo = 0;
                    varList.Add(ConversionUtil.ConvertTargetType(VersionNo, pt.PropertyInfo.PropertyType));
                } else {
                    varList.Add(pt.PropertyInfo.GetValue(bean, null));
                }
                varTypeList.Add(pt.PropertyInfo.PropertyType);
            }
            BindVariables = varList.ToArray();
            BindVariableTypes = (Type[]) varTypeList.ToArray(typeof(Type));
        }

        protected void SetupUpdateBindVariables(Object bean) {
            ArrayList varList = new ArrayList();
            ArrayList varTypeList = new ArrayList();
            for (int i = 0; i < _propertyTypes.Length; ++i) {
                IPropertyType pt = _propertyTypes[i];
                if (string.Compare(pt.PropertyName, BeanMetaData.TimestampPropertyName, true) == 0) {
                    Timestamp = DateTime.Now;
                    SetupTimestampVariableList(varList, pt);
                } else if (String.Compare(pt.PropertyName, BeanMetaData.VersionNoPropertyName, true) == 0) {
                    if (!IsVersionNoAutoIncrementOnMemory()) {
                        continue;// because of always 'VERSION_NO = VERSION_NO + 1'
                    }
                    Object value = pt.PropertyInfo.GetValue(bean, null);
                    if (value == null) {
                        continue;// because of 'VERSION_NO = VERSION_NO + 1'
                    }
                    SetupVersionNoValiableList(varList, pt, bean);
                } else {
                    varList.Add(pt.PropertyInfo.GetValue(bean, null));
                }
                varTypeList.Add(pt.PropertyInfo.PropertyType);
            }
            AddAutoUpdateWhereBindVariables(varList, varTypeList, bean);
            BindVariables = varList.ToArray();
            BindVariableTypes = (Type[]) varTypeList.ToArray(typeof(Type));
        }

        protected bool IsVersionNoAutoIncrementOnMemory() {
            return _versionNoAutoIncrementOnMemory;
        }

        protected void SetupDeleteBindVariables(object bean) {
            ArrayList varList = new ArrayList();
            ArrayList varTypeList = new ArrayList();
            AddAutoUpdateWhereBindVariables(varList, varTypeList, bean);
            BindVariables = varList.ToArray();
            BindVariableTypes = (Type[]) varTypeList.ToArray(typeof(Type));
        }

        protected void AddAutoUpdateWhereBindVariables(ArrayList varList, ArrayList varTypeList, Object bean) {
            IBeanMetaData bmd = BeanMetaData;
            for (int i = 0; i < bmd.PrimaryKeySize; ++i) {
                IPropertyType pt = bmd.GetPropertyTypeByColumnName(bmd.GetPrimaryKey(i));
                PropertyInfo pi = pt.PropertyInfo;
                varList.Add(pi.GetValue(bean, null));
                varTypeList.Add(pi.PropertyType);
            }
            if (bmd.HasVersionNoPropertyType) {
                IPropertyType pt = bmd.VersionNoPropertyType;
                PropertyInfo pi = pt.PropertyInfo;
                varList.Add(pi.GetValue(bean, null));
                varTypeList.Add(pi.PropertyType);
            }
            if (bmd.HasTimestampPropertyType) {
                IPropertyType pt = bmd.TimestampPropertyType;
                PropertyInfo pi = pt.PropertyInfo;
                varList.Add(pi.GetValue(bean, null));
                varTypeList.Add(pi.PropertyType);
            }
        }

        protected void UpdateTimestampIfNeed(Object bean) {
            if (Timestamp != DateTime.MinValue) {
                PropertyInfo pi = BeanMetaData.TimestampPropertyType.PropertyInfo;
                SetupTimestampPropertyInfo(pi, bean);
            }
        }

        protected void UpdateVersionNoIfNeed(object bean) {
            if (VersionNo != Int32.MinValue) {
                PropertyInfo pi = BeanMetaData.VersionNoPropertyType.PropertyInfo;
                SetupVersionNoPropertyInfo(pi, bean);
            }
        }

        protected void SetupTimestampVariableList(IList varList, IPropertyType pt) {
            if (pt.PropertyType == typeof(DateTime)) {
                varList.Add(Timestamp);
            } else if (pt.PropertyType == typeof(DateTime?)) {
                varList.Add(Timestamp);
            } else if (pt.PropertyType == typeof(SqlDateTime)) {
                varList.Add(new SqlDateTime(Timestamp));
            } else {
                throw new WrongPropertyTypeOfTimestampException(pt.PropertyName, pt.PropertyType.Name);
            }
        }

        protected void SetupTimestampPropertyInfo(PropertyInfo pi, object bean) {
            if (pi.PropertyType == typeof(DateTime)) {
                pi.SetValue(bean, Timestamp, null);
            } else if (pi.PropertyType == typeof(DateTime?)) {
                pi.SetValue(bean, new DateTime?(Timestamp), null);
            } else if (pi.PropertyType == typeof(SqlDateTime)) {
                pi.SetValue(bean, new SqlDateTime(Timestamp), null);
            } else {
                throw new WrongPropertyTypeOfTimestampException(pi.Name, pi.PropertyType.Name);
            }
        }

        protected void SetupVersionNoValiableList(IList varList, IPropertyType pt, object bean) {
            object value = pt.PropertyInfo.GetValue(bean, null);
            int intValue = Convert.ToInt32(value) + 1;
            VersionNo = intValue;
            varList.Add(ConversionUtil.ConvertTargetType(VersionNo, pt.PropertyInfo.PropertyType));
        }

        protected void SetupVersionNoPropertyInfo(PropertyInfo pi, object bean) {
            pi.SetValue(bean, ConversionUtil.ConvertTargetType(VersionNo, pi.PropertyType), null);
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public new IDataSource DataSource {// Attension! This is new method!
            get { return base.DataSource; }
            set {
                if (value is ConnectionHolderDataSource) {
                    base.DataSource = value;
                } else {
                    base.DataSource = new ConnectionHolderDataSource(value);
                }
            }
        }

        public IBeanMetaData BeanMetaData {
            get { return _beanMetaData; }
        }

        protected object[] BindVariables {
            get { return _bindVariables; }
            set { _bindVariables = value; }
        }

        protected Type[] BindVariableTypes {
            get { return _bindVariableTypes; }
            set { _bindVariableTypes = value; }
        }

        protected DateTime Timestamp {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        protected int VersionNo {
            get { return _versionNo; }
            set { _versionNo = value; }
        }

        protected IPropertyType[] PropertyTypes {
            get { return _propertyTypes; }
            set { _propertyTypes = value; }
        }

        public bool VersionNoAutoIncrementOnMemory {
            set { _versionNoAutoIncrementOnMemory = value; }
        }
    }
}
