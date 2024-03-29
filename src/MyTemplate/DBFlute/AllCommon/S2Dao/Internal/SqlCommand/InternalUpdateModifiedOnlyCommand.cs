
using System;
using System.Collections;
using System.Text;
using Seasar.Extension.ADO;
using Seasar.Framework.Log;
using Seasar.Dao;
using Seasar.Dao.Impl;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlCommand {

    public class InternalUpdateModifiedOnlyCommand : InternalUpdateAutoDynamicCommand {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalUpdateModifiedOnlyCommand(IDataSource dataSource, ICommandFactory commandFactory,
            IBeanMetaData beanMetaData, string[] propertyNames)
            : base(dataSource, commandFactory, beanMetaData, propertyNames) {
        }

        protected override IPropertyType[] CreateTargetPropertyTypes(IBeanMetaData bmd, object bean, string[] propertyNames) {
            IDictionary modifiedPropertyNames = bmd.GetModifiedPropertyNames(bean);
            IList types = new ArrayList();
            string timestampPropertyName = bmd.TimestampPropertyName;
            string versionNoPropertyName = bmd.VersionNoPropertyName;
            for (int i = 0; i < propertyNames.Length; ++i) {
                IPropertyType pt = bmd.GetPropertyType(propertyNames[i]);
                if (pt.IsPrimaryKey == false) {
                    string propertyName = pt.PropertyName;
                    if (propertyName.Equals(timestampPropertyName, StringComparison.CurrentCultureIgnoreCase)
                            || propertyName.Equals(versionNoPropertyName, StringComparison.CurrentCultureIgnoreCase)
                            || modifiedPropertyNames.Contains(propertyName)) {
                        types.Add(pt);
                    }
                }
            }
            IPropertyType[] propertyTypes = new IPropertyType[types.Count];
            types.CopyTo(propertyTypes, 0);
            return propertyTypes;
        }

        protected override bool CanExecute(object bean, IBeanMetaData bmd, IPropertyType[] propertyTypes, String[] propertyNames) {
            if (propertyTypes.Length > 0) {
                return true;
            }
            if (_log.IsDebugEnabled) {
                string s = CreateNoUpdateLogMessage(bean, bmd);
                _log.Debug(s);
            }
            return false;
        }

        protected virtual string CreateNoUpdateLogMessage(object bean, IBeanMetaData bmd) {
            StringBuilder builder = new StringBuilder();
            builder.Append("skip UPDATE: table=");
            builder.Append(bmd.TableName);
            int size = bmd.PrimaryKeySize;
            for ( int i = 0; i < size; i++ ) {
                if ( i == 0 ) {
                    builder.Append(", key{");
                } else {
                    builder.Append(", ");
                }
                string keyName = bmd.GetPrimaryKey(i);
                builder.Append(keyName);
                builder.Append("=");
                builder.Append(bmd.GetPropertyTypeByColumnName(keyName)
                        .PropertyInfo.GetValue(bean, null));
                if (i == size - 1) {
                    builder.Append("}");
                }
            }
            
            return builder.ToString();
        }
	}
}
