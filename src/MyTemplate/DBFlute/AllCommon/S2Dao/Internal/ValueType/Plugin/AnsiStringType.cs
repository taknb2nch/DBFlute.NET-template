
using System;
using System.Data;

using Seasar.Extension.ADO.Types;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.ValueType.Plugin {

    public class AnsiStringType : StringType {

        public override void BindValue(IDbCommand cmd, String columnName, Object value) {
            BindValue(cmd, columnName, value, DbType.AnsiString);
        }
    }
}
