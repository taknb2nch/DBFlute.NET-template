
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    public interface PagingHandler<ENTITY> {

        PagingBean PagingBean { get; }
        int Count();
        IList<ENTITY> Paging();
    }
}
