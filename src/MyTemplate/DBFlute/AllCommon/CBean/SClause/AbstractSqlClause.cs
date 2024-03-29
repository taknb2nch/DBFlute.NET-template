
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CKey;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm.Info;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause {

// JavaLike
[System.Serializable]
public abstract class AbstractSqlClause : SqlClause {

    // ===================================================================================
    //                                                                          Definition
    //                                                                          ==========
    protected static readonly SelectClauseType DEFAULT_SELECT_CLAUSE_TYPE = SelectClauseType.COLUMNS;
    protected static readonly String SELECT_HINT = "/*$pmb.SelectHint*/ ";

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    // -----------------------------------------------------
    //                                                 Basic
    //                                                 -----
    /** The DB name of table. */
    protected readonly String _tableName;

    /** The DB meta of table. */
    protected DBMeta _dbmeta;

    /** The hierarchy level of sub-query. (NotMinus: if zero, not for sub-query) */
    protected int _subQueryLevel;

    // -----------------------------------------------------
    //                                       Clause Resource
    //                                       ---------------
    // /- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // The resources that are not often used to are lazy-loaded for performance.
    // - - - - - - - - - -/
    /** Selected select column map. map:{tableAliasName : map:{columnName : selectColumnInfo}} */
    protected Map<String, Map<String, SelectedSelectColumnInfo>> _selectedSelectColumnMap = new LinkedHashMap<String, Map<String, SelectedSelectColumnInfo>>();

    /** Specified select column map. map:{ tableAliasName = map:{ columnName : null } } (NullAllowed: This is lazy-loaded) */
    protected Map<String, Map<String, String>> _specifiedSelectColumnMap;

    /** Specified derive sub-query map. (NullAllowed: This is lazy-loaded) */
    protected Map<String, String> _specifiedDeriveSubQueryMap;

    /** The map of real column and alias of select clause. map:{realColumnName : aliasName} */
    protected Map<String, String> _selectClauseRealColumnAliasMap = new HashMap<String, String>(); // order no needed

    /** The type of select clause. (NotNull) */
    protected SelectClauseType _selectClauseType = DEFAULT_SELECT_CLAUSE_TYPE;

    /** The previous type of select clause. (NullAllowed: The default is null) */
    protected SelectClauseType _previousSelectClauseType;

    /** The map of select index. {key:columnName, value:selectIndex} (NullAllowed) */
    protected Map<String, int?> _selectIndexMap;

    /** The reverse map of select index. {key:selectIndex, value:columnName} (NullAllowed) */
    protected Map<String, String> _selectIndexReverseMap;

    /** Is use select index? Default value is true. */
    protected bool _useSelectIndex = true;

    /** Outer join map. */
    protected Map<String, LeftOuterJoinInfo> _outerJoinMap = new LinkedHashMap<String, LeftOuterJoinInfo>();

    /** Is inner-join effective? Default value is false. */
    protected bool _innerJoinEffective;

    /** The list of where clause. */
    protected List<String> _whereList = new ArrayList<String>();

    /** Inline where list for BaseTable. */
    protected List<String> _baseTableInlineWhereList = new ArrayList<String>();

    /** The clause of order-by. (NotNull) */
    protected OrderByClause _orderByClause = new OrderByClause();

    /** The list of union clause. (NullAllowed: This is lazy-loaded) */
    protected List<UnionQueryInfo> _unionQueryInfoList;

    /** Is order-by effective? Default value is false. */
    protected bool _orderByEffective = false;

    // -----------------------------------------------------
    //                                        Fetch Property
    //                                        --------------
    /** Fetch start index. (for fetchXxx()) */
    protected int _fetchStartIndex = 0;

    /** Fetch size. (for fetchXxx()) */
    protected int _fetchSize = 0;

    /** Fetch page number. (for fetchXxx()) This value should be plus. */
    protected int _fetchPageNumber = 1;

    /** Is fetch-narrowing effective? Default value is false. */
    protected bool _fetchScopeEffective = false;

    // -----------------------------------------------------
    //                                          OrScopeQuery
    //                                          ------------
    protected bool _orScopeQueryEffective;
    protected TmpOrScopeQueryInfo _currentTmpOrScopeQueryInfo;
    protected bool _orScopeQueryAndPart;

    // -----------------------------------------------------
    //                               WhereClauseSimpleFilter
    //                               -----------------------
    // /** The filter for where clause. */
    // protected List<WhereClauseSimpleFilter> _whereClauseSimpleFilterList;

    // -----------------------------------------------------
    //                                 Selected Foreign Info
    //                                 ---------------------
    /** The information of selected foreign table. */
    protected Map<String, String> _selectedForeignInfo;

    // -----------------------------------------------------
    //                                    Invalid Query Info
    //                                    ------------------
    /** Does it check an invalid query? */
    protected bool _checkInvalidQuery;

    /** The map of invalid query column. */
    protected Map<String, ConditionKey> _invalidQueryColumnMap;

    // ===================================================================================
    //                                                                         Constructor
    //                                                                         ===========
    public AbstractSqlClause(String tableName) {
        if (tableName == null) {
            String msg = "Argument[tableName] must not be null.";
            throw new IllegalArgumentException(msg);
        }
        _tableName = tableName;
    }

    // ===================================================================================
    //                                                                      SubQuery Level
    //                                                                      ==============
    public int getSubQueryLevel() {
        return _subQueryLevel;
    }

    public void setupForSubQuery(int subQueryLevel) {
        _subQueryLevel = subQueryLevel;
    }

    public bool isForSubQuery() {
        return _subQueryLevel > 0;
    }

    // ===================================================================================
    //                                                                         Main Clause
    //                                                                         ===========
    // -----------------------------------------------------
    //                                       Complete Clause
    //                                       ---------------
    public String getClause() {
        StringBuilder sb = new StringBuilder(512);
        String selectClause = getSelectClause();
        sb.append(selectClause);
        sb.append(" ");
        buildClauseWithoutMainSelect(sb, selectClause);
        String sql = sb.toString();
        sql = filterEnclosingClause(sql);
        sql = filterSubQueryIndent(sql);
        return sql;
    }

    protected void buildClauseWithoutMainSelect(StringBuilder sb, String selectClause) {
        buildFromClause(sb);
        sb.append(getFromHint());
        sb.append(" ");
        buildWhereClause(sb);
        String unionClause = prepareUnionClause(selectClause);
        unionClause = deleteUnionWhereTemplateMark(unionClause); // required
        sb.append(unionClause);
        if (!needsUnionNormalSelectEnclosing()) {
            sb.append(prepareClauseOrderBy());
            sb.append(prepareClauseSqlSuffix());
        }
    }

    protected virtual String deleteUnionWhereTemplateMark(String unionClause) {
        if (unionClause != null && unionClause.Trim().Length > 0) {
            unionClause = replaceString(unionClause, getUnionWhereClauseMark(), "");
            unionClause = replaceString(unionClause, getUnionWhereFirstConditionMark(), "");
        }
        return unionClause;
    }

    // -----------------------------------------------------
    //                                       Fragment Clause
    //                                       ---------------
    public String getClauseFromWhereWithUnionTemplate() {
        return buildClauseFromWhereAsTemplate(false);
    }

    public String getClauseFromWhereWithWhereUnionTemplate() {
        return buildClauseFromWhereAsTemplate(true);
    }

    protected virtual String buildClauseFromWhereAsTemplate(bool template) {
        StringBuilder sb = new StringBuilder(256);
        buildFromClause(sb);
        sb.append(getFromHint());
        sb.append(" ");
        buildWhereClause(sb, template);
        sb.append(prepareUnionClause(getUnionSelectClauseMark()));
        return sb.toString();
    }

    protected virtual String prepareUnionClause(String selectClause) {
        if (!hasUnionQuery()) {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        for (Iterator<UnionQueryInfo> ite = _unionQueryInfoList.iterator(); ite.hasNext(); ) {
            UnionQueryInfo unionQueryInfo = (UnionQueryInfo)ite.next();
            String unionQueryClause = unionQueryInfo.getUnionQueryClause();
            bool unionAll = unionQueryInfo.isUnionAll();
            sb.append(ln());
            sb.append(unionAll ? " union all " : " union ");
            sb.append(ln());
            sb.append(selectClause).append(" ").append(unionQueryClause);
        }
        return sb.toString();
    }

    protected String prepareClauseOrderBy() {
        if (!_orderByEffective || _orderByClause.isEmpty()) {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        sb.append(" ");
        sb.append(getOrderByClause());
        return sb.toString();
    }

    protected String prepareClauseSqlSuffix() {
        String sqlSuffix = getSqlSuffix();
        if (sqlSuffix == null || sqlSuffix.Trim().Length == 0) {
            return "";
        }
        StringBuilder sb = new StringBuilder();
        sb.append(" ");
        sb.append(sqlSuffix);
        return sb.toString();
    }

    protected String filterEnclosingClause(String sql) {
        sql = filterUnionNormalSelectEnclosing(sql);
        sql = filterUnionCountOrScalarEnclosing(sql);
        return sql;
    }

    protected virtual String filterUnionNormalSelectEnclosing(String sql) {
        if (!needsUnionNormalSelectEnclosing()) {
            return sql;
        }
        String selectClause = "select" + SELECT_HINT + " *";
        String ln = this.ln();
        String beginMark = resolveSubQueryBeginMark("dfmain") + ln;
        String endMark = resolveSubQueryEndMark("dfmain");
        String clause = selectClause + ln + "  from (" + beginMark + sql + ln + "       ) dfmain" + endMark;
        clause = clause + prepareClauseOrderBy() + prepareClauseSqlSuffix();
        return clause;
    }

    protected virtual String filterUnionCountOrScalarEnclosing(String sql) {
        if (!needsUnionCountOrScalarEnclosing()) {
            return sql;
        }
        String selectClause = buildSelectClauseCountOrScalar("dfmain");
        String ln = this.ln();
        String beginMark = resolveSubQueryBeginMark("dfmain") + ln;
        String endMark = resolveSubQueryEndMark("dfmain");
        return selectClause + ln + "  from (" + beginMark + sql + ln + "       ) dfmain" + endMark;
    }

    protected virtual bool needsUnionNormalSelectEnclosing() {
        if (!isUnionNormalSelectEnclosingRequired()) {
            return false;
        }
        return hasUnionQuery() && !isSelectClauseTypeCountOrScalar();
    }

    protected virtual bool isUnionNormalSelectEnclosingRequired() { // for extension
        return false; // false as default
    }

    protected virtual bool needsUnionCountOrScalarEnclosing() {
        return hasUnionQuery() && isSelectClauseTypeCountOrScalar();
    }

    // ===================================================================================
    //                                                                        Clause Parts
    //                                                                        ============
    // -----------------------------------------------------
    //                                         Select Clause
    //                                         -------------
    public String getSelectClause() {
        // [DBFlute-0.8.6]
        if (isSelectClauseTypeCountOrScalar() && !hasUnionQuery()) {
            return buildSelectClauseCountOrScalar(getBasePointAliasName());
        }
        // /- - - - - - - - - - - - - - - - - - - - - - - - 
        // The type of select clause is COLUMNS since here.
        // - - - - - - - - - -/
        StringBuilder sb = new StringBuilder();
        DBMeta dbmeta = getDBMeta();
        List<ColumnInfo> columnInfoList = dbmeta.ColumnInfoList;

        // [DBFlute-0.7.4]
        Map<String, String> localSpecifiedMap = null;
        if (_specifiedSelectColumnMap != null) {
            localSpecifiedMap = _specifiedSelectColumnMap.get(getBasePointAliasName());
        }
        bool existsSpecifiedLocal = localSpecifiedMap != null && !localSpecifiedMap.isEmpty();

        int selectIndex = -1; // because 0 origin in ADO.NET
        if (_useSelectIndex) {
            _selectIndexMap = createSelectIndexMap();
        }

        // Columns of local table.
        bool needsDelimiter = false;
        foreach (ColumnInfo columnInfo in columnInfoList) {
            String columnDbName = columnInfo.ColumnDbName;
            String columnSqlName = columnInfo.ColumnSqlName;
            
            // [DBFlute-0.7.4]
            if (existsSpecifiedLocal && !localSpecifiedMap.containsKey(columnDbName)) {
                if (isSelectClauseTypeCountOrScalar() && hasUnionQuery()) {
                    // Here it must be with union query.
                    // So the primary Key is target for saving unique.
                    // But if it does not have primary keys, all column is target.
                    if (dbmeta.HasPrimaryKey) {
                        if (!columnInfo.IsPrimary) {
                            continue;
                        }
                    }
                } else {
                    continue;
                }
            }

            if (needsDelimiter) {
                sb.append(", ");
            } else {
                sb.append("select");
                appendSelectHint(sb);
                sb.append(" ");
                needsDelimiter = true;
            }
            String realColumnName = getBasePointAliasName() + "." + columnSqlName;
            String onQueryName;
            ++selectIndex;
            if (_useSelectIndex) {
                _selectIndexMap.put(columnDbName.ToLower(), selectIndex); // lower because of case sensitive map
                onQueryName = buildSelectIndexAliasName(selectIndex);
            } else {
                onQueryName = columnSqlName;
            }
            sb.append(realColumnName).append(" as ").append(onQueryName);
            _selectClauseRealColumnAliasMap.put(realColumnName, onQueryName);
        }
        
        // Columns of foreign tables.
        Set<String> tableAliasNameSet = _selectedSelectColumnMap.keySet();
        foreach (String tableAliasName in tableAliasNameSet) {
            Map<String, SelectedSelectColumnInfo> map = _selectedSelectColumnMap.get(tableAliasName);
            Collection<SelectedSelectColumnInfo> selectColumnInfoList = map.values();
            Map<String, String> foreginSpecifiedMap = null;
            if (_specifiedSelectColumnMap != null) {
                foreginSpecifiedMap = _specifiedSelectColumnMap.get(tableAliasName);
            }
            bool existsSpecifiedForeign = foreginSpecifiedMap != null && !foreginSpecifiedMap.isEmpty();
            bool finishedForeignIndent = false;
            foreach (SelectedSelectColumnInfo selectColumnInfo in selectColumnInfoList) {
                if (existsSpecifiedForeign && !foreginSpecifiedMap.containsKey(selectColumnInfo.getColumnDbName())) {
                    continue;
                }

                String realColumnName = selectColumnInfo.buildRealColumnName();
                String columnAliasName = selectColumnInfo.getColumnAliasName();
                String onQueryName;
                ++selectIndex;
                if (_useSelectIndex) {
                    _selectIndexMap.put(columnAliasName.ToLower(), selectIndex); // lower because of case sensitive map
                    onQueryName = buildSelectIndexAliasName(selectIndex);
                } else {
                    onQueryName = columnAliasName;
                }
                if (!finishedForeignIndent) {
                    sb.append(ln()).append("     ");
                    finishedForeignIndent = true;
                }
                sb.append(", ").append(realColumnName).append(" as ").append(onQueryName);
                _selectClauseRealColumnAliasMap.put(realColumnName, onQueryName);
            }
        }

        // [DBFlute-0.7.4]
        if (_specifiedDeriveSubQueryMap != null && !_specifiedDeriveSubQueryMap.isEmpty()) {
            Collection<String> deriveSubQuerySet = _specifiedDeriveSubQueryMap.values();
            foreach (String deriveSubQuery in deriveSubQuerySet) {
                sb.append(ln()).append("     ");
                sb.append(", ").append(deriveSubQuery);
                
                // [DBFlute-0.8.3]
                int beginIndex = deriveSubQuery.LastIndexOf(" as ");
                if (beginIndex >= 0) { // basically true
                    String aliasName = deriveSubQuery.Substring(beginIndex + " as ".Length);
                    int endIndex = aliasName.IndexOf("--df:");
                    if (endIndex >= 0) { // basically true
                        aliasName = aliasName.Substring(0, endIndex);
                    }
                    // for SpecifiedDerivedOrderBy
                    _selectClauseRealColumnAliasMap.put(aliasName, aliasName);
                }
            }
        }

        return sb.toString();
    }

    // -----------------------------------------------------
    //                                       Count or Scalar
    //                                       ---------------
    protected bool isSelectClauseTypeCountOrScalar() {
        if (_selectClauseType.equals(SelectClauseType.COUNT)) {
            return true;
        } else if (_selectClauseType.equals(SelectClauseType.MAX)) {
            return true;
        } else if (_selectClauseType.equals(SelectClauseType.MIN)) {
            return true;
        } else if (_selectClauseType.equals(SelectClauseType.SUM)) {
            return true;
        } else if (_selectClauseType.equals(SelectClauseType.AVG)) {
            return true;
        }
        return false;
    }

    protected String buildSelectClauseCountOrScalar(String aliasName) {
        if (_selectClauseType.equals(SelectClauseType.COUNT)) {
            return buildSelectClauseCount();
        } else if (_selectClauseType.equals(SelectClauseType.MAX)) {
            return buildSelectClauseMax(aliasName);
        } else if (_selectClauseType.equals(SelectClauseType.MIN)) {
            return buildSelectClauseMin(aliasName);
        } else if (_selectClauseType.equals(SelectClauseType.SUM)) {
            return buildSelectClauseSum(aliasName);
        } else if (_selectClauseType.equals(SelectClauseType.AVG)) {
            return buildSelectClauseAvg(aliasName);
        }
        String msg = "The type of select clause is not for scalar:";
        msg = msg + " type=" + _selectClauseType;
        throw new IllegalStateException(msg);
    }

    protected String buildSelectClauseCount() {
        return "select count(*)";
    }

    protected String buildSelectClauseMax(String aliasName) {
        String columnName = getSpecifiedColumnNameAsOne();
        assertScalarSelectSpecifiedColumnOnlyOne(columnName);
        return "select max(" + aliasName + "." + columnName  + ")";
    }

    protected String buildSelectClauseMin(String aliasName) {
        String columnName = getSpecifiedColumnNameAsOne();
        assertScalarSelectSpecifiedColumnOnlyOne(columnName);
        return "select min(" + aliasName + "." + columnName  + ")";
    }

    protected String buildSelectClauseSum(String aliasName) {
        String columnName = getSpecifiedColumnNameAsOne();
        assertScalarSelectSpecifiedColumnOnlyOne(columnName);
        return "select sum(" + aliasName + "." + columnName  + ")";
    }

    protected String buildSelectClauseAvg(String aliasName) {
        String columnName = getSpecifiedColumnNameAsOne();
        assertScalarSelectSpecifiedColumnOnlyOne(columnName);
        return "select avg(" + aliasName + "." + columnName  + ")";
    }

    protected void assertScalarSelectSpecifiedColumnOnlyOne(String columnName) {
        if (columnName != null) {
            return;
        }
        String msg = "The specified column exists one";
        msg = msg + " when the type of select clause is for scalar:";
        msg = msg + " specifiedSelectColumnMap=" + _specifiedSelectColumnMap;
        throw new IllegalStateException(msg);
    }

    // -----------------------------------------------------
    //                                          Select Index
    //                                          ------------
    public Map<String, int?> getSelectIndexMap() {
        return _selectIndexMap;
    }

    protected Map<String, int?> createSelectIndexMap() {
        // must be case insensitive because it treats result set column
        // (and it does not need to be ordered)
        // but it uses lower-case keys because of no case insensitive (java-like) map in DBFlute.NET
        return new HashMap<String, int?>();
    }

    public Map<String, String> getSelectIndexReverseMap() {
        if (_selectIndexReverseMap != null) {
            return _selectIndexReverseMap;
        }
        if (_selectIndexMap == null) {
            return null;
        }
        _selectIndexReverseMap = new HashMap<String, String>(); // same style as select index map
        foreach (String columnName in _selectIndexMap.keySet()) {
            int? selectIndex = _selectIndexMap.get(columnName);
            _selectIndexReverseMap.put(buildSelectIndexAliasName(selectIndex), columnName);
        }
        return _selectIndexReverseMap;
    }

    public void disableSelectIndex() {
        _useSelectIndex = false;
    }

    protected String buildSelectIndexAliasName(int? selectIndex) {
        return "c" + selectIndex;
    }

    // -----------------------------------------------------
    //                                           Select Hint
    //                                           -----------
    public String getSelectHint() {
        return createSelectHint();
    }

    protected virtual void appendSelectHint(StringBuilder sb) { // for extension
        sb.append(SELECT_HINT);
    }

    // -----------------------------------------------------
    //                                           From Clause
    //                                           -----------
    public String getFromClause() {
        StringBuilder sb = new StringBuilder();
        buildFromClause(sb);
        return sb.toString();
    }

    public void buildFromClause(StringBuilder sb) {
        sb.append(ln()).append("  ");
        sb.append("from ");
        if (isJoinInParentheses()) {
            for (int i = 0; i < _outerJoinMap.size(); i++) {
                sb.append("(");
            }
        }
        String tableSqlName = getDBMeta().TableSqlName;
        if (_baseTableInlineWhereList.isEmpty()) {
            sb.append(tableSqlName).append(" ").append(getBasePointAliasName());
        } else {
            sb.append(getInlineViewClause(tableSqlName, _baseTableInlineWhereList)).append(" ").append(getBasePointAliasName());
        }
        sb.append(getFromBaseTableHint());
        sb.append(getLeftOuterJoinClause());
    }

    protected String getLeftOuterJoinClause() {
        StringBuilder sb = new StringBuilder();
        for (Iterator<String> ite = _outerJoinMap.keySet().iterator(); ite.hasNext(); ) {
            String foreignAliasName = ite.next();
            LeftOuterJoinInfo joinInfo = (LeftOuterJoinInfo)_outerJoinMap.get(foreignAliasName);
            String foreignTableDbName = joinInfo.getForeignTableDbName();
            Map<String, String> joinOnMap = joinInfo.getJoinOnMap();
            assertJoinOnMapNotEmpty(joinOnMap, foreignAliasName);

            sb.append(ln()).append("   ");
            if (joinInfo.isInnerJoin()) {
                sb.append(" inner join ");
            } else {
                sb.append(" left outer join "); // is main!
            }
            DBMeta foreignDBMeta = findDBMeta(foreignTableDbName);
            String foreignTableSqlName = foreignDBMeta.TableSqlName;
            List<String> inlineWhereClauseList = joinInfo.getInlineWhereClauseList();
            String tableExp;
            if (inlineWhereClauseList.isEmpty()) {
                tableExp = foreignTableSqlName;
            } else {
                tableExp = getInlineViewClause(foreignTableSqlName, inlineWhereClauseList);
            }
            if (joinInfo.hasFixedCondition()) {
                sb.append(joinInfo.resolveFixedInlineView(tableExp));
            } else {
                sb.append(tableExp);
            }
            sb.append(" ").append(foreignAliasName);
            if (joinInfo.hasInlineOrOnClause() || joinInfo.hasFixedCondition()) {
                sb.append(ln()).append("     "); // only when additional conditions exist
            }
            sb.append(" on ");
            int count = 0;
            Set<String> localColumnNameSet = joinOnMap.keySet();
            foreach (String localColumnName in localColumnNameSet) {
                String foreignColumnName = (String)joinOnMap.get(localColumnName);
                if (count > 0) {
                    sb.append(" and ");
                }
                sb.append(localColumnName).append(" = ").append(foreignColumnName);
                ++count;
            }
            if (joinInfo.hasFixedCondition()) {
                String fixedCondition = joinInfo.getFixedCondition();
                sb.append(ln()).append("    ");
                sb.append(" and ").append(fixedCondition);
            }
            List<String> additionalOnClauseList = joinInfo.getAdditionalOnClauseList();
            foreach (String additionalOnClause in additionalOnClauseList) {
                sb.append(ln()).append("    ");
                sb.append(" and ").append(additionalOnClause);
            }
            if (isJoinInParentheses()) {
                sb.append(")");
            }
        }
        return sb.toString();
    }

    protected virtual bool isJoinInParentheses() { // for DBMS that needs to join in parentheses
        return false; // as default
    }

    protected String getInlineViewClause(String foreignTableDbName, List<String> inlineWhereClauseList) {
        StringBuilder sb = new StringBuilder();
        sb.append("(select * from ").append(foreignTableDbName).append(" where ");
        int count = 0;
        for (Iterator<String> ite = inlineWhereClauseList.iterator(); ite.hasNext(); ) {
            String clauseElement = ite.next();
            // clauseElement = filterWhereClauseSimply(clauseElement); *Non Support on C#
            if (count > 0) {
                sb.append(" and ");
            }
            sb.append(clauseElement);
            ++count;
        }
        sb.append(")");
        return sb.toString();
    }

    public String getFromBaseTableHint() {
        return createFromBaseTableHint();
    }

    // -----------------------------------------------------
    //                                             From Hint
    //                                             ---------
    public String getFromHint() {
        return createFromHint();
    }

    // -----------------------------------------------------
    //                                          Where Clause
    //                                          ------------
    public String getWhereClause() {
        StringBuilder sb = new StringBuilder();
        buildWhereClause(sb);
        return sb.toString();
    }

    protected void buildWhereClause(StringBuilder sb) {
        buildWhereClause(sb, false);
    }

    protected void buildWhereClause(StringBuilder sb, bool template) {
        if (_whereList.isEmpty()) {
            if (template) {
                sb.append(getWhereClauseMark());
            }
            return;
        }
        int count = 0;
        for (Iterator<String> ite = _whereList.iterator(); ite.hasNext(); count++) {
            String clauseElement = (String)ite.next();
            // clauseElement = filterWhereClauseSimply(clauseElement); *Non Support on C#
            if (count == 0) {
                sb.append(ln()).append(" ");
                sb.append("where ").append(template  ? getWhereFirstConditionMark() : "").append(clauseElement);
            } else {
                sb.append(ln()).append("  ");
                sb.append(" and ").append(clauseElement);
            }
        }
    }

    // -----------------------------------------------------
    //                                        OrderBy Clause
    //                                        --------------
    public String getOrderByClause() {
        String orderByClause = null;
        if (hasUnionQuery()) {
            if (_selectClauseRealColumnAliasMap == null || _selectClauseRealColumnAliasMap.isEmpty()) {
                String msg = "The selectClauseColumnAliasMap should not be null or empty when union query exists.";
                throw new IllegalStateException(msg);
            }
            orderByClause = _orderByClause.getOrderByClause(_selectClauseRealColumnAliasMap);
        } else {
            orderByClause = _orderByClause.getOrderByClause();
        }
        if (orderByClause != null && orderByClause.Trim().Length > 0) {
            return ln() + " " + orderByClause;
        } else {
            return orderByClause;
        }
    }

    // -----------------------------------------------------
    //                                            SQL Suffix
    //                                            ----------
    public String getSqlSuffix() {
        String sqlSuffix = createSqlSuffix();
        if (sqlSuffix != null && sqlSuffix.Trim().Length > 0) {
            return ln() + sqlSuffix;
        } else {
            return sqlSuffix;
        }
    }

    // ===================================================================================
    //                                                                SelectedSelectColumn
    //                                                                ====================
    public void registerSelectedSelectColumn(String foreignTableAliasName
                                           , String localTableName
                                           , String foreignPropertyName
                                           , String localRelationPath) {
        _selectedSelectColumnMap.put(foreignTableAliasName, createSelectedSelectColumnInfo(foreignTableAliasName, localTableName, foreignPropertyName, localRelationPath));
    }
    
    protected Map<String, SelectedSelectColumnInfo> createSelectedSelectColumnInfo(String foreignTableAliasName
                                                                                 , String localTableName
                                                                                 , String foreignPropertyName
                                                                                 , String localRelationPath) {
        DBMeta dbmeta = findDBMeta(localTableName);
        ForeignInfo foreignInfo = dbmeta.FindForeignInfo(foreignPropertyName);
        int relationNo = foreignInfo.RelationNo;
        String nextRelationPath = "_" + relationNo;
        if (localRelationPath != null) {
            nextRelationPath = localRelationPath + nextRelationPath;
        }
        Map<String, SelectedSelectColumnInfo> resultMap = new LinkedHashMap<String, SelectedSelectColumnInfo>();
        DBMeta foreignDBMeta = foreignInfo.ForeignDBMeta;
        foreach (ColumnInfo columnInfo in foreignDBMeta.ColumnInfoList) {
            String columnDbName = columnInfo.ColumnDbName;
            String columnSqlName = columnInfo.ColumnSqlName;
            SelectedSelectColumnInfo selectColumnInfo = new SelectedSelectColumnInfo();
            selectColumnInfo.setTableAliasName(foreignTableAliasName);
            selectColumnInfo.setColumnDbName(columnDbName);
            selectColumnInfo.setColumnSqlName(columnSqlName);
            selectColumnInfo.setColumnAliasName(columnDbName + nextRelationPath);
            resultMap.put(columnDbName, selectColumnInfo);
        }
        return resultMap;
    }

    public class SelectedSelectColumnInfo {
        protected String tableAliasName;
        protected String columnDbName;
        protected String columnSqlName;
        protected String columnAliasName;
        public String buildRealColumnName() {
            if (tableAliasName != null) {
                return tableAliasName + "." + columnSqlName;
            } else {
                return columnSqlName;
            }
        }
        public String getTableAliasName() {
            return tableAliasName;
        }
        public void setTableAliasName(String tableAliasName) {
            this.tableAliasName = tableAliasName;
        }
        public String getColumnDbName() {
            return columnDbName;
        }
        public void setColumnDbName(String columnDbName) {
            this.columnDbName = columnDbName;
        }
        public String getColumnSqlName() {
            return columnSqlName;
        }
        public void setColumnSqlName(String columnSqlName) {
            this.columnSqlName = columnSqlName;
        }
        public String getColumnAliasName() {
            return columnAliasName;
        }
        public void setColumnAliasName(String columnAliasName) {
            this.columnAliasName = columnAliasName;
        }
    }

    // ===================================================================================
    //                                                                           OuterJoin
    //                                                                           =========
    public virtual void registerOuterJoin(String baseTableDbName, String foreignTableDbName, String aliasName,
            Map<String, String> joinOnMap, String fixedCondition, FixedConditionResolver fixedConditionResolver) {
        assertAlreadyOuterJoin(aliasName);
        assertJoinOnMapNotEmpty(joinOnMap, aliasName);
        LeftOuterJoinInfo joinInfo = new LeftOuterJoinInfo();
        joinInfo.setAliasName(aliasName);
        joinInfo.setLocalTableDbName(baseTableDbName);
        joinInfo.setForeignTableDbName(foreignTableDbName);
        joinInfo.setJoinOnMap(joinOnMap);
        joinInfo.setFixedCondition(fixedCondition);
        joinInfo.setFixedConditionResolver(fixedConditionResolver);
        if (_innerJoinEffective) { // basically false
            joinInfo.setInnerJoin(true);
        }

        // it should be resolved before registration because
        // the process may have Query(Relation) as precondition
        joinInfo.resolveFixedCondition();

        _outerJoinMap.put(aliasName, joinInfo);
    }

    public void changeToInnerJoin(String aliasName) {
        LeftOuterJoinInfo joinInfo = _outerJoinMap.get(aliasName);
        if (joinInfo == null) {
            String msg = "The aliasName should be registered:";
            msg = msg + " aliasName=" + aliasName + " outerJoinMap=" + _outerJoinMap.keySet();
            throw new IllegalStateException(msg);
        }
        joinInfo.setInnerJoin(true);
    }

    public SqlClause makeInnerJoinEffective() {
        _innerJoinEffective = true;
        return this;
    }

    public SqlClause backToOuterJoin() {
        _innerJoinEffective = false;
        return this;
    }

    protected class LeftOuterJoinInfo {
        protected String _aliasName;
        protected String _localTableDbName;
        protected String _foreignTableDbName;
        protected List<String> _inlineWhereClauseList = new ArrayList<String>();
        protected List<String> _additionalOnClauseList = new ArrayList<String>();
        protected Map<String, String> _joinOnMap;
        protected String _fixedCondition;
        protected FixedConditionResolver _fixedConditionResolver;
        protected bool _innerJoin;
        public bool hasInlineOrOnClause() {
            return !_inlineWhereClauseList.isEmpty() || !_additionalOnClauseList.isEmpty();
        }
        public bool hasFixedCondition() {
            return _fixedCondition != null && _fixedCondition.Trim().Length > 0;
        }
        public void resolveFixedCondition() { // required before using fixed-condition
            if (hasFixedCondition() && _fixedConditionResolver != null) {
                _fixedCondition = _fixedConditionResolver.resolveVariable(_fixedCondition);
            }
        }
        public String resolveFixedInlineView(String foreignTableSqlName) {
            if (hasFixedCondition() && _fixedConditionResolver != null) {
                return _fixedConditionResolver.resolveFixedInlineView(foreignTableSqlName);
            }
            return foreignTableSqlName;
        }
        public String getAliasName() {
            return _aliasName;
        }
        public void setAliasName(String value) {
            _aliasName = value;
        }
        public String getLocalTableDbName() {
            return _localTableDbName;
        }
        public void setLocalTableDbName(String localTableDbName) {
            _localTableDbName = localTableDbName;
        }
        public String getForeignTableDbName() {
            return _foreignTableDbName;
        }
        public void setForeignTableDbName(String foreignTableDbName) {
            _foreignTableDbName = foreignTableDbName;
        }
        public List<String> getInlineWhereClauseList() {
            return _inlineWhereClauseList;
        }
        public void addInlineWhereClause(String value) {
            _inlineWhereClauseList.add(value);
        }
        public List<String> getAdditionalOnClauseList() {
            return _additionalOnClauseList;
        }
        public void addAdditionalOnClause(String value) {
            _additionalOnClauseList.add(value);
        }
        public Map<String, String> getJoinOnMap() {
            return _joinOnMap;
        }
        public void setJoinOnMap(Map<String, String> value) {
            _joinOnMap = value;
        }
        public String getFixedCondition() {
            return _fixedCondition;
        }
        public void setFixedCondition(String fixedCondition) {
            _fixedCondition = fixedCondition;
        }
        public FixedConditionResolver getFixedConditionResolver() {
            return _fixedConditionResolver;
        }
        public void setFixedConditionResolver(FixedConditionResolver fixedConditionResolver) {
            _fixedConditionResolver = fixedConditionResolver;
        }
        public bool isInnerJoin() {
            return _innerJoin;
        }
        public void setInnerJoin(bool value) {
            _innerJoin = value;
        }
    }

    protected void assertAlreadyOuterJoin(String aliasName) {
        if (_outerJoinMap.containsKey(aliasName)) {
            String msg = "The alias name have already registered in outer join: " + aliasName;
            throw new IllegalStateException(msg);
        }
    }

    protected void assertJoinOnMapNotEmpty(Map<String, String> joinOnMap, String aliasName) {
        if (joinOnMap.isEmpty()) {
            String msg = "The joinOnMap should not be empty: aliasName=" + aliasName;
            throw new IllegalStateException(msg);
        }
    }

    // ===================================================================================
    //                                                                               Where
    //                                                                               =====
    public void registerWhereClause(String columnFullName, ConditionKey key, ConditionValue value) {
        assertStringNotNullAndNotTrimmedEmpty("columnFullName", columnFullName);
        List<String> clauseList = getWhereClauseList4Register();
        doRegisterWhereClause(clauseList, columnFullName, key, value);
    }

    public void registerWhereClause(String columnFullName, ConditionKey key, ConditionValue value, ConditionOption option) {
        assertStringNotNullAndNotTrimmedEmpty("columnFullName", columnFullName);
        assertObjectNotNull("option of " + columnFullName, option);
        List<String> clauseList = getWhereClauseList4Register();
        doRegisterWhereClause(clauseList, columnFullName, key, value, option);
    }

    public void registerWhereClause(String clause) {
        assertStringNotNullAndNotTrimmedEmpty("clause", clause);
        List<String> clauseList = getWhereClauseList4Register();
        doRegisterWhereClause(clauseList, clause);
    }

    protected void doRegisterWhereClause(List<String> clauseList, String columnName, ConditionKey key,
            ConditionValue value) {
        key.addWhereClause(clauseList, columnName, value);
        markOrScopeQueryAndPart(clauseList);
    }

    protected void doRegisterWhereClause(List<String> clauseList, String columnName, ConditionKey key,
            ConditionValue value, ConditionOption option) {
        key.addWhereClause(clauseList, columnName, value, option);
        markOrScopeQueryAndPart(clauseList);
    }

    protected void doRegisterWhereClause(List<String> clauseList, String clause) {
        clauseList.add(clause);
        markOrScopeQueryAndPart(clauseList);
    }

    protected List<String> getWhereClauseList4Register() {
        if (_orScopeQueryEffective) {
            return getTmpOrWhereList();
        } else {
            return _whereList;
        }
    }

    public void exchangeFirstWhereClauseForLastOne() {
        if (_whereList.size() > 1) {
             String first = (String)_whereList.get(0);
             String last = (String)_whereList.get(_whereList.size() - 1);
            _whereList.set(0, last);
            _whereList.set(_whereList.size() - 1, first);
        }
    }

    public bool hasWhereClause() {
        return _whereList != null && _whereList.size() > 0;
    }

    // ===================================================================================
    //                                                                         InlineWhere
    //                                                                         ===========
    public void registerBaseTableInlineWhereClause(String columnName, ConditionKey key, ConditionValue value) {
        assertStringNotNullAndNotTrimmedEmpty("columnName", columnName);
        List<String> clauseList = getBaseTableInlineWhereClauseList4Register();
        doRegisterWhereClause(clauseList, columnName, key, value);
    }

    public void registerBaseTableInlineWhereClause(String columnName, ConditionKey key, ConditionValue value, ConditionOption option) {
        assertStringNotNullAndNotTrimmedEmpty("columnName", columnName);
        assertObjectNotNull("option of " + columnName, option);
        List<String> clauseList = getBaseTableInlineWhereClauseList4Register();
        doRegisterWhereClause(clauseList, columnName, key, value, option);
    }

    public void registerBaseTableInlineWhereClause(String value) {
        getBaseTableInlineWhereClauseList4Register().add(value);
    }

    protected List<String> getBaseTableInlineWhereClauseList4Register() {
        if (_orScopeQueryEffective) {
            return getTmpOrBaseTableInlineWhereList();
        } else {
            return _baseTableInlineWhereList;
        }
    }

    public void registerOuterJoinInlineWhereClause(String aliasName, String columnName, ConditionKey key, ConditionValue value, bool onClause) {
        assertNotYetOuterJoin(aliasName);
        assertStringNotNullAndNotTrimmedEmpty("columnName", columnName);
        List<String> clauseList = getOuterJoinInlineWhereClauseList4Register(aliasName, onClause);
        String realColumnName = (onClause ? aliasName + "." : "") + columnName;
        doRegisterWhereClause(clauseList, realColumnName, key, value);
    }

    public void registerOuterJoinInlineWhereClause(String aliasName, String columnName, ConditionKey key, ConditionValue value, ConditionOption option, bool onClause) {
        assertNotYetOuterJoin(aliasName);
        assertStringNotNullAndNotTrimmedEmpty("columnName", columnName);
        List<String> clauseList = getOuterJoinInlineWhereClauseList4Register(aliasName, onClause);
        String realColumnName = (onClause ? aliasName + "." : "") + columnName;
        doRegisterWhereClause(clauseList, realColumnName, key, value, option);
    }

    public void registerOuterJoinInlineWhereClause(String aliasName, String clause, bool onClause) {
        assertNotYetOuterJoin(aliasName);
        List<String> clauseList = getOuterJoinInlineWhereClauseList4Register(aliasName, onClause);
        doRegisterWhereClause(clauseList, clause);
    }

    protected List<String> getOuterJoinInlineWhereClauseList4Register(String aliasName, bool onClause) {
        LeftOuterJoinInfo joinInfo = _outerJoinMap.get(aliasName);
        List<String> clauseList;
        if (onClause) {
            if (_orScopeQueryEffective) {
                clauseList = getTmpOrAdditionalOnClauseList(aliasName);
            } else {
                clauseList = joinInfo.getAdditionalOnClauseList();
            }
        } else {
            if (_orScopeQueryEffective) {
                clauseList = getTmpOrOuterJoinInlineClauseList(aliasName);
            } else {
                clauseList = joinInfo.getInlineWhereClauseList();
            }
        }
        return clauseList;
    }

    protected void assertNotYetOuterJoin(String aliasName) {
        if (!_outerJoinMap.containsKey(aliasName)) {
            String msg = "The alias name have not registered in outer join yet: " + aliasName;
            throw new IllegalStateException(msg);
        }
    }

    // ===================================================================================
    //                                                                        OrScopeQuery
    //                                                                        ============
    public void makeOrScopeQueryEffective() {
        TmpOrScopeQueryInfo tmpOrScopeQueryInfo = new TmpOrScopeQueryInfo();
        if (_currentTmpOrScopeQueryInfo != null) {
            _currentTmpOrScopeQueryInfo.addChildInfo(tmpOrScopeQueryInfo);
        }
        _currentTmpOrScopeQueryInfo = tmpOrScopeQueryInfo;
        _orScopeQueryEffective = true;
    }

    public void closeOrScopeQuery() {
        assertCurrentTmpOrScopeQueryInfo();
        TmpOrScopeQueryInfo parentInfo = _currentTmpOrScopeQueryInfo.getParentInfo();
        if (parentInfo != null) {
            _currentTmpOrScopeQueryInfo = parentInfo;
        } else {
            reflectTmpOrClauseToRealObject(_currentTmpOrScopeQueryInfo);
            clearOrScopeQuery();
        }
    }

    protected void clearOrScopeQuery() {
        _currentTmpOrScopeQueryInfo = null;
        _orScopeQueryEffective = false;
        _orScopeQueryAndPart = false;
    }

    protected void reflectTmpOrClauseToRealObject(TmpOrScopeQueryInfo localInfo) {
        {
            List<TmpOrScopeQueryGroupInfo> listList = setupTmpOrListList(localInfo, delegate(TmpOrScopeQueryInfo tmpOrScopeQueryInfo) {
                return tmpOrScopeQueryInfo.getTmpOrWhereList();
            });
            setupOrScopeQuery(listList, _whereList, true);
        }
        {
            List<TmpOrScopeQueryGroupInfo> listList = setupTmpOrListList(localInfo, delegate(TmpOrScopeQueryInfo tmpOrScopeQueryInfo) {
                return tmpOrScopeQueryInfo.getTmpOrBaseTableInlineWhereList();
            });
            setupOrScopeQuery(listList, _baseTableInlineWhereList, false);
        }
        {
            Set<Entry<String, LeftOuterJoinInfo>> entrySet = _outerJoinMap.entrySet();
            foreach (Entry<String, LeftOuterJoinInfo> entry in entrySet) {
                String aliasName = entry.getKey();
                LeftOuterJoinInfo joinInfo = entry.getValue();
                List<TmpOrScopeQueryGroupInfo> listList = new ArrayList<TmpOrScopeQueryGroupInfo>();
                listList.addAll(setupTmpOrListList(localInfo, delegate(TmpOrScopeQueryInfo tmpOrScopeQueryInfo) {
                    return tmpOrScopeQueryInfo.getTmpOrAdditionalOnClauseList(aliasName);
                }));
                setupOrScopeQuery(listList, joinInfo.getAdditionalOnClauseList(), false);
            }
        }
        {
            Set<Entry<String, LeftOuterJoinInfo>> entrySet = _outerJoinMap.entrySet();
            foreach (Entry<String, LeftOuterJoinInfo> entry in entrySet) {
                String aliasName = entry.getKey();
                LeftOuterJoinInfo joinInfo = entry.getValue();
                List<TmpOrScopeQueryGroupInfo> listList = new ArrayList<TmpOrScopeQueryGroupInfo>();
                listList.addAll(setupTmpOrListList(localInfo, delegate(TmpOrScopeQueryInfo tmpOrScopeQueryInfo) {
                    return tmpOrScopeQueryInfo.getTmpOrOuterJoinInlineClauseList(aliasName);
                }));
                setupOrScopeQuery(listList, joinInfo.getInlineWhereClauseList(), false);
            }
        }
    }

    protected List<TmpOrScopeQueryGroupInfo> setupTmpOrListList(TmpOrScopeQueryInfo parentInfo, TmpOrClauseListProvider provider) {
        List<TmpOrScopeQueryGroupInfo> resultList = new ArrayList<TmpOrScopeQueryGroupInfo>();
        TmpOrScopeQueryGroupInfo groupInfo = new TmpOrScopeQueryGroupInfo();
        groupInfo.setOrClauseList(provider.Invoke(parentInfo));
        resultList.add(groupInfo);
        if (parentInfo.hasChildInfo()) {
            foreach (TmpOrScopeQueryInfo childInfo in parentInfo.getChildInfoList()) {
                resultList.addAll(setupTmpOrListList(childInfo, provider)); // recursive call
            }
        }
        return resultList;
    }

    protected delegate List<String> TmpOrClauseListProvider(TmpOrScopeQueryInfo tmpOrScopeQueryInfo);

    protected void setupOrScopeQuery(List<TmpOrScopeQueryGroupInfo> tmpOrGroupList, List<String> realList, bool line) {
        if (tmpOrGroupList == null || tmpOrGroupList.isEmpty()) {
            return;
        }
        String or = " or ";
        String and = " and ";
        String lnIndentOr = line ? ln() + "    " : "";
        String lnIndentAnd = ""; // no line separator either way
        String andPartMark = getOrScopeQueryAndPartMark();
        StringBuilder sb = new StringBuilder();
        bool exists = false;
        int validCount = 0;
        int groupListIndex = 0;
        foreach (TmpOrScopeQueryGroupInfo groupInfo in tmpOrGroupList) {
            List<String> orClauseList = groupInfo.getOrClauseList();
            if (orClauseList == null || orClauseList.isEmpty()) {
                continue; // not increment index
            }
            int listIndex = 0;
            bool inAndPart = false;
            foreach (String element in orClauseList) {
                String orClause = element;
                bool currentAndPart = orClause.StartsWith(andPartMark);
                bool beginAndPart;
                bool secondAndPart;
                if (currentAndPart) {
                    if (inAndPart) { // already begin
                        beginAndPart = false;
                        secondAndPart = true;
                    } else {
                        beginAndPart = true;
                        secondAndPart = false;
                        inAndPart = true;
                    }
                    orClause = orClause.Substring(andPartMark.Length);
                } else {
                    if (inAndPart) {
                        sb.append(")");
                        inAndPart = false;
                    }
                    beginAndPart = false;
                    secondAndPart = false;
                }
                if (groupListIndex == 0) { // first list
                    if (listIndex == 0) {
                        sb.append("(");
                    } else {
                        sb.append(secondAndPart ? lnIndentAnd : lnIndentOr);
                        sb.append(secondAndPart ? and : or);
                    }
                } else { // second or more list
                    if (listIndex == 0) {
                        // always 'or' here
                        sb.append(lnIndentOr);
                        sb.append(or);
                        sb.append("(");
                    } else {
                        sb.append(secondAndPart ? lnIndentAnd : lnIndentOr);
                        sb.append(secondAndPart ? and : or);
                    }
                }
                sb.append(beginAndPart ? "(" : "");
                sb.append(orClause);
                ++validCount;
                if (!exists) {
                    exists = true;
                }
                ++listIndex;
            }
            if (inAndPart) {
                sb.append(")");
                inAndPart = false;
            }
            if (groupListIndex > 0) { // second or more list
                sb.append(")");
            }
            ++groupListIndex;
        }
        if (exists) {
            sb.append(line && validCount > 1 ? ln() + "       " : "").append(")");
            realList.add(sb.toString());
        }
    }

    public bool isOrScopeQueryEffective() {
        return _orScopeQueryEffective;
    }

    protected List<String> getTmpOrWhereList() {
        assertCurrentTmpOrScopeQueryInfo();
        return _currentTmpOrScopeQueryInfo.getTmpOrWhereList();
    }

    protected List<String> getTmpOrBaseTableInlineWhereList() {
        assertCurrentTmpOrScopeQueryInfo();
        return _currentTmpOrScopeQueryInfo.getTmpOrBaseTableInlineWhereList();
    }

    protected List<String> getTmpOrAdditionalOnClauseList(String aliasName) {
        assertCurrentTmpOrScopeQueryInfo();
        return _currentTmpOrScopeQueryInfo.getTmpOrAdditionalOnClauseList(aliasName);
    }

    protected List<String> getTmpOrOuterJoinInlineClauseList(String aliasName) {
        assertCurrentTmpOrScopeQueryInfo();
        return _currentTmpOrScopeQueryInfo.getTmpOrOuterJoinInlineClauseList(aliasName);
    }

    protected class TmpOrScopeQueryInfo {
        protected List<String> _tmpOrWhereList;
        protected List<String> _tmpOrBaseTableInlineWhereList;
        protected Map<String, List<String>> _tmpOrAdditionalOnClauseListMap;
        protected Map<String, List<String>> _tmpOrOuterJoinInlineClauseListMap;
        protected TmpOrScopeQueryInfo _parentInfo; // null means base point
        protected List<TmpOrScopeQueryInfo> _childInfoList;

        public List<String> getTmpOrAdditionalOnClauseList(String aliasName) {
            List<String> orClauseList = getTmpOrAdditionalOnClauseListMap().get(aliasName);
            if (orClauseList != null) {
                return orClauseList;
            }
            orClauseList = new ArrayList<String>();
            _tmpOrAdditionalOnClauseListMap.put(aliasName, orClauseList);
            return orClauseList;
        }

        public List<String> getTmpOrOuterJoinInlineClauseList(String aliasName) {
            List<String> orClauseList = getTmpOrOuterJoinInlineClauseListMap().get(aliasName);
            if (orClauseList != null) {
                return orClauseList;
            }
            orClauseList = new ArrayList<String>();
            _tmpOrOuterJoinInlineClauseListMap.put(aliasName, orClauseList);
            return orClauseList;
        }

        public List<String> getTmpOrWhereList() {
            if (_tmpOrWhereList == null) {
                _tmpOrWhereList = new ArrayList<String>();
            }
            return _tmpOrWhereList;
        }

        public void setTmpOrWhereList(List<String> tmpOrWhereList) {
            this._tmpOrWhereList = tmpOrWhereList;
        }

        public List<String> getTmpOrBaseTableInlineWhereList() {
            if (_tmpOrBaseTableInlineWhereList == null) {
                _tmpOrBaseTableInlineWhereList = new ArrayList<String>();
            }
            return _tmpOrBaseTableInlineWhereList;
        }

        public void setTmpOrBaseTableInlineWhereList(List<String> tmpOrBaseTableInlineWhereList) {
            this._tmpOrBaseTableInlineWhereList = tmpOrBaseTableInlineWhereList;
        }

        public Map<String, List<String>> getTmpOrAdditionalOnClauseListMap() {
            if (_tmpOrAdditionalOnClauseListMap == null) {
                _tmpOrAdditionalOnClauseListMap = new LinkedHashMap<String, List<String>>();
            }
            return _tmpOrAdditionalOnClauseListMap;
        }

        public void setTmpOrAdditionalOnClauseListMap(Map<String, List<String>> tmpOrAdditionalOnClauseListMap) {
            this._tmpOrAdditionalOnClauseListMap = tmpOrAdditionalOnClauseListMap;
        }

        public Map<String, List<String>> getTmpOrOuterJoinInlineClauseListMap() {
            if (_tmpOrOuterJoinInlineClauseListMap == null) {
                _tmpOrOuterJoinInlineClauseListMap = new LinkedHashMap<String, List<String>>();
            }
            return _tmpOrOuterJoinInlineClauseListMap;
        }

        public void setTmpOrOuterJoinInlineClauseListMap(Map<String, List<String>> tmpOrOuterJoinInlineClauseListMap) {
            this._tmpOrOuterJoinInlineClauseListMap = tmpOrOuterJoinInlineClauseListMap;
        }

        public bool hasChildInfo() {
            return _childInfoList != null && !_childInfoList.isEmpty();
        }

        public TmpOrScopeQueryInfo getParentInfo() {
            return _parentInfo;
        }

        public void setParentInfo(TmpOrScopeQueryInfo parentInfo) {
            _parentInfo = parentInfo;
        }

        public List<TmpOrScopeQueryInfo> getChildInfoList() {
            if (_childInfoList == null) {
                _childInfoList = new ArrayList<TmpOrScopeQueryInfo>();
            }
            return _childInfoList;
        }

        public void addChildInfo(TmpOrScopeQueryInfo childInfo) {
            childInfo.setParentInfo(this);
            getChildInfoList().add(childInfo);
        }
    }

    protected class TmpOrScopeQueryGroupInfo {
        protected List<String> _orClauseList;
        protected bool _forcedAnd;

        public override String ToString() {
            return "{orClauseList=" + (_orClauseList != null ? _orClauseList.size().ToString() : "null") + "}";
        }

        public List<String> getOrClauseList() {
            return _orClauseList;
        }

        public void setOrClauseList(List<String> orClauseList) {
            this._orClauseList = orClauseList;
        }
    }

    public void beginOrScopeQueryAndPart() {
        assertCurrentTmpOrScopeQueryInfo();
        _orScopeQueryAndPart = true;
    }

    public void endOrScopeQueryAndPart() {
        assertCurrentTmpOrScopeQueryInfo();
        _orScopeQueryAndPart = false;
    }

    protected void markOrScopeQueryAndPart(List<String> clauseList) {
        if (_orScopeQueryEffective && _orScopeQueryAndPart && !clauseList.isEmpty()) {
            String original = clauseList.remove(clauseList.size() - 1); // as latest
            clauseList.add(getOrScopeQueryAndPartMark() + original);
        }
    }

    protected String getOrScopeQueryAndPartMark() {
        return "$$df:AndPart$$";
    }

    protected void assertCurrentTmpOrScopeQueryInfo() {
        if (_currentTmpOrScopeQueryInfo == null) {
            String msg = "The attribute 'currentTmpOrScopeQueryInfo' should not be null in or-scope query:";
            msg = msg + " orScopeQueryEffective=" + _orScopeQueryEffective;
            throw new IllegalStateException(msg);
        }
    }

    // ===================================================================================
    //                                                                             OrderBy
    //                                                                             =======
    public OrderByClause getSqlComponentOfOrderByClause() {
        return _orderByClause;
    }

    public SqlClause clearOrderBy() {
        _orderByEffective = false;
        _orderByClause.clear();
        return this;
    }

    public SqlClause ignoreOrderBy() {
        _orderByEffective = false;
        return this;
    }

    public SqlClause makeOrderByEffective() {
        if (!_orderByClause.isEmpty()) {
            _orderByEffective = true;
        }
        return this;
    }

    public void reverseOrderBy_Or_OverrideOrderBy(String orderByProperty, String registeredOrderByProperty, bool ascOrDesc) {
        _orderByEffective = true;
        if (!_orderByClause.isSameOrderByColumn(orderByProperty)) {
            clearOrderBy();
            registerOrderBy(orderByProperty, registeredOrderByProperty, ascOrDesc);
        } else {
            _orderByClause.reverseAll();
        }
    }

    public void registerOrderBy(String orderByProperty, String registeredOrderByProperty, bool ascOrDesc) {
        try {
            _orderByEffective = true;
            List<String> orderByList = new ArrayList<String>();
            {
                String[] splitArray = orderByProperty.Split('/');
                foreach (String element in splitArray) {
                    orderByList.add(element);
                }
            }

            if (registeredOrderByProperty == null || registeredOrderByProperty.Trim().Length ==0) {
                registeredOrderByProperty = orderByProperty;
            }

            List<String> registeredOrderByList = new ArrayList<String>();
            {
                String[] splitArray = registeredOrderByProperty.Split('/');
                foreach (String element in splitArray) {
                    registeredOrderByList.add(element);
                }
            }

            int count = 0;
            for (Iterator<String> ite = orderByList.iterator(); ite.hasNext(); ) {
                String orderBy = ite.next();
                String registeredOrderBy = (String)registeredOrderByList.get(count);

                _orderByEffective = true;
                String aliasName = null;
                String columnName = null;
                String registeredAliasName = null;
                String registeredColumnName = null;

                if (orderBy.IndexOf(".") < 0) {
                    columnName = orderBy;
                } else {
                    aliasName = orderBy.Substring(0, orderBy.LastIndexOf("."));
                    columnName = orderBy.Substring(orderBy.LastIndexOf(".") + 1);
                }

                if (registeredOrderBy.IndexOf(".") < 0) {
                    registeredColumnName = registeredOrderBy;
                } else {
                    registeredAliasName = registeredOrderBy.Substring(0, registeredOrderBy.LastIndexOf("."));
                    registeredColumnName = registeredOrderBy.Substring(registeredOrderBy.LastIndexOf(".") + 1);
                }

                OrderByElement element = new OrderByElement();
                element.setAliasName(aliasName);
                element.setColumnName(columnName);
                element.setRegisteredAliasName(registeredAliasName);
                element.setRegisteredColumnName(registeredColumnName);
                if (ascOrDesc) {
                    element.setupAsc();
                } else {
                    element.setupDesc();
                }
                _orderByClause.addOrderByElement(element);

                count++;
            }
        } catch (RuntimeException e) {
            String msg = "registerOrderBy() threw the exception: orderByProperty=" + orderByProperty;
            msg = msg + " registeredColumnFullName=" + registeredOrderByProperty;
            msg = msg + " ascOrDesc=" + ascOrDesc;
            msg = msg + " sqlClause=" + this.ToString();
            throw new RuntimeException(msg, e);
        }
    }
    
    public virtual void addNullsFirstToPreviousOrderBy() {
        _orderByClause.addNullsFirstToPreviousOrderByElement(createOrderByNullsSetupper());
    }
    
    public virtual void addNullsLastToPreviousOrderBy() {
        _orderByClause.addNullsLastToPreviousOrderByElement(createOrderByNullsSetupper());
    }
    
    protected virtual OrderByNullsSetupper createOrderByNullsSetupper() {// As Default
        return new OrderByNullsSetupperBySupported();
    }
    
    protected class OrderByNullsSetupperBySupported : OrderByNullsSetupper {
        public String setup(String columnName, String orderByElementClause, bool nullsFirst) {
            return orderByElementClause + " nulls " + (nullsFirst ? "first" : "last");
        }
    }
    
    protected virtual OrderByNullsSetupper createOrderByNullsSetupperByCaseWhen() {// Helper For Nulls Unsupported Database
        return new OrderByNullsSetupperByCaseWhen();
    }

    protected class OrderByNullsSetupperByCaseWhen : OrderByNullsSetupper {
        public String setup(String columnName, String orderByElementClause, bool nullsFirst) {
            String thenNumber = nullsFirst ? "1" : "0";
            String elseNumber = nullsFirst ? "0" : "1";
            String caseWhen = "case when " + columnName + " is not null then " + thenNumber + " else " + elseNumber + " end asc";
            return caseWhen + ", " + orderByElementClause;
        }
    }

    public bool hasOrderByClause() {
        return _orderByClause != null && !_orderByClause.isEmpty();
    }

    // ===================================================================================
    //                                                                          UnionQuery
    //                                                                          ==========
    public void registerUnionQuery(String unionQueryClause, bool unionAll) {
        assertStringNotNullAndNotTrimmedEmpty("unionQueryClause", unionQueryClause);
        UnionQueryInfo unionQueryInfo = new UnionQueryInfo();
        unionQueryInfo.setUnionQueryClause(unionQueryClause);
        unionQueryInfo.setUnionAll(unionAll);
        addUnionQueryInfo(unionQueryInfo);
    }

    protected void addUnionQueryInfo(UnionQueryInfo unionQueryInfo) {
        if (_unionQueryInfoList == null) {
            _unionQueryInfoList = new ArrayList<UnionQueryInfo>(); 
        }
        _unionQueryInfoList.add(unionQueryInfo);
    }

    public bool hasUnionQuery() {
        return _unionQueryInfoList != null && !_unionQueryInfoList.isEmpty();
    }

    protected class UnionQueryInfo {
        protected String _unionQueryClause;
        protected bool _unionAll;
        public String getUnionQueryClause() {
            return _unionQueryClause;
        }
        public void setUnionQueryClause(String unionQueryClause) {
            _unionQueryClause = unionQueryClause;
        }
        public bool isUnionAll() {
            return _unionAll;
        }
        public void setUnionAll(bool unionAll) {
            _unionAll = unionAll;
        }
    }

    // ===================================================================================
    //                                                                             Advance
    //                                                                             =======
    /**
     * @param fetchSize Fetch-size. (NotMinus & NotZero)
     * @return this. (NotNull)
     */
    public SqlClause fetchFirst(int fetchSize) {
        _fetchScopeEffective = true;
        if (fetchSize <= 0) {
            String msg = "Argument[fetchSize] must be plus: " + fetchSize;
            throw new IllegalArgumentException(msg);
        }
        _fetchStartIndex = 0;
        _fetchSize = fetchSize;
        _fetchPageNumber = 1;
        doClearFetchPageClause();
        doFetchFirst();
        return this;
    }

    /**
     * @param fetchStartIndex Fetch-start-index. 0 origin. (NotMinus)
     * @param fetchSize Fetch size. (NotMinus)
     * @return this. (NotNull)
     */
    public SqlClause fetchScope(int fetchStartIndex, int fetchSize) {
        _fetchScopeEffective = true;
        if (fetchStartIndex < 0) {
            String msg = "Argument[fetchStartIndex] must be plus or zero: " + fetchStartIndex;
            throw new IllegalArgumentException(msg);
        }
        if (fetchSize <= 0) {
            String msg = "Argument[fetchSize] must be plus: " + fetchSize;
            throw new IllegalArgumentException(msg);
        }
        _fetchStartIndex = fetchStartIndex;
        _fetchSize = fetchSize;
        return fetchPage(1);
    }

    /**
     * @param fetchPageNumber Page-number. 1 origin. (NotMinus & NotZero: If minus or zero, set one.)
     * @return this. (NotNull)
     */
    public SqlClause fetchPage(int fetchPageNumber) {
        _fetchScopeEffective = true;
        if (fetchPageNumber <= 0) {
            fetchPageNumber = 1;
        }
        if (_fetchSize <= 0) {
            String msg = "Look! Read the message below." + ln();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
            msg = msg + "Fetch size should not be minus or zero." + ln();
            msg = msg + ln();
            msg = msg + "[Fetch Size]" + ln();
            msg = msg + "fetchSize=" + _fetchSize + ln();
            msg = msg + ln();
            msg = msg + "[Fetch Page Number]" + ln();
            msg = msg + "fetchPageNumber=" + fetchPageNumber + ln();
            msg = msg + "* * * * * * * * * */" + ln();
            throw new IllegalStateException(msg);
        }
        _fetchPageNumber = fetchPageNumber;
        if (_fetchPageNumber == 1 && _fetchStartIndex == 0) {
            return fetchFirst(_fetchSize);
        }
        doClearFetchPageClause();
        doFetchPage();
        return this;
    }

    abstract protected void doFetchFirst();
    abstract protected void doFetchPage();
    abstract protected void doClearFetchPageClause();

    protected class RownumPagingProcessor {
        protected String _rownumExpression;
        protected bool _offset;
        protected bool _limit;
        protected int _pageStartIndex;
        protected int _pageEndIndex;
        protected String _selectHint = "";
        protected String _sqlSuffix = "";

        public RownumPagingProcessor(String rownumExpression
                , bool offset, bool limit, int pageStartIndex, int pageEndIndex) {
            _rownumExpression = rownumExpression;
            _offset = offset;
            _limit = limit;
            _pageStartIndex = pageStartIndex;
            _pageEndIndex = pageEndIndex;
        }

        public void processRowNumberPaging() {
            bool offset = _offset;
            bool limit = _limit;
            if (!offset && !limit) {
                return;
            }

            StringBuilder hintSb = new StringBuilder();
            String rownum = _rownumExpression;
            hintSb.append(" *").append(ln());
            hintSb.append("  from (").append(ln());
            hintSb.append("select plain.*, ").append(rownum).append(" as rn").append(ln());
            hintSb.append("  from (").append(ln());
            hintSb.append("select"); // main select

            StringBuilder suffixSb = new StringBuilder();
            String fromEnd = "       ) plain" + ln() + "       ) ext" + ln();
            if (offset) {
                int pageStartIndex = _pageStartIndex;
                suffixSb.append(fromEnd).append(" where ext.rn > ").append(pageStartIndex);
            }
            if (limit) {
                int pageEndIndex = _pageEndIndex;
                if (offset) {
                    suffixSb.append(ln()).append("   and ext.rn <= ").append(pageEndIndex);
                } else {
                    suffixSb.append(fromEnd).append(" where ext.rn <= ").append(pageEndIndex);
                }
            }

            _selectHint = hintSb.toString();
            _sqlSuffix = suffixSb.toString();
        }

        protected String ln() {
            return SimpleSystemUtil.GetLineSeparator();
        }

        public String getSelectHint() {
            return _selectHint;
        }

        public String getSqlSuffix() {
            return _sqlSuffix;
        }
    }

    public virtual int getFetchStartIndex() {
        return _fetchStartIndex;
    }

    public virtual int getFetchSize() {
        return _fetchSize;
    }

    public virtual int getFetchPageNumber() {
        return _fetchPageNumber;
    }

    /**
     * @return Page start index. 0 origin. (NotMinus)
     */
    public virtual int getPageStartIndex() {
        if (_fetchPageNumber <= 0) {
            String msg = "_fetchPageNumber must be plus: " + _fetchPageNumber;
            throw new IllegalStateException(msg);
        }
        return _fetchStartIndex + (_fetchSize * (_fetchPageNumber - 1));
    }

    /**
     * @return Page end index. 0 origin. (NotMinus)
     */
    public virtual int getPageEndIndex() {
        if (_fetchPageNumber <= 0) {
            String msg = "_fetchPageNumber must be plus: " + _fetchPageNumber;
            throw new IllegalStateException(msg);
        }
        return _fetchStartIndex + (_fetchSize * _fetchPageNumber);
    }

    public virtual bool isFetchScopeEffective() {
        return _fetchScopeEffective;
    }

    public virtual SqlClause ignoreFetchScope() {
        _fetchScopeEffective = false;
        doClearFetchPageClause();
        return this;
    }

    public virtual SqlClause makeFetchScopeEffective() {
        if (getFetchSize() > 0 && getFetchPageNumber() > 0) {
            fetchPage(getFetchPageNumber());
        }
        return this;
    }
    
    public virtual bool isFetchStartIndexSupported() {
        return true; // Default
    }

    public virtual bool isFetchSizeSupported() {
        return true; // Default
    }

    abstract protected String createSelectHint();
    abstract protected String createFromBaseTableHint();
    abstract protected String createFromHint();
    abstract protected String createSqlSuffix();

    // ===================================================================================
    //                                                                     Fetch Narrowing
    //                                                                     ===============
    public virtual int getFetchNarrowingSkipStartIndex() {
        return getPageStartIndex();
    }

    public virtual int getFetchNarrowingLoopCount() {
        return getFetchSize();
    }

    public virtual bool isFetchNarrowingEffective() {
        return _fetchScopeEffective;
    }

    // ===================================================================================
    //                                                                                Lock
    //                                                                                ====
    public abstract SqlClause lockForUpdate();
    
    // ===================================================================================
    //                                                                    Table Alias Info
    //                                                                    ================
    public String getBasePointAliasName() {
        // the variable should be resolved when making a sub-query clause
        return isForSubQuery() ? "sub" + getSubQueryLevel() + "loc" : "dfloc";
    }

    public String resolveJoinAliasName(String relationPath, int nestLevel) {
        return (isForSubQuery() ? "sub" + getSubQueryLevel() : "df") + "rel" + relationPath;

        // nestLevel is unused because relationPath has same role
        // (that was used long long ago)
    }

    public int resolveRelationNo(String localTableName, String foreignPropertyName) {
         DBMeta dbmeta = findDBMeta(localTableName);
         ForeignInfo foreignInfo = dbmeta.FindForeignInfo(foreignPropertyName);
        return foreignInfo.RelationNo;
    }

    // ===================================================================================
    //                                                                       Template Mark
    //                                                                       =============
    public String getWhereClauseMark() {
        return "#df:whereClause#";
    }
    
    public String getWhereFirstConditionMark() {
        return "#df:whereFirstCondition#";
    }
    
    public String getUnionSelectClauseMark() {
        return "#df:unionSelectClause#";
    }
    
    public String getUnionWhereClauseMark() {
        return "#df:unionWhereClause#";
    }
    
    public String getUnionWhereFirstConditionMark() {
        return "#df:unionWhereFirstCondition#";
    }
    
//    // =====================================================================================
//    //                                                            Where Clause Simple Filter
//    //                                                            ==========================
//    public void addWhereClauseSimpleFilter(WhereClauseSimpleFilter whereClauseSimpleFilter) {
//        if (_whereClauseSimpleFilterList == null) {
//            _whereClauseSimpleFilterList = new ArrayList<WhereClauseSimpleFilter>();
//        }
//        _whereClauseSimpleFilterList.add(whereClauseSimpleFilter);
//    }
//
//    protected String filterWhereClauseSimply(String clauseElement) {
//        if (_whereClauseSimpleFilterList == null || _whereClauseSimpleFilterList.isEmpty()) {
//            return clauseElement;
//        }
//        for (Iterator<WhereClauseSimpleFilter> ite = _whereClauseSimpleFilterList.iterator(); ite.hasNext(); ) {
//             WhereClauseSimpleFilter filter = ite.next();
//            if (filter == null) {
//                String msg = "The list of filter should not have null: _whereClauseSimpleFilterList=" + _whereClauseSimpleFilterList;
//                throw new IllegalStateException(msg);
//            }
//            clauseElement = filter.filterClauseElement(clauseElement);
//        }
//        return clauseElement;
//    }
    
    // =====================================================================================
    //                                                                 Selected Foreign Info
    //                                                                 =====================
    public bool isSelectedForeignInfoEmpty() {
        if (_selectedForeignInfo == null) {
            return true;
        }
        return _selectedForeignInfo.isEmpty();
    }

    public bool hasSelectedForeignInfo(String relationPath) {
        if (_selectedForeignInfo == null) {
            return false;
        }
        return _selectedForeignInfo.containsKey(relationPath);
    }

    public void registerSelectedForeignInfo(String relationPath, String foreignPropertyName) {
        if (_selectedForeignInfo == null) {
            _selectedForeignInfo = new HashMap<String, String>();
        }
        _selectedForeignInfo.put(relationPath, foreignPropertyName);
    }

    // ===================================================================================
    //                                                                    Sub Query Indent
    //                                                                    ================
    public String resolveSubQueryBeginMark(String subQueryIdentity) {
        return getSubQueryBeginMarkPrefix() + subQueryIdentity + getSubQueryIdentityTerminal();
    }

    public String resolveSubQueryEndMark(String subQueryIdentity) {
        return getSubQueryEndMarkPrefix() + subQueryIdentity + getSubQueryIdentityTerminal();
    }

    protected String getSubQueryBeginMarkPrefix() {
        return "--df:SubQueryBegin#";
    }

    protected String getSubQueryEndMarkPrefix() {
        return "--df:SubQueryEnd#";
    }
    
    protected String getSubQueryIdentityTerminal() {
        return "#IdentityTerminal#";
    }

    public String filterSubQueryIndent(String sql) {
        return filterSubQueryIndent(sql, "", sql);
    }

    protected String filterSubQueryIndent(String sql, String preIndent, String originalSql) {
        if (!sql.Contains(getSubQueryBeginMarkPrefix())) {
            return sql;
        }
        String[] lines = sql.Split(ln().ToCharArray());// *Attension about difference between Language!
        String beginMarkPrefix = getSubQueryBeginMarkPrefix();
        String endMarkPrefix = getSubQueryEndMarkPrefix();
        String identityTerminal = getSubQueryIdentityTerminal();
        int terminalLength = identityTerminal.Length;
        StringBuilder mainSb = new StringBuilder();
        StringBuilder subSb = null;
        bool throughBegin = false;
        bool throughBeginFirst = false;
        String subQueryIdentity = null;
        String indent = null;
        foreach (String line in lines) {
            if (!throughBegin) {
                if (line.Contains(beginMarkPrefix)) {
                    throughBegin = true;
                    subSb = new StringBuilder();
                    int markIndex = line.IndexOf(beginMarkPrefix);
                    int terminalIndex = line.IndexOf(identityTerminal);
                    if (terminalIndex < 0) {
                        String msg = "Identity terminal was Not Found at the begin line: [" + line + "]";
                        throw new SubQueryIndentFailureException(msg);
                    }
                    String clause = line.Substring(0, markIndex) + line.Substring(terminalIndex + terminalLength);
                    subQueryIdentity = line.Substring(markIndex + beginMarkPrefix.Length, terminalIndex - (markIndex + beginMarkPrefix.Length));// *Attension about difference between Language!
                    subSb.append(clause);
                    indent = buildSpaceBar(markIndex - preIndent.Length);
                } else {
                    mainSb.append(line).append(ln());
                }
            } else {
                // - - - - - - - -
                // In begin to end
                // - - - - - - - -
                if (line.Contains(endMarkPrefix + subQueryIdentity)) {// The end
                    int markIndex = line.IndexOf(endMarkPrefix);
                    int terminalIndex = line.IndexOf(identityTerminal);
                    if (terminalIndex < 0) {
                        String msg = "Identity terminal was Not Found at the begin line: [" + line + "]";
                        throw new SubQueryIndentFailureException(msg);
                    }
                    String clause = line.Substring(0, markIndex) + line.Substring(terminalIndex + terminalLength);
                    subSb.append(clause).append(ln());
                    String currentSql = filterSubQueryIndent(subSb.toString(), preIndent + indent, originalSql);
                    mainSb.append(currentSql);
                    throughBegin = false;
                    throughBeginFirst = false;
                } else {
                    if (!throughBeginFirst) {
                        subSb.append(line.Trim()).append(ln());
                        throughBeginFirst = true;
                    } else {
                        subSb.append(indent).append(line).append(ln());
                    }
                }
            }
        }
        String filteredSql = mainSb.toString();
        
        if (throughBegin) {
            String msg = "End Mark Not Found!";
            msg = msg + ln() + "[Current SubQueryIdentity]" + ln();
            msg = msg + subQueryIdentity + ln();
            msg = msg + ln() + "[Before Filter]" + ln();
            msg = msg + ln() + "[After Filter]" + ln() + filteredSql;
            msg = msg + ln() + "[Original SQL]" + ln() + originalSql;
            throw new SubQueryIndentFailureException(msg);
        }
        if (filteredSql.Contains(beginMarkPrefix)) {
            String msg = "Any begin marks are not filtered!";
            msg = msg + ln() + "[Current SubQueryIdentity]" + ln();
            msg = msg + subQueryIdentity + ln();
            msg = msg + ln() + "[Before Filter]" + ln();
            msg = msg + ln() + "[After Filter]" + ln() + filteredSql;
            msg = msg + ln() + "[Original SQL]" + ln() + originalSql;
            throw new SubQueryIndentFailureException(msg);
        }
        return filteredSql;
    }
    
    protected String buildSpaceBar(int size) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < size; i++) {
            sb.append(" ");
        }
        return sb.toString();
    }

    public class SubQueryIndentFailureException : RuntimeException {
        public SubQueryIndentFailureException(String msg) : base(msg) {
        }
    }

    // ===================================================================================
    //                                                                       Specification
    //                                                                       =============
    public void specifySelectColumn(String tableAliasName, String columnName) {
        if (_specifiedSelectColumnMap == null) {
            _specifiedSelectColumnMap = new HashMap<String, Map<String, String>>();
        }
        if (!_specifiedSelectColumnMap.containsKey(tableAliasName)) {
            _specifiedSelectColumnMap.put(tableAliasName, new LinkedHashMap<String, String>());
        }
        Map<String, String> elementMap = _specifiedSelectColumnMap.get(tableAliasName);
        elementMap.put(columnName, null); // The null value is unused actually, this is for future.
    }

    public bool hasSpecifiedSelectColumn(String tableAliasName) {
        return _specifiedSelectColumnMap != null && _specifiedSelectColumnMap.containsKey(tableAliasName);
    }

    public void specifyDeriveSubQuery(String aliasName, String deriveSubQuery) {
        if (_specifiedDeriveSubQueryMap == null) {
            _specifiedDeriveSubQueryMap = new LinkedHashMap<String, String>();
        }
        _specifiedDeriveSubQueryMap.put(aliasName, deriveSubQuery);
    }

    public bool hasSpecifiedDeriveSubQuery(String aliasName) {
        if (_specifiedDeriveSubQueryMap == null) { return false; }
        return _specifiedDeriveSubQueryMap.containsKey(aliasName);
    }

    public String getSpecifiedColumnNameAsOne() {
        if (_specifiedSelectColumnMap != null && _specifiedSelectColumnMap.size() == 1) {
            Map<String, String> elementMap = _specifiedSelectColumnMap.get(_specifiedSelectColumnMap.keySet().iterator().next());
            if (elementMap != null && elementMap.size() == 1) {
                return elementMap.keySet().iterator().next();
            }
        }
        return null;
    }

    public String getSpecifiedColumnRealNameAsOne() {
        if (_specifiedSelectColumnMap != null && _specifiedSelectColumnMap.size() == 1) {
            String tableAliasName = _specifiedSelectColumnMap.keySet().iterator().next();
            Map<String, String> elementMap = _specifiedSelectColumnMap.get(tableAliasName);
            if (elementMap != null && elementMap.size() == 1) {
                return tableAliasName + "." + elementMap.keySet().iterator().next();
            }
        }
        return null;
    }

    public void clearSpecifiedSelectColumn() {
        if (_specifiedSelectColumnMap != null) { _specifiedSelectColumnMap.clear(); _specifiedSelectColumnMap = null; }
    }

    // ===================================================================================
    //                                                                  Invalid Query Info
    //                                                                  ==================
    public bool isCheckInvalidQuery() {
        return _checkInvalidQuery;
    }

    public void checkInvalidQuery() {
        _checkInvalidQuery = true;
    }

    public Map<String, ConditionKey> getInvalidQueryColumnMap() {
        if (_invalidQueryColumnMap != null) {
            return _invalidQueryColumnMap;
        }
        return new HashMap<String, ConditionKey>();
    }

    public void registerInvalidQueryColumn(String columnFullName, ConditionKey key) {
        if (_invalidQueryColumnMap == null) {
            _invalidQueryColumnMap = new LinkedHashMap<String, ConditionKey>();
        }
        _invalidQueryColumnMap.put(columnFullName, key);
    }

    // [DBFlute-0.7.9]
    // ===================================================================================
    //                                                                        Query Update
    //                                                                        ============
    public String getClauseQueryUpdate(Map<String, String> columnParameterMap) {
        if (columnParameterMap.isEmpty()) {
            return null;
        }
        String aliasName = getBasePointAliasName();
        DBMeta dbmeta = getDBMeta();
        if (dbmeta.HasCompoundPrimaryKey) {
            String msg = "The target table of queryUpdate() should have only one primary key:";
            msg = msg + " primaryKeys=" + toStringView(dbmeta.PrimaryUniqueInfo.UniqueColumnList);
            throw new IllegalStateException(msg);
        }
        String tableSqlName = dbmeta.TableSqlName;
        String primaryKeyName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
        String selectClause = "select " + aliasName + "." + primaryKeyName;
        String fromWhereClause = getClauseFromWhereWithUnionTemplate();

        // Replace template marks. These are very important!
        fromWhereClause = replaceString(fromWhereClause, getUnionSelectClauseMark(), selectClause);
        fromWhereClause = replaceString(fromWhereClause, getUnionWhereClauseMark(), "");
        fromWhereClause = replaceString(fromWhereClause, getUnionWhereFirstConditionMark(), "");

        StringBuilder sb = new StringBuilder();
        sb.append("update ").append(tableSqlName).append(ln());
        int index = 0;
        // It is guaranteed that the map has one or more elements.
        foreach (String columnName in columnParameterMap.keySet()) {
            String parameter = columnParameterMap.get(columnName); 
            if (index == 0) {
                sb.append("   set ").append(columnName).append(" = ").append(parameter).append(ln());
            } else {
                sb.append("     , ").append(columnName).append(" = ").append(parameter).append(ln());
            }
            ++index;
        }
        if (isUpdateSubQueryUseLocalTableSupported()) {
            String subQuery = filterSubQueryIndent(selectClause + " " + fromWhereClause);
            sb.append(" where ").append(primaryKeyName);
            sb.append(" in (").append(ln()).append(subQuery);
            if (!subQuery.EndsWith(ln())) {
                sb.append(ln());
            }
            sb.append(")");
            return sb.toString();
        } else {
            String subQuery = filterSubQueryIndent(fromWhereClause);
            subQuery = replaceString(subQuery, aliasName + ".", "");
            subQuery = replaceString(subQuery, " " + aliasName + " ", " ");
            int whereIndex = subQuery.IndexOf("where ");
            if (whereIndex < 0) {
                return sb.toString();
            }
            subQuery = subQuery.Substring(whereIndex);
            sb.append(" ").append(subQuery);
            return sb.toString();
        }
    }

    public String getClauseQueryDelete() {
        String aliasName = getBasePointAliasName();
        DBMeta dbmeta = getDBMeta();
        if (dbmeta.HasCompoundPrimaryKey) {
            String msg = "The target table of queryDelete() should have only one primary key:";
            msg = msg + " primaryKeys=" + toStringView(dbmeta.PrimaryUniqueInfo.UniqueColumnList);
            throw new IllegalStateException(msg);
        }
        String tableSqlName = dbmeta.TableSqlName;
        String primaryKeyName = dbmeta.PrimaryUniqueInfo.FirstColumn.ColumnDbName;
        String selectClause = "select " + aliasName + "." + primaryKeyName;
        String fromWhereClause = getClauseFromWhereWithUnionTemplate();
        
        // Replace template marks. These are very important!
        fromWhereClause = replaceString(fromWhereClause, getUnionSelectClauseMark(), selectClause);
        fromWhereClause = replaceString(fromWhereClause, getUnionWhereClauseMark(), "");
        fromWhereClause = replaceString(fromWhereClause, getUnionWhereFirstConditionMark(), "");
        
        if (isUpdateSubQueryUseLocalTableSupported()) {
            String subQuery = filterSubQueryIndent(selectClause + " " + fromWhereClause);
            StringBuilder sb = new StringBuilder();
            sb.append("delete from ").append(tableSqlName).append(ln());
            sb.append(" where ").append(primaryKeyName);
            sb.append(" in (").append(ln()).append(subQuery);
            if (!subQuery.EndsWith(ln())) {
                sb.append(ln());
            }
            sb.append(")");
            return sb.toString();
        } else {
            String subQuery = filterSubQueryIndent(fromWhereClause);
            subQuery = replaceString(subQuery, aliasName + ".", "");
            subQuery = replaceString(subQuery, " " + aliasName + " ", " ");
            subQuery = subQuery.Substring(subQuery.IndexOf("from "));
            return "delete " + subQuery;
        }
    }

    protected virtual bool isUpdateSubQueryUseLocalTableSupported() {
        return true;
    }

    // [DBFlute-0.8.6]
    // ===================================================================================
    //                                                                  Select Clause Type
    //                                                                  ==================
    public void classifySelectClauseType(SelectClauseType selectClauseType) {
        changeSelectClauseType(selectClauseType);
    }

    protected void changeSelectClauseType(SelectClauseType selectClauseType) {
        savePreviousSelectClauseType();
        _selectClauseType = selectClauseType;
    }

    protected void savePreviousSelectClauseType() {
        _previousSelectClauseType = _selectClauseType;
    }

    public void rollbackSelectClauseType() {
        _selectClauseType = _previousSelectClauseType != null ? _previousSelectClauseType : DEFAULT_SELECT_CLAUSE_TYPE;
    }

    // [DBFlute-0.8.9.9]
    // ===================================================================================
    //                                                                       InScope Limit
    //                                                                       =============
    public virtual int getInScopeLimit() {
        return 0; // as default
    }

    // [DBFlute-0.7.9]
    // ===================================================================================
    //                                                                       DBMeta Helper
    //                                                                       =============
    protected DBMeta getDBMeta() {
        if (_dbmeta != null) {
            return _dbmeta;
        }
        _dbmeta = findDBMeta(_tableName);
        return _dbmeta;
    }

    protected DBMeta findDBMeta(String tableDbName) {
        return DBMetaInstanceHandler.FindDBMeta(tableDbName); 
    }

    // ===================================================================================
    //                                                                      General Helper
    //                                                                      ==============
    protected String replaceString(String text, String fromText, String toText) {
        return SimpleStringUtil.Replace(text, fromText, toText);
    }

    protected String ln() {
        return SimpleSystemUtil.GetLineSeparator();
    }

    protected String toStringView(Object obj) {
        return TraceViewUtil.ToStringView(obj);
    }
    
    // -----------------------------------------------------
    //                                         Assert Object
    //                                         -------------
    protected void assertObjectNotNull(String variableName, Object value) {
        SimpleAssertUtil.AssertObjectNotNull(variableName, value);
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
    public String toString() {
        return ToString();
    }
}

}
