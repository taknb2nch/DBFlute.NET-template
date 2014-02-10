
using System;
using System.Collections.Generic;

using Seasar.Quill.Attrs;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado {

    [Implementation(typeof(SqlLogRegistryLatestSqlProvider))]
    public interface LatestSqlProvider {
        String GetDisplaySql();
        IList<String> ExtractDisplaySqlList();
	    void ClearSqlCache();
    }
}
