
using System;
using System.Collections.Generic;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv.Setup {

    public delegate void EntityListSetupper<ENTITY>(IList<ENTITY> entityList) where ENTITY : Entity;
}
