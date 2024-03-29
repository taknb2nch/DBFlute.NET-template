
using System;
using System.Reflection;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CHelper;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm.Info;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

// JavaLike
[System.Serializable]
public abstract class AbstractConditionQuery : ConditionQuery {

    // ===================================================================================
    //                                                                          Definition
    //                                                                          ==========
    protected static readonly ConditionKey CK_EQ = ConditionKey.CK_EQUAL;
    protected static readonly ConditionKey CK_NES = ConditionKey.CK_NOT_EQUAL_STANDARD;
    protected static readonly ConditionKey CK_NET = ConditionKey.CK_NOT_EQUAL_TRADITION;
    protected static readonly ConditionKey CK_GE = ConditionKey.CK_GREATER_EQUAL;
    protected static readonly ConditionKey CK_GT = ConditionKey.CK_GREATER_THAN;
    protected static readonly ConditionKey CK_LE = ConditionKey.CK_LESS_EQUAL;
    protected static readonly ConditionKey CK_LT = ConditionKey.CK_LESS_THAN;
    protected static readonly ConditionKey CK_INS = ConditionKey.CK_IN_SCOPE;
    protected static readonly ConditionKey CK_NINS = ConditionKey.CK_NOT_IN_SCOPE;
    protected static readonly ConditionKey CK_LS = ConditionKey.CK_LIKE_SEARCH;
    protected static readonly ConditionKey CK_NLS = ConditionKey.CK_NOT_LIKE_SEARCH;
    protected static readonly ConditionKey CK_ISN = ConditionKey.CK_IS_NULL;
    protected static readonly ConditionKey CK_ISNN = ConditionKey.CK_IS_NOT_NULL;

    protected static readonly ConditionValue DUMMY_CONDITION_VALUE = new ConditionValue();
    protected static readonly Object DUMMY_OBJECT = new Object();
	protected static readonly String CQ_PROPERTY = "ConditionQuery";
	
    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    protected readonly SqlClause _sqlClause;
    protected readonly String _aliasName;
    protected readonly int _nestLevel;
	protected int _subQueryLevel;

    // -----------------------------------------------------
    //                                          Foreign Info
    //                                          ------------
    protected String _foreignPropertyName;
    protected String _relationPath;
    protected readonly ConditionQuery _referrerQuery;

    // -----------------------------------------------------
    //                                                Inline
    //                                                ------
	protected bool _onClause;

    // ===================================================================================
    //                                                                         Constructor
    //                                                                         ===========
    public AbstractConditionQuery(ConditionQuery childQuery, SqlClause sqlClause, String aliasName, int nestLevel) {
        _referrerQuery = childQuery;
        _sqlClause = sqlClause;
        _aliasName = aliasName;
        _nestLevel = nestLevel;
    }

    // ===================================================================================
    //                                                                     DBMeta Provider
    //                                                                     ===============
    protected DBMetaProvider xgetDBMetaProvider() {
        return DBMetaInstanceHandler.getProvider();
    }

    protected DBMeta findDBMeta(String tableFlexibleName) {
        return DBMetaInstanceHandler.FindDBMeta(tableFlexibleName);
    }

    // ===================================================================================
    //                                                                          Table Name
    //                                                                          ==========
    public abstract String getTableDbName();
    public abstract String getTableSqlName();

    // ===================================================================================
    //                                                                  Important Accessor
    //                                                                  ==================
    public ConditionQuery xgetReferrerQuery() {
        return _referrerQuery;
    }

    public SqlClause xgetSqlClause() {
        return _sqlClause;
    }

    public String xgetAliasName() {
        return _aliasName;
    }

    public int xgetNestLevel() {
        return _nestLevel;
    }

    public int xgetNextNestLevel() {
        return _nestLevel+1;
    }

    public bool isBaseQuery() {
        return (xgetReferrerQuery() == null);
    }

    // -----------------------------------------------------
    //                                             Real Name
    //                                             ---------
    public String toColumnRealName(String columnDbName) {
        assertColumnName(columnDbName);
        return buildRealColumnName(xgetAliasName(), toColumnSqlName(columnDbName));
    }

	protected String buildRealColumnName(String aliasName, String columnDbName) {
        return aliasName + "." + columnDbName;
    }

    public String toColumnSqlName(String columnDbName) {
        return findDBMeta(getTableDbName()).FindColumnInfo(columnDbName).ColumnSqlName;
    }

    // -----------------------------------------------------
    //                                          Foreign Info
    //                                          ------------
    public String xgetForeignPropertyName() {
        return _foreignPropertyName;
    }

    public void xsetForeignPropertyName(String foreignPropertyName) {
        this._foreignPropertyName = foreignPropertyName;
    }

    public String xgetRelationPath() {
        return _relationPath;
    }

    public void xsetRelationPath(String relationPath) {
        this._relationPath = relationPath;
    }

    // -----------------------------------------------------
    //                                                Inline
    //                                                ------
	public void xsetOnClause(bool onClause) {
	    _onClause = onClause;
	}

    // -----------------------------------------------------
    //                                              Location
    //                                              --------
    public String xgetLocationBase() {
        StringBuilder sb = new StringBuilder();
        ConditionQuery query = this;
        while (true) {
            if (query.isBaseQuery()) {
                sb.insert(0, CQ_PROPERTY + ".");
                break;
            } else {
                String foreignPropertyName = query.xgetForeignPropertyName();
                if (foreignPropertyName == null) {
                    String msg = "The foreignPropertyName of the query should not be null:";
                    msg = msg + " query=" + query;
                    throw new IllegalStateException(msg);
                }
                sb.insert(0, CQ_PROPERTY + initCap(foreignPropertyName) + ".");
            }
            query = query.xgetReferrerQuery();
        }
        return sb.toString();
    }

    protected String getLocation(String columnPropertyName, ConditionKey key) {
        return xgetLocationBase(columnPropertyName) + "." + key.getConditionKey();
    }

    protected String xgetLocationBase(String columnPropertyName) {
        return xgetLocationBase() + columnPropertyName;
    }

    // ===================================================================================
    //                                                                  Nested SetupSelect
    //                                                                  ==================
    public void doNss(NssCall callback) { // Very Internal
        String foreignPropertyName = callback.Invoke().xgetForeignPropertyName();
        String foreignTableAliasName = callback.Invoke().xgetAliasName();
        xgetSqlClause().registerSelectedSelectColumn(foreignTableAliasName, getTableDbName(), foreignPropertyName, xgetRelationPath());
        xgetSqlClause().registerSelectedForeignInfo(callback.Invoke().xgetRelationPath(), foreignPropertyName);
    }
    
    public delegate ConditionQuery NssCall(); // Very Internal

    // ===================================================================================
    //                                                                           OuterJoin
    //                                                                           =========
    protected virtual void registerOuterJoin(ConditionQuery foreignCQ, Map<String, String> joinOnResourceMap) {
        registerOuterJoin(foreignCQ, joinOnResourceMap, null);
    }

    protected virtual void registerOuterJoin(ConditionQuery foreignCQ, Map<String, String> joinOnResourceMap, String fixedCondition) {
        // translate join-on map using column real name
        Map<String, String> joinOnMap = new LinkedHashMap<String, String>();
        Set<Entry<String, String>> entrySet = joinOnResourceMap.entrySet();
        foreach (Entry<String, String> entry in entrySet) {
            String local = entry.getKey();
            String foreign = entry.getValue();
            joinOnMap.put(toColumnRealName(local), foreignCQ.toColumnRealName(foreign));
        }
        String localDbName = getTableDbName();
        String foreignDbName = foreignCQ.getTableDbName();
        String foreignAliasName = foreignCQ.xgetAliasName();
        FixedConditionResolver resolver = createFixedConditionResolver(foreignCQ, joinOnMap);
        xgetSqlClause().registerOuterJoin(localDbName, foreignDbName, foreignAliasName, joinOnMap, fixedCondition, resolver);
    }

    protected FixedConditionResolver createFixedConditionResolver(ConditionQuery foreignCQ,
            Map<String, String> joinOnMap) {
        return new HpFixedConditionQueryResolver(this, foreignCQ, xgetDBMetaProvider());
    }

    // ===================================================================================
    //                                                                         Union Query
    //                                                                         ===========
    protected Map<String, ConditionQuery> _unionQueryMap;

    public Map<String, ConditionQuery> getUnionQueryMap() {// for Internal
		if (_unionQueryMap == null) {
		    _unionQueryMap = new LinkedHashMap<String, ConditionQuery>();
		}
        return _unionQueryMap;
    }

	public Map<String, ConditionQuery> UnionQueryMap {// for SQL-Comment
        get { return getUnionQueryMap(); }
    }

    public void xsetUnionQuery(ConditionQuery unionQuery) {
        xsetupUnion(unionQuery, false, getUnionQueryMap());
    }

    protected Map<String, ConditionQuery> _unionAllQueryMap;

    public Map<String, ConditionQuery> getUnionAllQueryMap() {// for Internal
		if (_unionAllQueryMap == null) {
		    _unionAllQueryMap = new LinkedHashMap<String, ConditionQuery>();
		}
        return _unionAllQueryMap;
    }

	public Map<String, ConditionQuery> UnionAllQueryMap {// for SQL-Comment
        get { return getUnionAllQueryMap(); }
    }

    public void xsetUnionAllQuery(ConditionQuery unionAllQuery) {
        xsetupUnion(unionAllQuery, true, getUnionAllQueryMap());
    }

    protected void xsetupUnion(ConditionQuery unionQuery, bool unionAll, Map<String, ConditionQuery> unionQueryMap) {
        if (unionQuery == null) {
            String msg = "The argument[unionQuery] should not be null.";
            throw new IllegalArgumentException(msg);
        }
        reflectRelationOnUnionQuery(this, unionQuery); // Reflect Relation!
        String key = (unionAll ? "unionAllQuery" : "unionQuery") + unionQueryMap.size();
        unionQueryMap.put(key, unionQuery);
        registerUnionQuery(unionQuery, unionAll, (unionAll ? "UnionAllQueryMap" : "UnionQueryMap") + "." + key);// If CSharp, The property of 'Union' should be 'InitCap'.
    }
	
    abstract public void reflectRelationOnUnionQuery(ConditionQuery baseQueryAsSuper, ConditionQuery unionQueryAsSuper);

    public bool hasUnionQueryOrUnionAllQuery() {
        return (_unionQueryMap != null && !_unionQueryMap.isEmpty()) || (_unionAllQueryMap != null && !_unionAllQueryMap.isEmpty());
    }

    public List<ConditionQuery> getUnionQueryList() {
		if (_unionQueryMap == null) { return new ArrayList<ConditionQuery>(); }
        return new ArrayList<ConditionQuery>(_unionQueryMap.values());
    }

    public List<ConditionQuery> getUnionAllQueryList() {
		if (_unionAllQueryMap == null) { return new ArrayList<ConditionQuery>(); }
        return new ArrayList<ConditionQuery>(_unionAllQueryMap.values());
    }

    // ===================================================================================
    //                                                                            Register
    //                                                                            ========
    // -----------------------------------------------------
    //                                          Normal Query
    //                                          ------------
    protected virtual void regQ(ConditionKey key, Object value, ConditionValue cvalue, String colName) {
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        setupConditionValueAndRegisterWhereClause(key, value, cvalue, colName);
    }
	
	protected virtual void regQ(ConditionKey key, Object value, ConditionValue cvalue, String colName, ConditionOption option) {
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        setupConditionValueAndRegisterWhereClause(key, value, cvalue, colName, option);
    }

    protected virtual bool isValidQuery(ConditionKey key, Object value, ConditionValue cvalue, String colName) {
        String realColumnName = toColumnRealName(colName);
        if (key.isValidRegistration(cvalue, value, realColumnName)) {
            return true;
        } else {
            if (xgetSqlClause().isCheckInvalidQuery()) {
                throwInvalidQueryRegisteredException(key, cvalue, realColumnName);
                return false; // unreachable
            } else {
                xgetSqlClause().registerInvalidQueryColumn(realColumnName, key);
                return false;
            }
        }
    }

    protected void throwInvalidQueryRegisteredException(ConditionKey key, Object value, String realColumnName) {
        String msg = "Look! Read the message below." + ln();
        msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
        msg = msg + "An invalid query was registered. (check is working)" + ln();
        msg = msg + ln();
        msg = msg + "[Advice]" + ln();
        msg = msg + "You should not set an invalid query when you set the check valid."  + ln();
        msg = msg + "For example:"  + ln();
        msg = msg + "  (x):"  + ln();
        msg = msg + "    MemberCB cb = new MemberCB();"  + ln();
        msg = msg + "    cb.CheckInvalidQuery();"  + ln();
        msg = msg + "    cb.Query().SetMemberId_Equal(null); // exception"  + ln();
        msg = msg + "  (o):"  + ln();
        msg = msg + "    MemberCB cb = new MemberCB();"  + ln();
        msg = msg + "    cb.CheckInvalidQuery();"  + ln();
        msg = msg + "    cb.Query().SetMemberId_Equal(3);"  + ln();
        msg = msg + "  (o):"  + ln();
        msg = msg + "    MemberCB cb = new MemberCB();"  + ln();
        msg = msg + "    cb.Query().SetMemberId_Equal(null);"  + ln();
        msg = msg + ln();
        msg = msg + "[Column]" + ln();
        msg = msg + realColumnName + ln();
        msg = msg + ln();
        msg = msg + "[Condition Key]" + ln();
        msg = msg + key.getConditionKey() + ln();
        msg = msg + ln();
        msg = msg + "[Registered Value]" + ln();
        msg = msg + value + ln();
        msg = msg + "* * * * * * * * * */";
        throw new InvalidQueryRegisteredException(msg);
    }

    // -----------------------------------------------------
    //                                         InScope Query
    //                                         -------------
    protected void regINS<ELEMENT>(ConditionKey key, System.Collections.IList value, ConditionValue cvalue, String colName) {
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        int inScopeLimit = xgetSqlClause().getInScopeLimit();
        if (inScopeLimit > 0 && value.Count > inScopeLimit) {
            // if the key is for inScope, it should be split as 'or'
            // (if the key is for notInScope, it should be split as 'and')
            bool alreadyOrScopeQuery = xgetSqlClause().isOrScopeQueryEffective();
            if (isConditionKeyInScope(key)) {
                // if or-scope query has already been effective, create new or-scope
                xgetSqlClause().makeOrScopeQueryEffective();
            } else {
                if (alreadyOrScopeQuery) {
                    xgetSqlClause().beginOrScopeQueryAndPart();
                }
            }

            try {
                // split the condition
                List<Object> objectList = convertToJavaList(value);
                List<List<Object>> valueList = splitByLimit(objectList, inScopeLimit);
                for (int i = 0; i < valueList.size(); i++) {
                    System.Collections.IList currentValue = convertToEmbeddedList<ELEMENT>(valueList.get(i));
                    if (i == 0) {
                        setupConditionValueAndRegisterWhereClause(key, currentValue, cvalue, colName);
                    } else {
                        invokeQuery(colName, key.getConditionKey(), currentValue);
                    }
                }
            } finally {
                if (isConditionKeyInScope(key)) {
                    xgetSqlClause().closeOrScopeQuery();
                } else {
                    if (alreadyOrScopeQuery) {
                        xgetSqlClause().endOrScopeQueryAndPart();
                    }
                }
            }
        } else {
            setupConditionValueAndRegisterWhereClause(key, value, cvalue, colName);
        }
    }

    static bool isConditionKeyInScope(ConditionKey key) { // default scope for test 
        return typeof(ConditionKeyInScope).IsAssignableFrom(key.GetType());
    }

    private static List<Object> convertToJavaList(System.Collections.IList list) {
        List<Object> resultList = new ArrayList<Object>();
        foreach (Object element in list) {
            resultList.add(element);
        }
        return resultList;
    }

    private static System.Collections.IList convertToEmbeddedList<ELEMENT>(List<Object> list) {
        System.Collections.IList resultList = new System.Collections.Generic.List<ELEMENT>();
        foreach (Object element in list) {
            resultList.Add(element);
        }
        return resultList;
    }

    private static List<List<ELEMENT>> splitByLimit<ELEMENT>(List<ELEMENT> elementList, int limit) {
        List<List<ELEMENT>> valueList = new ArrayList<List<ELEMENT>>();
        int valueSize = elementList.size();
        int index = 0;
        int remainderSize = valueSize;
        do {
            int beginIndex = limit * index;
            int endPoint = beginIndex + limit;
            int endIndex = limit <= remainderSize ? endPoint : valueSize;
            List<ELEMENT> splitList = new ArrayList<ELEMENT>();
            splitList.addAll(elementList.subList(beginIndex, endIndex));
            valueList.add(splitList);
            remainderSize = valueSize - endIndex;
            ++index;
        } while (remainderSize > 0);
        return valueList;
    }

    // -----------------------------------------------------
    //                                          FromTo Query
    //                                          ------------
    protected void regFTQ(DateTime? fromDate, DateTime? toDate, ConditionValue cvalue, String colName, FromToOption option) {
        {
            ConditionKey fromKey = option.getFromDateConditionKey();
            DateTime? filteredFromDate = option.filterFromDate(fromDate);
            if (isValidQuery(fromKey, filteredFromDate, cvalue, colName)) {
                setupConditionValueAndRegisterWhereClause(fromKey, filteredFromDate, cvalue, colName);
            }
        }
        {
            ConditionKey toKey = option.getToDateConditionKey();
            DateTime? filteredToDate = option.filterToDate(toDate);
            if (isValidQuery(toKey, filteredToDate, cvalue, colName)) {
                setupConditionValueAndRegisterWhereClause(toKey, filteredToDate, cvalue, colName);
            }
        }
    }
	
    // -----------------------------------------------------
    //                                      LikeSearch Query
    //                                      ----------------
    protected void regLSQ(ConditionKey key
                        , String value
                        , ConditionValue cvalue
                        , String colName
                        , LikeSearchOption option) {
        registerLikeSearchQuery(key, value, cvalue, colName, option);
    }

    protected void registerLikeSearchQuery(ConditionKey key
                                         , String value
                                         , ConditionValue cvalue
                                         , String colName
                                         , LikeSearchOption option) {
        if (option == null) {
            throwLikeSearchOptionNotFoundException(colName, value);
            return;// Unreachable!
        }
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        if (xsuppressEscape()) {
            option.NotEscape();
        }
        if (value == null || !option.isSplit()) {
            // As Normal Condition.
            setupConditionValueAndRegisterWhereClause(key, value, cvalue, colName, option);
            return;
        }
        // - - - - - - - - -
        // Use splitByXxx().
        // - - - - - - - - -
        throw new UnsupportedOperationException("The method 'splitByXxx()' have been unsupported yet!");
    }
    
    protected virtual bool xsuppressEscape() { // for override
        return false; // as default
    }

    protected void throwLikeSearchOptionNotFoundException(String colName, String value) {
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String propertyName = dbmeta.FindPropertyName(colName);
        String capPropName = initCap(propertyName);
        String msg = "Look! Read the message below." + ln();
        msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
        msg = msg + "The likeSearchOption was Not Found! (Should not be null!)" + ln();
        msg = msg + ln();
        msg = msg + "[Advice]" + ln();
        msg = msg + "Please confirm your method call:"  + ln();
        String beanName = GetType().Name;
        String methodName = "set" + capPropName + "_LikeSearch('" + value + "', likeSearchOption);";
        msg = msg + "    " + beanName + "." + methodName + ln();
        msg = msg + "* * * * * * * * * */";
        throw new RequiredOptionNotFoundException(msg);
    }

    // -----------------------------------------------------
    //                                          Inline Query
    //                                          ------------
    protected virtual void regIQ(ConditionKey key, Object value, ConditionValue cvalue, String colName) {
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String propertyName = dbmeta.FindPropertyName(colName);
        String capPropName = initCap(propertyName);
        key.setupConditionValue(cvalue, value, getLocation(capPropName, key));// If CSharp, it is necessary to use capPropName!
        if (isBaseQuery()) {
            xgetSqlClause().registerBaseTableInlineWhereClause(colName, key, cvalue);
        } else {
            xgetSqlClause().registerOuterJoinInlineWhereClause(xgetAliasName(), colName, key, cvalue, _onClause);
        }
    }

    protected virtual void regIQ(ConditionKey key, Object value, ConditionValue cvalue
                               , String colName, ConditionOption option) {
        if (!isValidQuery(key, value, cvalue, colName)) {
            return;
        }
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String propertyName = dbmeta.FindPropertyName(colName);
        String capPropName = initCap(propertyName);
        key.setupConditionValue(cvalue, value, getLocation(capPropName, key), option);// If CSharp, it is necessary to use capPropName!
        if (isBaseQuery()) {
            xgetSqlClause().registerBaseTableInlineWhereClause(colName, key, cvalue, option);
        } else {
			xgetSqlClause().registerOuterJoinInlineWhereClause(xgetAliasName(), colName, key, cvalue, option, _onClause);
        }
    }

    // -----------------------------------------------------
    //                                       InScopeSubQuery
    //                                       ---------------
    protected virtual void registerInScopeSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName) {
        registerInScopeSubQuery(subQuery, columnName, relatedColumnName, propertyName, null);
    }

    protected virtual void registerNotInScopeSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName) {
        registerInScopeSubQuery(subQuery, columnName, relatedColumnName, propertyName, "not");
    }

    protected virtual void registerInScopeSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName
                                 , String inScopeOption) {
        assertObjectNotNull("InScopeSubQyery(" + columnName + ")", subQuery);
        inScopeOption = inScopeOption != null ? inScopeOption + " " : "";
        String realColumnName = getInScopeSubQueryRealColumnName(columnName);
        String subQueryClause = getInScopeSubQueryClause(subQuery, relatedColumnName, propertyName);
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String subQueryIdentity = propertyName + "[" + subQueryLevel + "]";
        String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
        String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
        String endIndent = "       ";
        String clause = realColumnName + " " + inScopeOption
                     + "in (" + beginMark + subQueryClause + ln() + endIndent + ")" + endMark;
        registerWhereClause(clause);
    }

    protected virtual String getInScopeSubQueryRealColumnName(String columnName) {
        return toColumnRealName(columnName);
    }

    protected virtual String getInScopeSubQueryClause(ConditionQuery subQuery
                                 , String relatedColumnName, String propertyName) {
        String tableAliasName = subQuery.xgetSqlClause().getBasePointAliasName();
        String selectClause = "select " + tableAliasName + "." + relatedColumnName;
        String fromWhereClause = buildPlainSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                 , selectClause, tableAliasName);
        return selectClause + " " + fromWhereClause;
    }

    // -----------------------------------------------------
    //                                        ExistsSubQuery
    //                                        --------------
    protected virtual void registerExistsSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName) {
        registerExistsSubQuery(subQuery, columnName, relatedColumnName, propertyName, null);
    }

    protected virtual void registerNotExistsSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName) {
        registerExistsSubQuery(subQuery, columnName, relatedColumnName, propertyName, "not");
    }

    protected virtual void registerExistsSubQuery(ConditionQuery subQuery
                                 , String columnName, String relatedColumnName, String propertyName
                                 , String existsOption) {
        assertObjectNotNull("ExistsSubQyery(" + columnName + ")", subQuery);
        existsOption = existsOption != null ? existsOption + " " : "";
        String realColumnName = getExistsSubQueryRealColumnName(columnName);
        String subQueryClause = getExistsSubQueryClause(subQuery, realColumnName, relatedColumnName, propertyName);
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String subQueryIdentity = propertyName + "[" + subQueryLevel + "]";
        String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
        String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
        String endIndent = "       ";
        String clause = existsOption + "exists (" + beginMark + subQueryClause + ln() + endIndent + ")" + endMark;
        registerWhereClause(clause);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    // *Unsupport ExistsSubQuery as inline because it's so dangerous.
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

    protected virtual String getExistsSubQueryRealColumnName(String columnName) {
        return toColumnRealName(columnName);
    }

    protected virtual String getExistsSubQueryClause(ConditionQuery subQuery
                                 , String realColumnName, String relatedColumnName, String propertyName) {
		int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
		String tableAliasName = subQuery.xgetSqlClause().getBasePointAliasName();
        String selectClause = "select " + tableAliasName + "." + relatedColumnName;
        String fromWhereClause = buildCorrelationSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                       , selectClause, tableAliasName, realColumnName);
		return selectClause + " " + fromWhereClause;
    }

    // [DBFlute-0.7.9]
    // -----------------------------------------------------
    //                              (Specify)DerivedReferrer
    //                              ------------------------
    protected void registerSpecifyDerivedReferrer(String function, ConditionQuery subQuery
                                                , String columnName, String relatedColumnName
                                                , String propertyName, String aliasName) {
        assertObjectNotNull("DerivedReferrerSubQuery(function)", function);
        assertObjectNotNull("DerivedReferrerSubQuery(" + columnName + ")", subQuery);
        String realColumnName = getSpecifyDerivedReferrerRealColumnName(columnName);
        String subQueryClause = getSpecifyDerivedReferrerSubQueryClause(function, subQuery, realColumnName
                                                                      , relatedColumnName, propertyName, aliasName);
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String subQueryIdentity = propertyName + "[" + subQueryLevel + "]";
        String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
        String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
        String endIndent = "       ";
        String clause = "(" + beginMark + subQueryClause + ln() + endIndent + ") as " + aliasName + endMark;
        xgetSqlClause().specifyDeriveSubQuery(aliasName, clause);
    }

    protected String getSpecifyDerivedReferrerRealColumnName(String columnName) {
        return toColumnRealName(columnName);
    }

    protected String getSpecifyDerivedReferrerSubQueryClause(String function, ConditionQuery subQuery
                                                           , String realColumnName, String relatedColumnName
                                                           , String propertyName, String aliasName) {
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String tableAliasName = subQuery.xgetSqlClause().getBasePointAliasName();
        String deriveColumnName = subQuery.xgetSqlClause().getSpecifiedColumnNameAsOne();
        if (deriveColumnName == null || deriveColumnName.Trim().Length == 0) {
            throwSpecifyDerivedReferrerInvalidColumnSpecificationException(function, aliasName);
        }
        assertSpecifyDerivedReferrerColumnType(function, subQuery, deriveColumnName);
        subQuery.xgetSqlClause().clearSpecifiedSelectColumn();
        String connect = xbuildFunctionConnector(function);
        if (subQuery.xgetSqlClause().hasUnionQuery()) {
            String subQueryIdentity = propertyName + "[" + subQueryLevel + ":subquerymain]";
            String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
            String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
            DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(subQuery.getTableDbName());
            if (!dbmeta.HasPrimaryKey || dbmeta.HasCompoundPrimaryKey) {
                String msg = "The derived-referrer is unsupported when no primary key or two-or-more primary keys:";
                msg = msg + " table=" + subQuery.getTableDbName();
                throw new NotSupportedException(msg);
            }
            String primaryKeyName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
            String selectClause = "select " + tableAliasName + "." + primaryKeyName 
                                     + ", " + tableAliasName + "." + relatedColumnName
                                     + ", " + tableAliasName + "." + deriveColumnName;
            String fromWhereClause = buildPlainSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                     , selectClause, tableAliasName);
            String mainSql = selectClause + " " + fromWhereClause;
            String joinCondition = "dfsubquerymain." + relatedColumnName + " = " + realColumnName;
            return "select " + function + connect + "dfsubquerymain." + deriveColumnName + ")" + ln()
                 + "  from (" + beginMark
                 + mainSql + ln()
                 + "       ) dfsubquerymain" + endMark + ln() + " where " + joinCondition;
        } else {
            String selectClause = "select " + function + connect + tableAliasName + "." + deriveColumnName + ")";
            String fromWhereClause = buildCorrelationSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                           , selectClause, tableAliasName, realColumnName);
            return selectClause + " " + fromWhereClause;
        }
    }

    protected void throwSpecifyDerivedReferrerInvalidColumnSpecificationException(String function, String aliasName) {
        String method = xconvertFunctionToMethod(function);
        String msg = "Look! Read the message below." + ln();
        msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
        msg = msg + "The specified the column for derived-referrer was Invalid!" + ln();
        msg = msg + ln();
        msg = msg + "[Advice]" + ln();
        msg = msg + " You should call specify().column[TargetColumn]() only once." + ln();
        msg = msg + "  For example:" + ln();
        msg = msg + "    " + ln();
        msg = msg + "    [Wrong]" + ln();
        msg = msg + "    /- - - - - - - - - - - - - - - - - - - - " + ln();
        msg = msg + "    MemberCB cb = new MemberCB();" + ln();
        msg = msg + "    cb.specify().derivePurchaseList()." + method + "(new SubQuery<PurchaseCB>() {" + ln();
        msg = msg + "        public void query(PurchaseCB subCB) {" + ln();
        msg = msg + "            // *No! It's empty!" + ln();
        msg = msg + "        }" + ln();
        msg = msg + "    }, \"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    - - - - - - - - - -/" + ln();
        msg = msg + "    " + ln();
        msg = msg + "    [Wrong]" + ln();
        msg = msg + "    /- - - - - - - - - - - - - - - - - - - - " + ln();
        msg = msg + "    MemberCB cb = new MemberCB();" + ln();
        msg = msg + "    cb.specify().derivePurchaseList()." + method + "(new SubQuery<PurchaseCB>() {" + ln();
        msg = msg + "        public void query(PurchaseCB subCB) {" + ln();
        msg = msg + "            subCB.specify().columnPurchaseDatetime();" + ln();
        msg = msg + "            subCB.specify().columnPurchaseCount(); // *No! It's duplicated!" + ln();
        msg = msg + "        }" + ln();
        msg = msg + "    }, \"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    - - - - - - - - - -/" + ln();
        msg = msg + "    " + ln();
        msg = msg + "    [Good!]" + ln();
        msg = msg + "    /- - - - - - - - - - - - - - - - - - - - " + ln();
        msg = msg + "    MemberCB cb = new MemberCB();" + ln();
        msg = msg + "    cb.specify().derivePurchaseList()." + method + "(new SubQuery<PurchaseCB>() {" + ln();
        msg = msg + "        public void query(PurchaseCB subCB) {" + ln();
        msg = msg + "            subCB.specify().columnPurchaseDatetime(); // *Point!" + ln();
        msg = msg + "        }" + ln();
        msg = msg + "    }, \"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    - - - - - - - - - -/" + ln();
        msg = msg + ln();
        msg = msg + "[Alias Name]" + ln() + aliasName + ln();
        msg = msg + "* * * * * * * * * */";
        throw new SpecifyDerivedReferrerInvalidColumnSpecificationException(msg);
    }

    public class SpecifyDerivedReferrerInvalidColumnSpecificationException : SystemException {
        public SpecifyDerivedReferrerInvalidColumnSpecificationException(String msg) : base(msg) {
        }
    }
    
    protected void assertSpecifyDerivedReferrerColumnType(String function, ConditionQuery subQuery, String deriveColumnName) {
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(subQuery.getTableDbName());
        Type deriveColumnType = dbmeta.FindColumnInfo(deriveColumnName).PropertyType;
        if ("sum".Equals(function.ToLower()) || "avg".Equals(function.ToLower())) {
            // Determine as not string and not date because CSharp does not have abstract class of number.
            if (typeof(String).IsAssignableFrom(deriveColumnType) || typeof(DateTime).IsAssignableFrom(deriveColumnType)) {
                throwSpecifyDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType);
            }
        }
    }

    protected void throwSpecifyDerivedReferrerUnmatchedColumnTypeException(String function, String deriveColumnName, Type deriveColumnType) {
        String msg = "Look! Read the message below." + ln();
        msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
        msg = msg + "The type of the specified the column unmatched with the function!" + ln();
        msg = msg + ln();
        msg = msg + "[Advice]" + ln();
        msg = msg + "You should confirm the list as follow:" + ln();
        msg = msg + "    max()   : String, Number, Date" + ln();
        msg = msg + "    min()   : String, Number, Date" + ln();
        msg = msg + "    sum()   : Number" + ln();
        msg = msg + "    avg()   : Number" + ln();
        msg = msg + "    count() : String, Number, Date" + ln();
        msg = msg + ln();
        msg = msg + "[Function]" + ln() + function + ln();
        msg = msg + ln();
        msg = msg + "[Derive Column]" + ln() + deriveColumnName + "(" + deriveColumnType.Name + ")" + ln();
        msg = msg + "* * * * * * * * * */";
        throw new SpecifyDerivedReferrerUnmatchedColumnTypeException(msg);
    }

    public class SpecifyDerivedReferrerUnmatchedColumnTypeException : SystemException {
        public SpecifyDerivedReferrerUnmatchedColumnTypeException(String msg) : base(msg) {
        }
    }

    // [DBFlute-0.8.9.2]
    // -----------------------------------------------------
    //                                (Query)DerivedReferrer
    //                                ----------------------
    protected void registerQueryDerivedReferrer(String function, ConditionQuery subQuery
                                              , String columnName, String relatedColumnName, String propertyName
                                              , String operand, Object value, String parameterPropertyName) {
        assertObjectNotNull("QueryDerivedReferrer(function)", function);
        assertObjectNotNull("QueryDerivedReferrer(" + columnName + ")", subQuery);
        String realColumnName = getQueryDerivedReferrerRealColumnName(columnName);
        String subQueryClause = getQueryDerivedReferrerSubQueryClause(function, subQuery, realColumnName
                                                                    , relatedColumnName, propertyName, value);
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String subQueryIdentity = propertyName + "[" + subQueryLevel + "]";
        String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
        String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
        String endIndent = "       ";
        String parameter = "/*pmb." + xgetLocationBase(parameterPropertyName) + "*/null";
        String clause = "(" + beginMark
                      + subQueryClause + ln() + endIndent
                      + ") " + operand + " " + parameter + " " + endMark;
        registerWhereClause(clause);
    }

    protected String getQueryDerivedReferrerRealColumnName(String columnName) {
        return toColumnRealName(columnName);
    }

    protected String getQueryDerivedReferrerSubQueryClause(String function, ConditionQuery subQuery
                                                         , String realColumnName, String relatedColumnName
                                                         , String propertyName, Object value) {
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String tableAliasName = subQuery.xgetSqlClause().getBasePointAliasName();
        String deriveColumnName = subQuery.xgetSqlClause().getSpecifiedColumnNameAsOne();
        if (deriveColumnName == null || deriveColumnName.Trim().Length == 0) {
            throwQueryDerivedReferrerInvalidColumnSpecificationException(function);
        }
        assertQueryDerivedReferrerColumnType(function, subQuery, deriveColumnName, value);
        subQuery.xgetSqlClause().clearSpecifiedSelectColumn(); // specified columns disappear at this timing
        String connect = xbuildFunctionConnector(function);
        if (subQuery.xgetSqlClause().hasUnionQuery()) {
            String subQueryIdentity = propertyName + "[" + subQueryLevel + ":subquerymain]";
            String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
            String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
            DBMeta dbmeta = findDBMeta(subQuery.getTableDbName());
            if (!dbmeta.HasPrimaryKey || dbmeta.HasCompoundPrimaryKey) {
                String msg = "The derived-referrer is unsupported when no primary key or two-or-more primary keys:";
                msg = msg + " table=" + subQuery.getTableDbName();
                throw new UnsupportedOperationException(msg);
            }
            String primaryKeyName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
            String selectClause = "select " + tableAliasName + "." + primaryKeyName 
                                     + ", " + tableAliasName + "." + relatedColumnName
                                     + ", " + tableAliasName + "." + deriveColumnName;
            String fromWhereClause = buildPlainSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                     , selectClause, tableAliasName);
            String mainSql = selectClause + " " + fromWhereClause;
            String joinCondition = "dfsubquerymain." + relatedColumnName + " = " + realColumnName;
            return "select " + function + connect + "dfsubquerymain." + deriveColumnName + ")" + ln()
                 + "  from (" + beginMark
                 + mainSql + ln()
                 + "       ) dfsubquerymain" + endMark + ln() + " where " + joinCondition;
        } else {
            String selectClause = "select " + function + connect + tableAliasName + "." + deriveColumnName + ")";
            String fromWhereClause = buildCorrelationSubQueryFromWhereClause(subQuery, relatedColumnName, propertyName
                                                                           , selectClause, tableAliasName, realColumnName);
            return selectClause + " " + fromWhereClause;
        }
    }

    protected void throwQueryDerivedReferrerInvalidColumnSpecificationException(String function) {
        ConditionBeanContext.ThrowQueryDerivedReferrerInvalidColumnSpecificationException(function);
    }

    protected void assertQueryDerivedReferrerColumnType(String function, ConditionQuery subQuery, String deriveColumnName, Object value) {
        DBMeta dbmeta = findDBMeta(subQuery.getTableDbName());
        Type deriveColumnType = dbmeta.FindColumnInfo(deriveColumnName).PropertyType;
        if ("sum".Equals(function.ToLower()) || "avg".Equals(function.ToLower())) {
            // Determine as not string and not date because CSharp does not have abstract class of number.
            if (typeof(String).IsAssignableFrom(deriveColumnType) || typeof(DateTime).IsAssignableFrom(deriveColumnType)) {
                throwQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
            }
        }
        if (value != null) {
            Type parameterType = value.GetType();
            if (typeof(String).IsAssignableFrom(deriveColumnType)) {
                if (!typeof(String).IsAssignableFrom(parameterType)) {
                    throwQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
                }
            }

            // Check main number type only because CSharp does not have abstract class of number.
            if (typeof(int).IsAssignableFrom(deriveColumnType)) {
                if (!typeof(int).IsAssignableFrom(parameterType)) {
                    throwQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
                }
            }
            if (typeof(decimal).IsAssignableFrom(deriveColumnType)) {
                if (!typeof(decimal).IsAssignableFrom(parameterType)) {
                    throwQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
                }
            }

            if (typeof(DateTime).IsAssignableFrom(deriveColumnType)) {
                if (!typeof(DateTime).IsAssignableFrom(parameterType)) {
                    throwQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
                }
            }
        }
    }

    protected void throwQueryDerivedReferrerUnmatchedColumnTypeException(String function, String deriveColumnName, Type deriveColumnType, Object value) {
        ConditionBeanContext.ThrowQueryDerivedReferrerUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType, value);
    }

    public class QDRFunction<CB> where CB : ConditionBean { // Internal
        protected QDRSetupper<CB> _setupper;
        public QDRFunction(QDRSetupper<CB> setupper) {
            _setupper = setupper;
        }
        
        /**
         * Set up the sub query of referrer for the scalar 'count'.
         * <pre>
         * cb.query().scalarPurchaseList().count(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchaseId(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, int> Count(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, int>("count", subQuery, _setupper);
        }
        
        /**
         * Set up the sub query of referrer for the scalar 'count(with distinct)'.
         * <pre>
         * cb.query().scalarPurchaseList().countDistinct(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, int> CountDistinct(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, int>("count(distinct", subQuery, _setupper);
        }

        /**
         * Set up the sub query of referrer for the scalar 'max'.
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, Object> Max(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, Object>("max", subQuery, _setupper);
        }
        
        /**
         * Set up the sub query of referrer for the scalar 'min'.
         * <pre>
         * cb.query().scalarPurchaseList().min(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, Object> Min(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, Object>("min", subQuery, _setupper);
        }
        
        /**
         * Set up the sub query of referrer for the scalar 'sum'.
         * <pre>
         * cb.query().scalarPurchaseList().sum(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, Object> Sum(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, Object>("sum", subQuery, _setupper);
        }
        
        /**
         * Set up the sub query of referrer for the scalar 'avg'.
         * <pre>
         * cb.query().scalarPurchaseList().avg(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // *Don't forget the parameter!
         * </pre> 
         * @param subQuery The sub query of referrer. (NotNull) 
         * @return The parameter for comparing with scalar. (NotNull)
         */
        public QDRParameter<CB, Object> Avg(SubQuery<CB> subQuery) {
            return new QDRParameter<CB, Object>("avg", subQuery, _setupper);
        }
    }

    public delegate void QDRSetupper<CB>(String function
                                       , SubQuery<CB> subQuery
                                       , String operand
                                       , Object value) where CB : ConditionBean;

    public class QDRParameter<CB, PARAMETER> where CB : ConditionBean { // Internal
        protected String _function;
        protected SubQuery<CB> _subQuery;
        protected QDRSetupper<CB> _setupper;
        public QDRParameter(String function, SubQuery<CB> subQuery, QDRSetupper<CB> setupper) {
            _function = function;
            _subQuery = subQuery;
            _setupper = setupper;
        }

        /**
         * Set up the operand 'equal' and the value of parameter. <br />
         * The type of the parameter should be same as the type of target column. 
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // If the type is Integer...
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).equal(123); // This parameter should be Integer!
         * </pre> 
         * @param value The value of parameter. (NotNull) 
         */
        public void Equal(PARAMETER value) {
            _setupper.Invoke(_function, _subQuery, "=", value);
        }
        
        /**
         * Set up the operand 'greaterThan' and the value of parameter. <br />
         * The type of the parameter should be same as the type of target column. 
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // If the type is Integer...
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterThan(123); // This parameter should be Integer!
         * </pre> 
         * @param value The value of parameter. (NotNull) 
         */
        public void GreaterThan(PARAMETER value) {
            _setupper.Invoke(_function, _subQuery, ">", value);
        }
        
        /**
         * Set up the operand 'lessThan' and the value of parameter. <br />
         * The type of the parameter should be same as the type of target column. 
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // If the type is Integer...
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).lessThan(123); // This parameter should be Integer!
         * </pre> 
         * @param value The value of parameter. (NotNull) 
         */
        public void LessThan(PARAMETER value) {
            _setupper.Invoke(_function, _subQuery, "<", value);
        }
        
        /**
         * Set up the operand 'greaterEqual' and the value of parameter. <br />
         * The type of the parameter should be same as the type of target column. 
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // If the type is Integer...
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).greaterEqual(123); // This parameter should be Integer!
         * </pre> 
         * @param value The value of parameter. (NotNull) 
         */
        public void GreaterEqual(PARAMETER value) {
            _setupper.Invoke(_function, _subQuery, ">=", value);
        }
        
        /**
         * Set up the operand 'lessEqual' and the value of parameter. <br />
         * The type of the parameter should be same as the type of target column. 
         * <pre>
         * cb.query().scalarPurchaseList().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // If the type is Integer...
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * }).lessEqual(123); // This parameter should be Integer!
         * </pre> 
         * @param value The value of parameter. (NotNull) 
         */
        public void LessEqual(PARAMETER value) {
            _setupper.Invoke(_function, _subQuery, "<=", value);
        }
    }

    // [DBFlute-0.8.8]
    // -----------------------------------------------------
    //                                        ScalarSubQuery
    //                                        --------------
    protected void registerScalarSubQuery(String function, ConditionQuery subQuery
                                        , String propertyName, String operand) {
        assertObjectNotNull("ScalarSubQuery(" + propertyName + ")", subQuery);
        
        // Get the specified column before it disappears at sub-query making.
        String deriveRealColumnName;
        {
            String deriveColumnName = subQuery.xgetSqlClause().getSpecifiedColumnNameAsOne();
            if (deriveColumnName == null || deriveColumnName.Trim().Length == 0) {
                throwScalarSubQueryInvalidColumnSpecificationException(function);
            }
            deriveRealColumnName = getScalarSubQueryRealColumnName(deriveColumnName);
        }

        String subQueryClause = getScalarSubQueryClause(function, subQuery, propertyName);
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String subQueryIdentity = propertyName + "[" + subQueryLevel + "]";
        String beginMark = subQuery.xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
        String endMark = subQuery.xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
        String endIndent = "       ";
        String clause = deriveRealColumnName + " " + operand + " ("
                      + beginMark + subQueryClause + ln() + endIndent
                      + ") " + endMark;
        registerWhereClause(clause);
    }

    protected String getScalarSubQueryRealColumnName(String columnName) {
        return toColumnRealName(columnName);
    }

    protected String getScalarSubQueryClause(String function, ConditionQuery subQuery
                                           , String propertyName) {
        int subQueryLevel = subQuery.xgetSqlClause().getSubQueryLevel();
        String tableAliasName = subQuery.xgetSqlClause().getBasePointAliasName();
        String deriveColumnName = subQuery.xgetSqlClause().getSpecifiedColumnNameAsOne();
        if (deriveColumnName == null || deriveColumnName.Trim().Length == 0) {
            throwScalarSubQueryInvalidColumnSpecificationException(function);
        }
        assertScalarSubQueryColumnType(function, subQuery, deriveColumnName);
        subQuery.xgetSqlClause().clearSpecifiedSelectColumn(); // specified columns disappear at this timing
        DBMeta dbmeta = findDBMeta(subQuery.getTableDbName());
        if (!dbmeta.HasPrimaryKey || dbmeta.HasCompoundPrimaryKey) {
            String msg = "The scalar-sub-query is unsupported when no primary key or two-or-more primary keys:";
            msg = msg + " table=" + subQuery.getTableDbName();
            throw new UnsupportedOperationException(msg);
        }
        String primaryKeyName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
        if (subQuery.xgetSqlClause().hasUnionQuery()) {
            String subQueryIdentity = propertyName + "[" + subQueryLevel + ":subquerymain]";
            String beginMark = xgetSqlClause().resolveSubQueryBeginMark(subQueryIdentity) + ln();
            String endMark = xgetSqlClause().resolveSubQueryEndMark(subQueryIdentity);
            String selectClause = "select " + tableAliasName + "." + primaryKeyName
                                     + ", " + tableAliasName + "." + deriveColumnName;
            String fromWhereClause = buildPlainSubQueryFromWhereClause(subQuery, primaryKeyName, propertyName
                                                                     , selectClause, tableAliasName);
            String mainSql = selectClause + " " + fromWhereClause;
            return "select " + function + "(dfsubquerymain." + deriveColumnName + ")" + ln()
                 + "  from (" + beginMark
                 + mainSql + ln()
                 + "       ) dfsubquerymain" + endMark;
        } else {
            String selectClause = "select " + function + "(" + tableAliasName + "." + deriveColumnName + ")";
            String fromWhereClause = buildPlainSubQueryFromWhereClause(subQuery, primaryKeyName, propertyName
                                                                     , selectClause, tableAliasName);
            return selectClause + " " + fromWhereClause;
        }
    }

    protected void throwScalarSubQueryInvalidColumnSpecificationException(String function) {
        ConditionBeanContext.ThrowScalarSubQueryInvalidColumnSpecificationException(function);
    }

    protected void assertScalarSubQueryColumnType(String function, ConditionQuery subQuery, String deriveColumnName) {
        DBMeta dbmeta = findDBMeta(subQuery.getTableDbName());
        Type deriveColumnType = dbmeta.FindColumnInfo(deriveColumnName).PropertyType;
        if ("sum".Equals(function.ToLower()) || "avg".Equals(function.ToLower())) {
            // Determine as not string and not date because CSharp does not have abstract class of number.
            if (typeof(String).IsAssignableFrom(deriveColumnType) || typeof(DateTime).IsAssignableFrom(deriveColumnType)) {
                throwScalarSubQueryUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType);
            }
        }
    }

    protected void throwScalarSubQueryUnmatchedColumnTypeException(String function, String deriveColumnName, Type deriveColumnType) {
        ConditionBeanContext.ThrowScalarSubQueryUnmatchedColumnTypeException(function, deriveColumnName, deriveColumnType);
    }

    public class SSQFunction<CB> where CB : ConditionBean { // Internal
        protected SSQSetupper<CB> _setupper;
        public SSQFunction(SSQSetupper<CB> setupper) {
            _setupper = setupper;
        }

        /**
         * Set up the sub query of myself for the scalar 'max'.
         * <pre>
         * cb.query().scalar_Equal().max(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * });
         * </pre> 
         * @param subQuery The sub query of myself. (NotNull) 
         */
        public void Max(SubQuery<CB> subQuery) {
            _setupper.Invoke("max", subQuery);
        }

        /**
         * Set up the sub query of myself for the scalar 'min'.
         * <pre>
         * cb.query().scalar_Equal().min(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * });
         * </pre> 
         * @param subQuery The sub query of myself. (NotNull) 
         */
        public void Min(SubQuery<CB> subQuery) {
            _setupper.Invoke("min", subQuery);
        }

        /**
         * Set up the sub query of myself for the scalar 'sum'.
         * <pre>
         * cb.query().scalar_Equal().sum(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * });
         * </pre> 
         * @param subQuery The sub query of myself. (NotNull) 
         */
        public void Sum(SubQuery<CB> subQuery) {
            _setupper.Invoke("sum", subQuery);
        }

        /**
         * Set up the sub query of myself for the scalar 'avg'.
         * <pre>
         * cb.query().scalar_Equal().avg(new SubQuery&lt;PurchaseCB&gt;() {
         *     public void query(PurchaseCB subCB) {
         *         subCB.specify().columnPurchasePrice(); // *Point!
         *         subCB.query().setPaymentCompleteFlg_Equal_True();
         *     }
         * });
         * </pre> 
         * @param subQuery The sub query of myself. (NotNull) 
         */
        public void Avg(SubQuery<CB> subQuery) {
            _setupper.Invoke("avg", subQuery);
        }
    }

    public delegate void SSQSetupper<CB>(String function
                                       , SubQuery<CB> subQuery)
                                       where CB : ConditionBean; // Internal

    // -----------------------------------------------------
    //                                       SubQuery Common
    //                                       ---------------
    protected String buildPlainSubQueryFromWhereClause(ConditionQuery subQuery
                                                     , String relatedColumnName
                                                     , String propertyName
                                                     , String selectClause
                                                     , String tableAliasName) {
        String fromWhereClause = subQuery.xgetSqlClause().getClauseFromWhereWithUnionTemplate();

        // Resolve the location path for the condition-query of sub-query. 
        fromWhereClause = replaceString(fromWhereClause, "." + CQ_PROPERTY + ".", "." + xgetLocationBase(propertyName) + ".");

		// Replace template marks. These are very important!
		fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionSelectClauseMark(), selectClause);
		fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionWhereClauseMark(), "");
		fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionWhereFirstConditionMark(), "");
		return fromWhereClause;
    }

    protected String buildCorrelationSubQueryFromWhereClause(ConditionQuery subQuery
                                                           , String relatedColumnName
                                                           , String propertyName
                                                           , String selectClause
                                                           , String tableAliasName
                                                           , String realColumnName) {
        String fromWhereClause = subQuery.xgetSqlClause().getClauseFromWhereWithWhereUnionTemplate();

        // Resolve the location path for the condition-query of sub-query. 
        fromWhereClause = replaceString(fromWhereClause, "." + CQ_PROPERTY + ".", "." + xgetLocationBase(propertyName) + ".");

        String joinCondition = tableAliasName + "." + relatedColumnName + " = " + realColumnName;
        String firstConditionAfter = ln() + "   and ";
        
        // Replace template marks. These are very important!
        fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getWhereClauseMark(), "where " + joinCondition);
        fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getWhereFirstConditionMark(), joinCondition + firstConditionAfter);
        fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionSelectClauseMark(), selectClause);
        fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionWhereClauseMark(), "where " + joinCondition);
        fromWhereClause = replaceString(fromWhereClause, subQuery.xgetSqlClause().getUnionWhereFirstConditionMark(), joinCondition + firstConditionAfter);
        return fromWhereClause;
    }

    protected String xbuildFunctionConnector(String function) {
        if (function != null && function.EndsWith("(distinct")) { // For example 'count(distinct'
            return " ";
        } else {
            return "(";
        }
    }

    protected String xconvertFunctionToMethod(String function) {
        if (function != null && function.Contains("(")) { // For example 'count(distinct'
            int index = function.IndexOf("(");
            String front = function.Substring(0, index);
            if (function.Length > front.Length + "(".Length) {
                String rear = function.Substring(index + "(".Length);
                function = front + initCap(rear);
            } else {
                function = front;
            }
        }
        return function;
    }

    // -----------------------------------------------------
    //                                          Where Clause
    //                                          ------------
    protected virtual void setupConditionValueAndRegisterWhereClause(ConditionKey key, Object value, ConditionValue cvalue, String colName) {
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String propertyName = dbmeta.FindPropertyName(colName);
        String capPropName = initCap(propertyName);
        // If CSharp, it is necessary to use capPropName!
        key.setupConditionValue(cvalue, value, getLocation(capPropName, key));
        xgetSqlClause().registerWhereClause(toColumnRealName(colName), key, cvalue);
    }

    protected virtual void setupConditionValueAndRegisterWhereClause(ConditionKey key, Object value, ConditionValue cvalue
                                                                   , String colName, ConditionOption option) {
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String propertyName = dbmeta.FindPropertyName(colName);
        String capPropName = initCap(propertyName);
        // If CSharp, it is necessary to use capPropName!
        key.setupConditionValue(cvalue, value, getLocation(capPropName, key), option);
        xgetSqlClause().registerWhereClause(toColumnRealName(colName), key, cvalue, option);
    }
	
    protected virtual void registerWhereClause(String whereClause) {
        xgetSqlClause().registerWhereClause(whereClause);
    }

    protected virtual void registerInlineWhereClause(String whereClause) {
        if (isBaseQuery()) {
            xgetSqlClause().registerBaseTableInlineWhereClause(whereClause);
        } else {
            xgetSqlClause().registerOuterJoinInlineWhereClause(xgetAliasName(), whereClause, _onClause);
        }
    }

    // -----------------------------------------------------
    //                                           Union Query
    //                                           -----------
    public void registerUnionQuery(ConditionQuery unionQuery, bool unionAll, String unionQueryPropertyName) {
        String unionQueryClause = getUnionQuerySql(unionQuery, unionQueryPropertyName);
		
		// At the future, building SQL will be moved to sqlClause.
        xgetSqlClause().registerUnionQuery(unionQueryClause, unionAll);
    }

    protected String getUnionQuerySql(ConditionQuery unionQuery, String unionQueryPropertyName) {
	    String fromClause = unionQuery.xgetSqlClause().getFromClause();
		String whereClause = unionQuery.xgetSqlClause().getWhereClause();
		String unionQueryClause;
		if (whereClause.Trim().Length <= 0) {
		    unionQueryClause = fromClause + " " + xgetSqlClause().getUnionWhereClauseMark();
		} else {
		    int whereIndex = whereClause.IndexOf("where ");
		    if (whereIndex < 0) {
				String msg = "The whereClause should have 'where' string: " + whereClause;
			    throw new IllegalStateException(msg);
			}
			int clauseIndex = whereIndex + "where ".Length;
			String mark = xgetSqlClause().getUnionWhereFirstConditionMark();
			String markedClause = whereClause.Substring(0, clauseIndex) + mark + whereClause.Substring(clauseIndex);
			unionQueryClause = fromClause + " " + markedClause;
		}
        String oldStr = "." + CQ_PROPERTY + ".";
        String newStr = "." + CQ_PROPERTY + "." + unionQueryPropertyName + ".";
        return replaceString(unionQueryClause, oldStr, newStr); // Very Important!
    }

    // -----------------------------------------------------
    //                                            Inner Join
    //                                            ----------
    public void InnerJoin() {
        if (isBaseQuery()) {
            String msg = "Look! Read the message below." + ln();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
            msg = msg + "The method 'InnerJoin()' should be called for a relation query!" + ln();
            msg = msg + ln();
            msg = msg + "[Advice]" + ln();
            msg = msg + "Please confirm your program. " + ln();
            msg = msg + "  For example:" + ln();
            msg = msg + "    (x) - cb.Query().InnerJoin();" + ln();
            msg = msg + "    (o) - cb.Query().QueryMemberStatusCode().InnerJoin();" + ln();
            msg = msg + "* * * * * * * * * */";
            throw new IllegalStateException(msg);
        }
        xgetSqlClause().changeToInnerJoin(xgetAliasName());
    }

    // -----------------------------------------------------
    //                                               OrderBy
    //                                               -------
	public void WithNullsFirst() {// is User Public!
	    xgetSqlClause().addNullsFirstToPreviousOrderBy();
	}
	
	public void WithNullsLast() {// is User Public!
	    xgetSqlClause().addNullsLastToPreviousOrderBy();
	}

    protected void registerSpecifiedDerivedOrderBy_Asc(String aliasName) {
        if (!xgetSqlClause().hasSpecifiedDeriveSubQuery(aliasName)) {
            throwSpecifiedDerivedOrderByAliasNameNotFoundException(aliasName);
        }
        xgetSqlClause().registerOrderBy(aliasName, null, true);
    }

    protected void registerSpecifiedDerivedOrderBy_Desc(String aliasName) {
        if (!xgetSqlClause().hasSpecifiedDeriveSubQuery(aliasName)) {
            throwSpecifiedDerivedOrderByAliasNameNotFoundException(aliasName);
        }
        xgetSqlClause().registerOrderBy(aliasName, null, false);
    }

    protected void throwSpecifiedDerivedOrderByAliasNameNotFoundException(String aliasName) {
        String msg = "Look! Read the message below." + ln();
        msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
        msg = msg + "The aliasName was Not Found in specified alias names." + ln();
        msg = msg + ln();
        msg = msg + "[Advice]" + ln();
        msg = msg + "You should specified an alias name that is the same as one in specify-derived-referrer." + ln();
        msg = msg + "  For example:" + ln();
        msg = msg + "    " + ln();
        msg = msg + "    [Wrong]" + ln();
        msg = msg + "    /- - - - - - - - - - - - - - - - - - - - " + ln();
        msg = msg + "    MemberCB cb = new MemberCB();" + ln();
        msg = msg + "    cb.specify().derivePurchaseList().max(new SubQuery<PurchaseCB>() {" + ln();
        msg = msg + "        public void query(PurchaseCB subCB) {" + ln();
        msg = msg + "            subCB.specify().specifyProduct().columnProductName(); // *No!" + ln();
        msg = msg + "        }" + ln();
        msg = msg + "    }, \"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    cb.query().addSpecifiedDerivedOrderBy_Desc(\"WRONG_NAME_DATETIME\");" + ln();
        msg = msg + "    - - - - - - - - - -/" + ln();
        msg = msg + "    " + ln();
        msg = msg + "    [Good!]" + ln();
        msg = msg + "    /- - - - - - - - - - - - - - - - - - - - " + ln();
        msg = msg + "    MemberCB cb = new MemberCB();" + ln();
        msg = msg + "    cb.specify().derivePurchaseList().max(new SubQuery<PurchaseCB>() {" + ln();
        msg = msg + "        public void query(PurchaseCB subCB) {" + ln();
        msg = msg + "            subCB.specify().columnPurchaseDatetime();// *Point!" + ln();
        msg = msg + "        }" + ln();
        msg = msg + "    }, \"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    cb.query().addSpecifiedDerivedOrderBy_Desc(\"LATEST_PURCHASE_DATETIME\");" + ln();
        msg = msg + "    - - - - - - - - - -/" + ln();
        msg = msg + ln();
        msg = msg + "[Not Found Alias Name]" + ln() + aliasName + ln();
        msg = msg + "* * * * * * * * * */";
        throw new SpecifiedDerivedOrderByAliasNameNotFoundException(msg);
    }

    public class SpecifiedDerivedOrderByAliasNameNotFoundException : SystemException {
        public SpecifiedDerivedOrderByAliasNameNotFoundException(String msg) : base(msg) {
        }
    }

    public void registerOrderBy(String columnName, bool ascOrDesc) {
        xgetSqlClause().registerOrderBy(toColumnRealName(columnName), null, ascOrDesc);
    }

    protected void regOBA(String columnName) {
        registerOrderBy(columnName, true);
    }

    protected void regOBD(String columnName) {
        registerOrderBy(columnName, false);
    }

    // ===================================================================================
    //                                                                       Name Resolver
    //                                                                       =============
    protected String resolveJoinAliasName(String relationPath, int nestLevel) {
        return xgetSqlClause().resolveJoinAliasName(relationPath, nestLevel);
    }

	protected String resolveNextRelationPath(String tableName, String relationPropertyName) {
	    int relationNo = xgetSqlClause().resolveRelationNo(tableName, relationPropertyName);
        String nextRelationPath = "_" + relationNo;
        if (_relationPath != null) {
            nextRelationPath = _relationPath + nextRelationPath;
        }
		return nextRelationPath;
	}

    // ===================================================================================
    //                                                                 Reflection Invoking
    //                                                                 ===================
    public ConditionValue invokeValue(String columnFlexibleName) {
        assertStringNotNullAndNotTrimmedEmpty("columnFlexibleName", columnFlexibleName);
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(getTableDbName());
        String columnCapPropName = initCap(dbmeta.FindPropertyName(columnFlexibleName));
        String propertyName = columnCapPropName;
        PropertyInfo property = helpGettingCQProperty(this, propertyName);
        return (ConditionValue)helpInvokingCQProperty(this, property);
    }

    public void invokeQuery(String columnFlexibleName, String conditionKeyName, Object value) {
        assertStringNotNullAndNotTrimmedEmpty("columnFlexibleName", columnFlexibleName);
        assertStringNotNullAndNotTrimmedEmpty("conditionKeyName", conditionKeyName);
        if (value == null) {
            return;
        }
        PropertyNameCQContainer container = helpExtractingPropertyNameCQContainer(columnFlexibleName);
        String propertyName = container.getPropertyName();
        ConditionQuery cq = container.getConditionQuery();
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(cq.getTableDbName());
        String columnCapPropName = initCap(dbmeta.FindPropertyName(propertyName));
        String methodName = "Set" + columnCapPropName + "_" + initCap(conditionKeyName);
        Type type = value.GetType();
        // QQQ Does it need to convert IList if an implementation of IList?
        MethodInfo method = helpGettingCQMethod(cq, methodName, new Type[]{type}, propertyName);
        helpInvokingCQMethod(cq, method, new Object[]{value});
    }

    public void invokeOrderBy(String columnFlexibleName, bool isAsc) {
        assertStringNotNullAndNotTrimmedEmpty("columnFlexibleName", columnFlexibleName);
        PropertyNameCQContainer container = helpExtractingPropertyNameCQContainer(columnFlexibleName);
        String propertyName = container.getPropertyName();
        ConditionQuery cq = container.getConditionQuery();
        String ascDesc = isAsc ? "Asc" : "Desc";
        DBMeta dbmeta = DBMetaInstanceHandler.FindDBMeta(cq.getTableDbName());
        String columnCapPropName = initCap(dbmeta.FindPropertyName(propertyName));
        String methodName = "AddOrderBy_" + columnCapPropName + "_" + ascDesc;
        MethodInfo method = helpGettingCQMethod(cq, methodName, new Type[]{}, propertyName);
        helpInvokingCQMethod(cq, method, new Object[]{});
    }

    public ConditionQuery invokeForeignCQ(String foreignPropertyName) {
        assertStringNotNullAndNotTrimmedEmpty("foreignPropertyName", foreignPropertyName);
        String[] splitList = foreignPropertyName.Split('.');
        ConditionQuery foreignCQ = this;
        foreach (String elementName in splitList) {
            foreignCQ = doInvokeForeignCQ(foreignCQ, elementName);
        }
        return foreignCQ;
    }

    protected ConditionQuery doInvokeForeignCQ(ConditionQuery cq, String foreignPropertyName) {
        assertStringNotNullAndNotTrimmedEmpty("foreignPropertyName", foreignPropertyName);
        String methodName = "Query" + initCap(foreignPropertyName);
        MethodInfo method = helpGettingCQMethod(cq, methodName, new Type[]{}, foreignPropertyName);
        return (ConditionQuery)helpInvokingCQMethod(cq, method, new Object[]{});
    }

    public bool invokeHasForeignCQ(String foreignPropertyName) {
        assertStringNotNullAndNotTrimmedEmpty("foreignPropertyName", foreignPropertyName);
        String[] splitList = foreignPropertyName.Split('.');
        ConditionQuery foreignCQ = this;
        int splitLength = splitList.Length;
        int index = 0;
        foreach (String elementName in splitList) {
            if (!doInvokeHasForeignCQ(foreignCQ, elementName)) {
                return false;
            }
            if ((index + 1) < splitLength) { // not last loop
                foreignCQ = foreignCQ.invokeForeignCQ(elementName);
            }
            ++index;
        }
        return true;
    }

    protected bool doInvokeHasForeignCQ(ConditionQuery cq, String foreignPropertyName) {
        assertStringNotNullAndNotTrimmedEmpty("foreignPropertyName", foreignPropertyName);
        String methodName = "hasConditionQuery" + initCap(foreignPropertyName);
        MethodInfo method = helpGettingCQMethod(cq, methodName, new Type[]{}, foreignPropertyName);
        return (bool)helpInvokingCQMethod(cq, method, new Object[]{});
    }

    private PropertyNameCQContainer helpExtractingPropertyNameCQContainer(String name) {
        String[] strings = name.Split('.');
        int length = strings.Length;
        String propertyName = null;
        ConditionQuery cq = this;
        int index = 0;
        foreach (String element in strings) {
            if (length == (index+1)) { // at last loop!
                propertyName = element;
                break;
            }
            cq = cq.invokeForeignCQ(element);
            ++index;
        }
        return new PropertyNameCQContainer(propertyName, cq);
    }

    private class PropertyNameCQContainer {
        protected String _propertyName;
        protected ConditionQuery _cq;
        public PropertyNameCQContainer(String propertyName, ConditionQuery cq) {
            this._propertyName = propertyName;
            this._cq = cq;
        }
        public String getPropertyName() {
            return _propertyName;
        }
        public ConditionQuery getConditionQuery() {
            return _cq;
        }
    }

    private PropertyInfo helpGettingCQProperty(ConditionQuery cq, String propertyName) {
        PropertyInfo property = cq.GetType().GetProperty(propertyName);
        if (property == null) {
            String msg = "The property is not existing:";
            msg = msg + " propertyName=" + propertyName;
            msg = msg + " tableName=" + cq.getTableDbName();
            throw new IllegalStateException(msg);
        }
        return property;
    }

    private Object helpInvokingCQProperty(ConditionQuery cq, PropertyInfo property) {
        return property.GetValue(cq, null);
    }

    private MethodInfo helpGettingCQMethod(ConditionQuery cq, String methodName, Type[] argTypes, String property) {
        MethodInfo method = cq.GetType().GetMethod(methodName, argTypes);
        if (method == null) {
            method = cq.GetType().GetMethod(methodName);
            if (method == null) {
                String msg = "The method is not existing:";
                msg = msg + " methodName=" + methodName;
                msg = msg + " argTypes=" + convertObjectArrayToStringView(argTypes);
                msg = msg + " tableName=" + cq.getTableDbName();
                msg = msg + " property=" + property;
                throw new IllegalStateException(msg);
            }
        }
        return method;
    }

    private Object helpInvokingCQMethod(ConditionQuery cq, MethodInfo method, Object[] args) {
        return method.Invoke(cq, args);
    }

    // ===================================================================================
    //                                                                       Assist Helper
    //                                                                       =============
    protected String fRES(String value) {
        return filterRemoveEmptyString(value);
    }

    private String filterRemoveEmptyString(String value) {
        return ((value != null && !"".Equals(value)) ? value : null);
    }
    
    protected LikeSearchOption cLSOP() {
        return new LikeSearchOption().LikePrefix();
    }

    protected System.Collections.IList cTL<PROPERTY_TYPE>(System.Collections.Generic.ICollection<PROPERTY_TYPE> col) {
        return convertToList(col);
    }

    private System.Collections.IList convertToList<PROPERTY_TYPE>(System.Collections.Generic.ICollection<PROPERTY_TYPE> col) {
        if (col == null) {
            return null;
        }
		if (col is System.Collections.IList) {
		    return filterRemoveNullOrEmptyValueFromList((System.Collections.IList)col);
		}
		System.Collections.IList resultList = new System.Collections.ArrayList();
		foreach (PROPERTY_TYPE value in col) {
		    resultList.Add(value);
		}
        return filterRemoveNullOrEmptyValueFromList(resultList);
    }

    private System.Collections.IList filterRemoveNullOrEmptyValueFromList(System.Collections.IList ls) {
        if (ls == null) {
            return null;
        }
        System.Collections.IList newList = new System.Collections.ArrayList();
		foreach (Object element in ls) {
			if (element == null) {
			    continue;
			}
			if (element is String) {
                if ("".Equals((String)element)) {
                    continue;
                }
            }
		    newList.Add(element);
		}
        return newList;
    }

    // ===================================================================================
    //                                                                      General Helper
    //                                                                      ==============
    protected String ln() {
        return SimpleSystemUtil.GetLineSeparator();
    }

    protected String replaceString(String text, String fromText, String toText) {
	    return SimpleStringUtil.Replace(text, fromText, toText);
    }

    protected String initCap(String str) {
	    return SimpleStringUtil.InitCap(str);
    }

    protected String initUncap(String str) {
	    return SimpleStringUtil.InitUncap(str);
    }

    protected String convertObjectArrayToStringView(Object[] objArray) {
	    return TraceViewUtil.ConvertObjectArrayToStringView(objArray);
    }

    // -----------------------------------------------------
    //                                         Assert Object
    //                                         -------------
    protected void assertObjectNotNull(String variableName, Object value) {
	    SimpleAssertUtil.AssertObjectNotNull(variableName, value);
    }

    protected void assertColumnName(String columnName) {
        if (columnName == null) {
            String msg = "The columnName should not be null.";
            throw new IllegalArgumentException(msg);
        }
        if (columnName.Trim().Length == 0) {
            String msg = "The columnName should not be empty-string.";
            throw new IllegalArgumentException(msg);
        }
        if (columnName.IndexOf(",") >= 0) {
            String msg = "The columnName should not contain comma ',': " + columnName;
            throw new IllegalArgumentException(msg);
        }
    }

    protected void assertAliasName(String aliasName) {
        if (aliasName == null) {
            String msg = "The aliasName should not be null.";
            throw new IllegalArgumentException(msg);
        }
        if (aliasName.Trim().Length == 0) {
            String msg = "The aliasName should not be empty-string.";
            throw new IllegalArgumentException(msg);
        }
        if (aliasName.IndexOf(",") >= 0) {
            String msg = "The aliasName should not contain comma ',': " + aliasName;
            throw new IllegalArgumentException(msg);
        }
    }

    // -----------------------------------------------------
    //                                         Assert String
    //                                         -------------
    protected void assertStringNotNullAndNotTrimmedEmpty(String variableName, String value) {
	    SimpleAssertUtil.AssertStringNotNullAndNotTrimmedEmpty(variableName, value);
    }
	
    // ===================================================================================
    //                                                                      Basic Override
    //                                                                      ==============
    public override String ToString() {
        return GetType().Name + ":{aliasName=" + _aliasName + ", nestLevel=" + _nestLevel
		     + ", subQueryLevel=" + _subQueryLevel + ", foreignPropertyName=" + _foreignPropertyName
			 + ", relationPath=" + _relationPath + ", onClause=" + _onClause + "}";
    }
}

}
