
using System;
using System.Data;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;
using Seasar.Framework.Log;
using Seasar.Framework.Util;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler {

    public class InternalBasicUpdateHandler : InternalBasicHandler {

        // ===============================================================================
        //                                                                      Definition
        //                                                                      ==========
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalBasicUpdateHandler(IDataSource dataSource, string sql, ICommandFactory commandFactory)
            : base(dataSource, sql, commandFactory) {
        }
		
        // ===============================================================================
        //                                                                         Execute
        //                                                                         =======
        public virtual int Execute(object[] args) {
            return Execute(args, GetArgTypes(args));
        }

        public virtual int Execute(object[] args, Type[] argTypes) {
            LogSql(args, argTypes);
            IDbConnection conn = Connection;
            try {
                return Execute(conn, args, argTypes);
            } finally {
                Close(conn);
            }
        }

        public virtual int Execute(object[] args, Type[] argTypes, string[] argNames) {
            return Execute(args, argTypes);
        }

        protected virtual int Execute(IDbConnection connection, object[] args, Type[] argTypes) {
            IDbCommand cmd = Command(connection);
            try {
                BindArgs(cmd, args, argTypes);
                return ExecuteUpdate(cmd);
            } finally {
                Close(cmd);
            }
        }
    }
}
