
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm.Info {
    
    public interface RelationInfo {

        String RelationPropertyName { get; }
        DBMeta LocalDBMeta { get; }
        DBMeta TargetDBMeta { get; }
        Map<ColumnInfo,ColumnInfo> LocalTargetColumnInfoMap { get; }
        bool IsOneToOne { get; }
        bool IsReferrer { get; }
    }
}
