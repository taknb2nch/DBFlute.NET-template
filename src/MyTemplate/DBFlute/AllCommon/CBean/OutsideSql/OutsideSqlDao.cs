
using System;
using System.Collections.Generic;

using Seasar.Quill.Attrs;
using Seasar.Dao.Attrs;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql {

    [Implementation]
    [S2Dao(typeof(S2DaoSetting))]
    [Bean(typeof(OutsideSqlDaoDummyEntity))]
    public interface OutsideSqlDao : DaoReadable {

        // ===================================================================================
        //                                                                              Select
        //                                                                              ======
        System.Collections.IList SelectList(String path, Object pmb, OutsideSqlOption option, Type entityType);

        Object SelectCursor(String path, Object pmb, OutsideSqlOption option, CursorHandler handler);

        int Execute(String path, Object pmb, OutsideSqlOption option);

		// Not implemented yet!
        // int[] batchExecute(String path, List<Object> pmb, OutsideSqlOption option);

        // [DBFlute-0.8.0]
        // ===================================================================================
        //                                                                                Call
        //                                                                                ====
        void Call(String path, Object pmb, OutsideSqlOption option);
    }

    public class OutsideSqlDaoDummyEntity {
    }
}
