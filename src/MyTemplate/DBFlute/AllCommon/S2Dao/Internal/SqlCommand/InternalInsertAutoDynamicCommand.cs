
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Text;
using Seasar.Framework.Exceptions;
using Seasar.Extension.ADO;
using Seasar.Dao;
using Seasar.Dao.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlCommand {

    public class InternalInsertAutoDynamicCommand : AbstractSqlCommand {
		
        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        private const int NO_UPDATE = 0;
        private readonly IBeanMetaData _beanMetaData;
        private readonly string[] _propertyNames;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalInsertAutoDynamicCommand(IDataSource dataSource, ICommandFactory commandFactory,
            IBeanMetaData beanMetaData, string[] propertyNames)
            : base(dataSource, commandFactory) {
            _beanMetaData = beanMetaData;
            _propertyNames = propertyNames;
        }

        // ===============================================================================
        //                                                                         Execute
        //                                                                         =======
        public override Object Execute(object[] args) {
            object bean = args[0];
            IBeanMetaData bmd = BeanMetaData;
            string[] propertyNames = PropertyNames;
            IPropertyType[] propertyTypes = CreateTargetPropertyTypes(bmd, bean, propertyNames);
            if (CanExecute(bean, bmd, propertyTypes, propertyNames) == false) {
                return NO_UPDATE;
            }
            InternalAbstractAutoHandler handler = CreateAutoHandler(DataSource, CommandFactory, bmd, propertyTypes);
            handler.Sql = SetupSql(bmd, propertyTypes);
            handler.LoggingMessageSqlArgs = args; // Actually set up this property in the handler again.
            int i = handler.Execute(args);
            if (i < 1) {
                throw new NotSingleRowUpdatedRuntimeException(args[0], i);
            }
            return i;
        }

        protected virtual IPropertyType[] CreateTargetPropertyTypes(IBeanMetaData bmd, object bean, string[] propertyNames) {
            IList types = new ArrayList();
            string timestampPropertyName = bmd.TimestampPropertyName;
            string versionNoPropertyName = bmd.VersionNoPropertyName;
            for (int i = 0; i < propertyNames.Length; ++i)
            {
                IPropertyType pt = bmd.GetPropertyType(propertyNames[i]);
                if (IsTargetProperty(pt, timestampPropertyName, versionNoPropertyName, bean)) {
                    types.Add(pt);
                }
            }

            IPropertyType[] propertyTypes = new IPropertyType[types.Count];
            types.CopyTo(propertyTypes, 0);
            return propertyTypes;
        }
		
        protected virtual InternalAbstractAutoHandler CreateAutoHandler(IDataSource dataSource, ICommandFactory commandFactory, 
            IBeanMetaData beanMetaData, IPropertyType[] propertyTypes) {
            return new InternalInsertAutoHandler(dataSource, commandFactory, beanMetaData, propertyTypes);
        }

        protected virtual string SetupSql(IBeanMetaData bmd, IPropertyType[] propertyTypes) {
            StringBuilder buf = new StringBuilder(100);
            buf.Append("INSERT INTO ");
            buf.Append(bmd.TableName);
            buf.Append(" (");
            for (int i = 0; i < propertyTypes.Length; ++i) {
                IPropertyType pt = propertyTypes[i];
                String columnName = pt.ColumnName;
                if (i > 0)
                {
                    buf.Append(", ");
                }
                buf.Append(columnName);
            }
            buf.Append(") VALUES (");
            for (int i = 0; i < propertyTypes.Length; ++i) {
                if (i > 0) {
                    buf.Append(", ");
                }
                buf.Append("?");
            }
            buf.Append(")");
            return buf.ToString();
        }

        protected virtual bool IsTargetProperty(IPropertyType pt, string timestampPropertyName, string versionNoPropertyName, object bean) {
            IIdentifierGenerator identifierGenerator = BeanMetaData.IdentifierGenerator;
            if (pt.IsPrimaryKey) {
                return identifierGenerator.IsSelfGenerate;
            }
            string propertyName = pt.PropertyName;
            if (propertyName.Equals(timestampPropertyName, StringComparison.CurrentCultureIgnoreCase)
                        || propertyName.Equals(versionNoPropertyName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            object value = pt.PropertyInfo.GetValue(bean, null);

            //  for normal type include Nullable<T>
            if (value == null)
            {
                return false;
            }
            //  for Sytem.Data.SqlTypes.INullable
            if (value is INullable && ((INullable)value).IsNull)
            {
                return false;
            }
            return true;
        }

        protected virtual bool CanExecute(object bean, IBeanMetaData bmd, IPropertyType[] propertyTypes, string[] propertyNames)
        {
            if (propertyTypes.Length == 0) {
                throw new SRuntimeException("EDAO0014");
            }
            return true;
        }


        public IBeanMetaData BeanMetaData
        {
            get { return _beanMetaData; }
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }
	}
}
