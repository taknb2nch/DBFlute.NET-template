
using System;
using System.Collections.Generic;

using Seasar.Quill.Attrs;
using Seasar.Dao.Attrs;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao;
using Aaa.Bbb.Ccc.DBFlute.ExEntity;
using Aaa.Bbb.Ccc.DBFlute.CBean;

namespace Aaa.Bbb.Ccc.DBFlute.ExDao {

    [Implementation]
    [S2Dao(typeof(S2DaoSetting))]
    [Bean(typeof(MyTable))]
    public partial interface MyTableDao : DaoWritable {
		void InitializeDaoMetaData(String methodName);// Very Internal Method!

        int SelectCount(MyTableCB cb);
        IList<MyTable> SelectList(MyTableCB cb);

        int Insert(MyTable entity);
        int UpdateModifiedOnly(MyTable entity);
        int UpdateNonstrictModifiedOnly(MyTable entity);
        int UpdateByQuery(MyTableCB cb, MyTable entity);
        int Delete(MyTable entity);
        int DeleteNonstrict(MyTable entity);
        int DeleteByQuery(MyTableCB cb);// {DBFlute-0.7.9}

        int? SelectNextVal();
    }
}
