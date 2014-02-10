
using System;
using System.Collections;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    public interface DaoWritable : DaoReadable {

        int Create(Entity entity);
        int Modify(Entity entity);
        int Remove(Entity entity);
    }
}
