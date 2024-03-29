
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

using Seasar.Framework.Util;
using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Types;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlHandler {

    public class InternalProcedureHandler : InternalBasicSelectHandler {
        // = = = = = = = = = = = = = = = = = = = = = = = =
        // [Attension]
        // The return is not out-parameter at ADO.NET!
        // though JDBC treats it as out-parameter.
        // = = = = = = = = = =/

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected readonly InternalProcedureMetaData _procedureMetaData;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalProcedureHandler(IDataSource dataSource, String sql,
                IDataReaderHandler dataReaderHandler, ICommandFactory commandFactory,
                IDataReaderFactory dataReaderFactory, InternalProcedureMetaData procedureMetaData)
                : base(dataSource, sql, dataReaderHandler, commandFactory, dataReaderFactory) {
            this._procedureMetaData = procedureMetaData;
        }

        // ===============================================================================
        //                                                                         Execute
        //                                                                         =======
        protected override Object Execute(IDbConnection conn, Object[] args, Type[] argTypes) {
            Object dto = GetArgumentDto(args);
            LogSql(args, argTypes);
            IDbCommand dbCommand = null;
            try {
                try {
                    dbCommand = PrepareCallableStatement(conn, this.Sql);
                } catch (Exception e) {
                    HandleDbException(e, dbCommand, false);
                }
                Object returnValue = null;
                if (_procedureMetaData.HasReturnParameterType) {
                    Type returnType = _procedureMetaData.ReturnParameterType;
                    String returnParamName = BindReturnValues(dbCommand, "RetValue", GetDbValueType(returnType));
                    try {
                        BindParamters(dbCommand, dto);
                    } catch (Exception e) {
                        HandleDbException(e, dbCommand, false);
                    }
                    ExecuteNonQuery(dbCommand);
                    IDbDataParameter param = (IDbDataParameter)dbCommand.Parameters[returnParamName];
                    returnValue = param.Value;
                } else {
                    try {
                        BindParamters(dbCommand, dto);
                    } catch (Exception e) {
                        HandleDbException(e, dbCommand, false);
                    }
                    ExecuteNonQuery(dbCommand);
                }
                try {
                    return HandleOutParameters(dbCommand, dto, returnValue);
                } catch (Exception e) {
                    HandleDbException(e, dbCommand, false);
                    return null; // Unreachable!
                }
            } finally {
                try {
                    Close(dbCommand);
                } finally {
                    Close(conn);
                }
            }
        }

        protected Object GetArgumentDto(Object[] args) {
            if (args.Length == 0) {
                return null;
            }
            if (args.Length == 1) {
                if (args[0] == null) {
                    throw new IllegalArgumentException("args[0] should not be null!");
                }
                return args[0];
            }
            throw new IllegalArgumentException("args");
        }

        protected override String GetCompleteSql(Object[] args) { // for Procedure Call
            String sql = this.Sql;
            Object dto = GetArgumentDto(args);
            StringBuilder sb = new StringBuilder(100);
            int size = _procedureMetaData.ParameterTypeSize;
            if (size == 0 || dto == null) {
                return "call " + sql + "()"; // Because the procedure name is SQL at CSharp!
            }
            StringBuilder tmpSb = new StringBuilder();
            for (int i = 0; i < size; i++) {
                InternalProcedureParameterType ppt = _procedureMetaData.GetParameterType(i);
                if (ppt.IsReturnType) {
                    continue;
                }
                tmpSb.append(", ?");
            }
            if (tmpSb.length() > 0) {
                tmpSb.delete(0, ", ".Length);
            }
            sql = "call " + sql + "(" + tmpSb.toString() + ")"; // Because the procedure name is SQL at CSharp!
            if (_procedureMetaData.HasReturnParameterType) {
                sql = "? = " + sql;
            }
            int pos = 0;
            int pos2 = 0;
            for (int i = 0; i < size; i++) {
                InternalProcedureParameterType ppt = _procedureMetaData.GetParameterType(i);
                pos2 = sql.IndexOf('?', pos);
                if (pos2 < 0) {
                    break;
                }
                sb.append(sql.Substring(pos, (pos2 - pos)));
                pos = pos2 + 1;
                if (ppt.IsInType) {
                    sb.append(GetBindVariableText(ppt.GetValue(dto)));
                } else {
                    sb.append(sql.Substring(pos2, 1));
                }
            }
            sb.append(sql.Substring(pos));
            return sb.toString();
        }

        protected IDbCommand PrepareCallableStatement(IDbConnection conn, String sql) {
            if (this.Sql == null) { throw new IllegalStateException("The SQL should not be null!"); }
            IDbCommand dbCommand = CommandFactory.CreateCommand(conn, sql);
            dbCommand.CommandType = CommandType.StoredProcedure;
            return dbCommand;
        }

        protected String BindReturnValues(IDbCommand command, string parameterName, DbType dbType) {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Direction = ParameterDirection.ReturnValue;
            parameter.DbType = dbType;
            parameter.Size = 4096;
            if ("OleDbCommand".Equals(command.GetType().Name) && dbType == DbType.String) {
                OleDbParameter oleDbParam = parameter as OleDbParameter;
                oleDbParam.OleDbType = OleDbType.VarChar;
            } else if ("SqlDbCommand".Equals(command.GetType().Name) && dbType == DbType.String) {
                SqlParameter sqlDbParam = parameter as SqlParameter;
                sqlDbParam.SqlDbType = SqlDbType.VarChar;
            }
            command.Parameters.Add(parameter);
            return parameter.ParameterName;
        }

        protected void BindParamters(IDbCommand command, Object dto) {
            int size = _procedureMetaData.ParameterTypeSize;
            for (int i = 0; i < size; i++) {
                InternalProcedureParameterType ppt = _procedureMetaData.GetParameterType(i);
                if (ppt.IsReturnType) {
                    continue;
                }
                String parameterName = ppt.ParameterName;
                InternalBindVariableType vt = GetBindVariableType(command);
                switch (vt) {
                    case InternalBindVariableType.QuestionWithParam:
                        parameterName = "?" + parameterName;
                        break;
                    case InternalBindVariableType.ColonWithParam:
                        if ("OracleCommand".Equals(command.GetType().Name)) {
                            parameterName = string.Empty + parameterName;
                        } else {
                            parameterName = ":" + parameterName;
                        }
                        break;
                    default:
                        parameterName = "@" + parameterName;
                        break;
                }

                DbType dbType = GetDbValueType(ppt.ParameterPropertyType);
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Direction = ppt.ParameterDirectionType;
                parameter.Value = ppt.GetValue(dto);
                parameter.DbType = dbType;

                // If this setting is valid on MySQL, the exception occured.
                if (!"MySqlCommand".Equals(command.GetType().Name)) {
                    parameter.Size = 4096;
                }

                if ("OleDbCommand".Equals(command.GetType().Name) && dbType == DbType.String) {
                    OleDbParameter oleDbParam = parameter as OleDbParameter;
                    oleDbParam.OleDbType = OleDbType.VarChar;
                } else if ("SqlCommand".Equals(command.GetType().Name) && dbType == DbType.String) {
                    SqlParameter sqlDbParam = parameter as SqlParameter;
                    sqlDbParam.SqlDbType = SqlDbType.VarChar;
                }
                command.Parameters.Add(parameter);
            }
        }

        protected InternalBindVariableType GetBindVariableType(IDbCommand cmd) {
            String name = cmd.GetType().Name;
            if ("SqlCommand".Equals(name) || "DB2Command".Equals(name)) {
                return InternalBindVariableType.AtmarkWithParam;
            } else if ("OracleCommand".Equals(name)) {
                return InternalBindVariableType.ColonWithParam;
            } else if ("MySqlCommand".Equals(name)) {
                return InternalBindVariableType.QuestionWithParam;
            } else if ("NpgsqlCommand".Equals(name)) {
                return InternalBindVariableType.ColonWithParam;
            } else if ("FbCommand".Equals(name)) {
                return InternalBindVariableType.Question;
            } else {
                return InternalBindVariableType.Question;
            }
        }

        public enum InternalBindVariableType {
            None,
            AtmarkWithParam,
            Question,
            QuestionWithParam,
            ColonWithParam
        }

        protected DbType GetDbValueType(Type type) {
            if (type.IsGenericType && Regex.IsMatch(type.FullName, "^System.Nullable")) {
                type = type.GetGenericArguments()[0];
            }
            if (type == typeof(Byte) || type.FullName == "System.Byte&")
                return DbType.Byte;
            if (type == typeof(SByte) || type.FullName == "System.SByte&")
                return DbType.SByte;
            if (type == typeof(Int16) || type.FullName == "System.Int16&")
                return DbType.Int16;
            if (type == typeof(Int32) || type.FullName == "System.Int32&")
                return DbType.Int32;
            if (type == typeof(Int64) || type.FullName == "System.Int64&")
                return DbType.Int64;
            if (type == typeof(Single) || type.FullName == "System.Single&")
                return DbType.Single;
            if (type == typeof(Double) || type.FullName == "System.Double&")
                return DbType.Double;
            if (type == typeof(Decimal) || type.FullName == "System.Decimal&")
                return DbType.Decimal;
            if (type == typeof(DateTime) || type.FullName == "System.DateTime&")
                return DbType.DateTime;
            if (type == ValueTypes.BYTE_ARRAY_TYPE)
                return DbType.Binary;
            if (type == typeof(String) || type.FullName == "System.String&")
                return DbType.String;
            if (type == typeof(Boolean) || type.FullName == "System.Boolean&")
                return DbType.Boolean;
            if (type == typeof(Guid) || type.FullName == "System.Guid&")
                return DbType.Guid;
            else
                return DbType.Object;
        }

        protected Object HandleOutParameters(IDbCommand dbCommand, Object dto, Object returnValue) {
            if (dto == null) {
                return null;
            }
            int size = _procedureMetaData.ParameterTypeSize;
            for (int i = 0; i < size; i++) {
                InternalProcedureParameterType ppt = _procedureMetaData.GetParameterType(i);
                if (ppt.IsOutType) {
                    Object value = ((IDataParameter) dbCommand.Parameters[i]).Value;
                    if (value is IDataReader) {// Not support yet
                        IDataReader reader = (IDataReader) value;
                        throw new NotImplementedException("The result set of procedure is not implemented: " + reader);
                    //     IDataReaderHandler handler = CreateOutParameterResultSetHandler(ppt, reader);
                    //     try {
                    //         value = handler.Handle(reader);
                    //     } finally {
                    //         if (reader != null) {
                    //             reader.Close();
                    //         }
                    //     }
                    }
                    if (!(value is DBNull)) {
                        ppt.SetValue(dto, value);
                    }
                    // [Under Review]: @jflute If it's DBNull, what can I do for you?
                } else if (ppt.IsReturnType) {
                    ppt.SetValue(dto, returnValue);
                }
            }
            return dto;
        }

        protected IDataReaderHandler CreateOutParameterResultSetHandler(InternalProcedureParameterType ppt, IDataReader reader) {
            // return new InternalMapListResultSetHandler();
            return null;
        }
    }
}
