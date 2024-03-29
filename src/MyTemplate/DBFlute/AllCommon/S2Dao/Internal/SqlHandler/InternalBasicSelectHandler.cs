
using System;
using System.Data;
using Seasar.Framework.Log;
using Seasar.Framework.Util;
using Seasar.Framework.Exceptions;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler {

    public class InternalBasicSelectHandler : InternalBasicHandler {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected IDataReaderFactory _dataReaderFactory = BasicDataReaderFactory.INSTANCE;
        protected IDataReaderHandler _dataReaderHandler;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalBasicSelectHandler(IDataSource dataSource
			                                 , String sql
										     , IDataReaderHandler dataReaderHandler
											 , ICommandFactory commandFactory
											 , IDataReaderFactory dataReaderFactory)
			: base(dataSource, sql, commandFactory) {
            DataReaderHandler = dataReaderHandler;
            DataReaderFactory = dataReaderFactory;
        }

        // ===============================================================================
        //                                                                         Execute
        //                                                                         =======
        public virtual Object Execute(Object[] args) {
            return Execute(args, GetArgTypes(args));
        }

        public virtual Object Execute(Object[] args, Type[] argTypes) {
            IDbConnection conn = Connection;
            try {
                return Execute(conn, args, argTypes);
            } finally {
                Close(conn);
            }
        }

        public virtual Object Execute(Object[] args, Type[] argTypes, String[] argNames) {
            return Execute(args, argTypes);
        }

        protected virtual Object Execute(IDbConnection conn, object[] args, Type[] argTypes) {
            LogSql(args, argTypes);
            IDbCommand cmd = null;
            try {
                cmd = Command(conn);
                BindArgs(cmd, args, argTypes);
                return Execute(cmd, args);
            } finally {
                Close(cmd);
            }
        }

        protected virtual Object Execute(IDbCommand cmd, Object[] args) {
            if (_dataReaderHandler == null) {
                throw new EmptyRuntimeException("dataReaderHandler");
            }
            IDataReader dataReader = null;
            try {
                if (_dataReaderHandler is ObjectDataReaderHandler) {// CSharp specific
                    return ExecuteScalar(cmd);
                } else {
                    try {
                        dataReader = CreateDataReader(cmd);
                        return _dataReaderHandler.Handle(dataReader);
                    } catch (Exception e) {
                        HandleDbException(e, cmd);
                        return null; // Unreachable!
                    }
                }
            } finally {
                Close(dataReader);
            }
        }

        protected virtual IDataReader CreateDataReader(IDbCommand cmd) {
            return _dataReaderFactory.CreateDataReader(DataSource, cmd);
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IDataReaderFactory DataReaderFactory {
            get { return _dataReaderFactory; }
            set { _dataReaderFactory = value; }
        }

        public IDataReaderHandler DataReaderHandler {
            get { return _dataReaderHandler; }
            set { _dataReaderHandler = value; }
        }
    }
}
