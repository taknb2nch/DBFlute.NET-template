
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Text;
using Seasar.Extension.ADO;
using Seasar.Dao;
using Seasar.Dao.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlCommand {

    public class InternalUpdateAutoDynamicCommand : AbstractSqlCommand {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
	    protected const int NO_UPDATE = 0;
	
        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected readonly IBeanMetaData _beanMetaData;
        protected readonly String[] _propertyNames;
        protected bool checkSingleRowUpdate = true;
        protected bool _versionNoAutoIncrementOnMemory = true;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalUpdateAutoDynamicCommand(IDataSource dataSource, ICommandFactory commandFactory,
            IBeanMetaData beanMetaData, string[] propertyNames)
            : base(dataSource, commandFactory) {
            _beanMetaData = beanMetaData;
            _propertyNames = propertyNames;
        }

        public override object Execute(object[] args) {
            Object bean = args[0];
            IBeanMetaData bmd = BeanMetaData;
            String[] propertyNames = PropertyNames;
            IPropertyType[] propertyTypes = CreateTargetPropertyTypes(bmd, bean, propertyNames);
            if (CanExecute(bean, bmd, propertyTypes, propertyNames) == false) {
                return NO_UPDATE;
            }
            InternalAbstractAutoHandler handler = CreateInternalAutoHandler(DataSource, CommandFactory, bmd, propertyTypes);
            handler.Sql = CreateUpdateSql(bmd, propertyTypes, bean);
            int i = handler.Execute(args);

            // [Comment Out]: This statement moved to the handler at DBFlute-0.8.0.
            // if (IsCheckSingleRowUpdate && i < 1) {
            //     throw new NotSingleRowUpdatedRuntimeException(args[0], i);
            // }

            return i;
        }
		
        protected virtual IPropertyType[] CreateTargetPropertyTypes(IBeanMetaData bmd, object bean, string[] propertyNames) {
            IList types = new ArrayList();
            String timestampPropertyName = bmd.TimestampPropertyName;
            String versionNoPropertyName = bmd.VersionNoPropertyName;
            for (int i = 0; i < propertyNames.Length; ++i) {
                IPropertyType pt = bmd.GetPropertyType(propertyNames[i]);
                if (IsTargetProperty(pt, timestampPropertyName, versionNoPropertyName, bean)) {
                    types.Add(pt);
                }
            }
            IPropertyType[] propertyTypes = new IPropertyType[types.Count];
            types.CopyTo(propertyTypes, 0);
            return propertyTypes;
        }
		
        protected virtual InternalAbstractAutoHandler CreateInternalAutoHandler(IDataSource dataSource, ICommandFactory commandFactory, 
            IBeanMetaData beanMetaData, IPropertyType[] propertyTypes) {
            InternalUpdateAutoHandler handler = new InternalUpdateAutoHandler(dataSource, commandFactory, beanMetaData, propertyTypes);
            handler.VersionNoAutoIncrementOnMemory = _versionNoAutoIncrementOnMemory;
            handler.IsCheckSingleRowUpdate = IsCheckSingleRowUpdate; // [DBFlute-0.8.0]
            return handler;
        }

        protected virtual String CreateUpdateSql(IBeanMetaData bmd, IPropertyType[] propertyTypes, Object bean) {
            if (bmd.PrimaryKeySize == 0) {
                String msg = "The table '" + bmd.TableName + "' does not have primary keys!";
                throw new SystemException(msg);
            }
            StringBuilder sb = new StringBuilder(100);
            sb.Append("UPDATE ").Append(bmd.TableName).Append(" SET ");
            String versionNoPropertyName = bmd.VersionNoPropertyName;
            for (int i = 0; i < propertyTypes.Length; ++i) {
                IPropertyType pt = propertyTypes[i];
                String columnName = pt.ColumnName;
                if (i > 0) {
                    sb.Append(", ");
                }
                if (String.Compare(pt.PropertyName, versionNoPropertyName, true) == 0) {
                    if (!IsVersionNoAutoIncrementOnMemory()) {
                        SetupVersionNoAutoIncrementOnQuery(sb, columnName);
                        continue;// because of always 'VERSION_NO = VERSION_NO + 1'
                    }
                    Object value = pt.PropertyInfo.GetValue(bean, null);
                    if (value == null) {
                        SetupVersionNoAutoIncrementOnQuery(sb, columnName);
                        continue;
                    }
                }
                sb.Append(columnName).Append(" = ?");
            }
            sb.Append(" WHERE ");
            const string ADD_AND = " AND ";
            for (int i = 0; i < bmd.PrimaryKeySize; ++i) {
                sb.Append(bmd.GetPrimaryKey(i)).Append(" = ?").Append(ADD_AND);
            }
            sb.Length = sb.Length - ADD_AND.Length;
            if (bmd.HasVersionNoPropertyType) {
                IPropertyType pt = bmd.VersionNoPropertyType;
                sb.Append(ADD_AND).Append(pt.ColumnName).Append(" = ?");
            }
            if (bmd.HasTimestampPropertyType) {
                IPropertyType pt = bmd.TimestampPropertyType;
                sb.Append(ADD_AND).Append(pt.ColumnName).Append(" = ?");
            }
            return sb.ToString();
        }

        protected bool IsVersionNoAutoIncrementOnMemory() {
            return _versionNoAutoIncrementOnMemory;
        }

        protected void SetupVersionNoAutoIncrementOnQuery(StringBuilder sb, String columnName) {
            sb.Append(columnName).Append(" = ").Append(columnName).Append(" + 1");
        }

        protected virtual bool IsTargetProperty(IPropertyType pt, string timestampPropertyName, string versionNoPropertyName, object bean) {
            if (pt.IsPrimaryKey) {
                return false;
            }
            string propertyName = pt.PropertyName;
            if (propertyName.Equals(timestampPropertyName, StringComparison.CurrentCultureIgnoreCase)
                        || propertyName.Equals(versionNoPropertyName, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            object value = pt.PropertyInfo.GetValue(bean, null);

            //  for normal type include Nullable<T>
            if (value == null) {
                return false;
            }
            //  for System.Data.SqlTypes.INullable
            if (value is INullable && ((INullable)value).IsNull) {
                return false;
            }
			return true;
        }

        protected virtual bool CanExecute(object bean, IBeanMetaData bmd, IPropertyType[] propertyTypes, string[] propertyNames) {
            if ( propertyTypes.Length == 0 ) {
                throw new NoUpdatePropertyTypeRuntimeException();
            }
            return true;
        }
		
        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IBeanMetaData BeanMetaData {
            get { return _beanMetaData; }
        }

        public string[] PropertyNames {
            get { return _propertyNames; }
        }

        public bool VersionNoAutoIncrementOnMemory {
            set { _versionNoAutoIncrementOnMemory = value; }
        }

        public bool IsCheckSingleRowUpdate {
            get { return checkSingleRowUpdate; }
            set { checkSingleRowUpdate = value; }
        }
	}
}
