
using System;
using System.Collections;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    public interface EntityDefinedCommonColumn : Entity {

        DateTime? CreatedDatetime { get; set; }

        String CreatedUser { get; set; }

        DateTime? UpdatedDatetime { get; set; }

        String UpdatedUser { get; set; }

        void EnableCommonColumnAutoSetup(); // for after disable because the default is enabled
        void DisableCommonColumnAutoSetup();
        bool CanCommonColumnAutoSetup();
    }
}
