
using System;

using Seasar.Quill.Attrs;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    [Implementation(typeof(CacheBehaviorSelector))]
    public interface BehaviorSelector {
        void InitializeConditionBeanMetaData();
        BEHAVIOR Select<BEHAVIOR>() where BEHAVIOR : BehaviorReadable;
        BehaviorReadable ByName(String tableFlexibleName);
    }
}
