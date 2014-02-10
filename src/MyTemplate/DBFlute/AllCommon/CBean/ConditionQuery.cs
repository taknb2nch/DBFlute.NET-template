
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

// JavaLike
public interface ConditionQuery {

    // ===================================================================================
    //                                                                          Table Name
    //                                                                          ==========
    String getTableDbName();
    String getTableSqlName();
	
    // ===================================================================================
    //                                                                  Important Accessor
    //                                                                  ==================
    ConditionQuery xgetReferrerQuery();
    SqlClause xgetSqlClause();
    String xgetAliasName();
    String toColumnRealName(String columnName);
    int xgetNestLevel();
    int xgetNextNestLevel();
    bool isBaseQuery();
	String xgetForeignPropertyName();
    String xgetRelationPath();
    String xgetLocationBase();
	
    // ===================================================================================
    //                                                                 Reflection Invoking
    //                                                                 ===================
    ConditionValue invokeValue(String columnFlexibleName);
    void invokeQuery(String columnFlexibleName, String conditionKeyName, Object value);
    void invokeOrderBy(String columnFlexibleName, bool isAsc);
    ConditionQuery invokeForeignCQ(String foreignPropertyName);
    bool invokeHasForeignCQ(String foreignPropertyName);
}

}
