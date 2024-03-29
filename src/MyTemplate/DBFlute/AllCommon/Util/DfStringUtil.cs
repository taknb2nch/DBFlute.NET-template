
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

public class Srl {

    // ===================================================================================
    //                                                                        Null & Empty
    //                                                                        ============
    public static bool is_Null_or_Empty(String str) {
        return str == null || str.Length == 0;
    }

    public static bool is_Null_or_TrimmedEmpty(String str) {
        return str == null || str.Trim().Length == 0;
    }

    public static bool is_NotNull_and_NotEmpty(String str) {
        return !is_Null_or_Empty(str);
    }

    public static bool is_NotNull_and_NotTrimmedEmpty(String str) {
        return !is_Null_or_TrimmedEmpty(str);
    }

    public static bool isEmpty(String str) {
        return str != null && str.Length == 0;
    }

    public static bool isTrimmedEmpty(String str) {
        return str != null && str.Trim().Length == 0;
    }

    // ===================================================================================
    //                                                                              Length
    //                                                                              ======
    public static int length(String str) {
        assertStringNotNull(str);
        return str.Length;
    }

    // ===================================================================================
    //                                                                                Case
    //                                                                                ====
    public static String toLowerCase(String str) {
        assertStringNotNull(str);
        return str.ToLower();
    }

    public static String toUpperCase(String str) {
        assertStringNotNull(str);
        return str.ToUpper();
    }

    // ===================================================================================
    //                                                                                Trim
    //                                                                                ====
    public static String trim(String str) {
        assertStringNotNull(str);
        return str.Trim();
    }

    // ===================================================================================
    //                                                                             Replace
    //                                                                             =======
    public static String replace(String text, String fromText, String toText) {
        if(text == null || fromText == null || toText == null) {
            return null;
		}
		return text.Replace(fromText, toText);
    }

    // ===================================================================================
    //                                                                               Split
    //                                                                               =====
    /**
     * @param str The split target string. (NotNull)
     * @param delimiter The delimiter for split. (NotNull)
     * @return The split list. (NotNull)
     */
    public static List<String> splitList(String str, String delimiter) {
        return doSplitList(str, delimiter, false);
    }

    /**
     * @param str The split target string. (NotNull)
     * @param delimiter The delimiter for split. (NotNull)
     * @return The split list that their elements is trimmed. (NotNull)
     */
    public static List<String> splitListTrimmed(String str, String delimiter) {
        return doSplitList(str, delimiter, true);
    }

    protected static List<String> doSplitList(String str, String delimiter, bool trim) {
        assertStringNotNull(str);
        assertDelimiterNotNull(delimiter);
        List<String> list = new ArrayList<String>();
        int i = 0;
        int j = str.IndexOf(delimiter);
        for (int h = 0; j >= 0; h++) {
            String element = str.Substring(i, (j - i));
            list.add(trim ? element.Trim() : element);
            i = j + delimiter.Length;
            j = str.IndexOf(delimiter, i);
        }
        String last = str.Substring(i);
        list.add(trim ? last.Trim() : last);
        return list;
    }

    // ===================================================================================
    //                                                                             IndexOf
    //                                                                             =======
    /**
     * Get the index of the first-found delimiter.
     * <pre>
     * indexOfFirst("foo.bar/baz.qux", ".", "/")
     * returns the index of ".bar"
     * </pre>
     * @param str The target string. (NotNull)
     * @param delimiters The array of delimiters. (NotNull) 
     * @return The information of index. (NullAllowed: if delimiter not found)
     */
    public static IndexOfInfo indexOfFirst(String str, params String[] delimiters) {
        return doIndexOfFirst(false, str, delimiters);
    }

    /**
     * Get the index of the first-found delimiter ignoring case.
     * <pre>
     * indexOfFirst("foo.bar/baz.qux", "A", "U")
     * returns the index of "ar/baz..."
     * </pre>
     * @param str The target string. (NotNull)
     * @param delimiters The array of delimiters. (NotNull) 
     * @return The information of index. (NullAllowed: if delimiter not found)
     */
    public static IndexOfInfo indexOfFirstIgnoreCase(String str, params String[] delimiters) {
        return doIndexOfFirst(true, str, delimiters);
    }

    protected static IndexOfInfo doIndexOfFirst(bool ignoreCase, String str, params String[] delimiters) {
        return doIndexOf(ignoreCase, false, str, delimiters);
    }

    /**
     * Get the index of the last-found delimiter.
     * <pre>
     * indexOfLast("foo.bar/baz.qux", ".", "/")
     * returns the index of ".qux"
     * </pre>
     * @param str The target string. (NotNull)
     * @param delimiters The array of delimiters. (NotNull) 
     * @return The information of index. (NullAllowed: if delimiter not found)
     */
    public static IndexOfInfo indexOfLast(String str, params String[] delimiters) {
        return doIndexOfLast(false, str, delimiters);
    }

    /**
     * Get the index of the last-found delimiter ignoring case.
     * <pre>
     * indexOfLast("foo.bar/baz.qux", "A", "U")
     * returns the index of "ux"
     * </pre>
     * @param str The target string. (NotNull)
     * @param delimiters The array of delimiters. (NotNull) 
     * @return The information of index. (NullAllowed: if delimiter not found)
     */
    public static IndexOfInfo indexOfLastIgnoreCase(String str, params String[] delimiters) {
        return doIndexOfLast(true, str, delimiters);
    }

    protected static IndexOfInfo doIndexOfLast(bool ignoreCase, String str, params String[] delimiters) {
        return doIndexOf(ignoreCase, true, str, delimiters);
    }

    protected static IndexOfInfo doIndexOf(bool ignoreCase, bool last, String str, params String[] delimiters) {
        String filteredStr;
        if (ignoreCase) {
            filteredStr = str.ToLower();
        } else {
            filteredStr = str;
        }
        int targetIndex = -1;
        String targetDelimiter = null;
        foreach (String delimiter in delimiters) {
            String filteredDelimiter;
            if (ignoreCase) {
                filteredDelimiter = delimiter.ToLower();
            } else {
                filteredDelimiter = delimiter;
            }
            int index;
            if (last) {
                index = filteredStr.LastIndexOf(filteredDelimiter);
            } else {
                index = filteredStr.IndexOf(filteredDelimiter);
            }
            if (index < 0) {
                continue;
            }
            if (targetIndex < 0 || (last ? targetIndex < index : targetIndex > index)) {
                targetIndex = index;
                targetDelimiter = delimiter;
            }
        }
        if (targetIndex < 0) {
            return null;
        }
        IndexOfInfo info = new IndexOfInfo();
        info.setBaseString(str);
        info.setIndex(targetIndex);
        info.setDelimiter(targetDelimiter);
        return info;
    }

    // ===================================================================================
    //                                                                           SubString
    //                                                                           =========
    public static String substring(String str, int beginIndex) {
        assertStringNotNull(str);
        return str.Substring(beginIndex);
    }

    public static String substring(String str, int beginIndex, int endIndex) {
        assertStringNotNull(str);
        return str.Substring(beginIndex, endIndex - beginIndex);
    }

    // ===================================================================================
    //                                                                            Contains
    //                                                                            ========
    public static bool contains(String str, String keyword) {
        assertStringNotNull(str);
        return str.Contains(keyword);
    }

    // ===================================================================================
    //                                                                          StartsWith
    //                                                                          ==========
    public static bool startsWith(String str, String prefix) {
        assertStringNotNull(str);
        return str.StartsWith(prefix);
    }

    // ===================================================================================
    //                                                                            EndsWith
    //                                                                            ========
    public static bool endsWith(String str, String prefix) {
        assertStringNotNull(str);
        return str.EndsWith(prefix);
    }

    // ===================================================================================
    //                                                                               Count
    //                                                                               =====
    public static int count(String str, String element) {
        return doCount(str, element, false);
    }

    public static int countIgnoreCase(String str, String element) {
        return doCount(str, element, true);
    }

    protected static int doCount(String str, String element, bool ignoreCase) {
        assertStringNotNull(str);
        assertElementNotNull(element);
        int count = 0;
        if (ignoreCase) {
            str = str.ToLower();
            element = element.ToLower();
        }
        while (true) {
            int index = str.IndexOf(element);
            if (index < 0) {
                break;
            }
            str = str.Substring(index + element.Length);
            ++count;
        }
        return count;
    }

    // ===================================================================================
    //                                                                              Equals
    //                                                                              ======
    public static bool equalsIgnoreCase(String str1, String str2) {
        if (str1 != null) {
            if (str2 != null) {
                return str1.ToLower().Equals(str2.ToLower());
            } else {
                return false;
            }
        } else {
            return str2 == null; // if both are null, it means equal
        }
    }

    public static bool equalsFlexible(String str1, String str2) {
        if (str1 != null) {
            if (str2 != null) {
                str1 = replace(str1, "_", "");
                str2 = replace(str2, "_", "");
                return str1.ToLower().Equals(str2.ToLower());
            } else {
                return false;
            }
        } else {
            return str2 == null; // if both are null, it means equal
        }
    }

    public static bool equalsFlexibleTrimmed(String str1, String str2) {
        str1 = str1 != null ? str1.Trim() : null;
        str2 = str2 != null ? str2.Trim() : null;
        return equalsFlexible(str1, str2);
    }

    public static bool equalsPlain(String str1, String str2) {
        if (str1 != null) {
            if (str2 != null) {
                return str1.Equals(str2);
            } else {
                return false;
            }
        } else {
            return str2 == null; // if both are null, it means equal
        }
    }

    // ===================================================================================
    //                                                                    Initial Handling
    //                                                                    ================
    public static String initCap(String str) {
        assertStringNotNull(str);
        if (is_Null_or_Empty(str)) {
            return str;
        }
        if (length(str) == 1) {
            return str.ToUpper();
        }
        return str.Substring(0, 1).ToUpper() + str.Substring(1);
    }

    public static String initCapTrimmed(String str) {
        assertStringNotNull(str);
        str = str.Trim();
        return initCap(str);
    }

    public static String initUncap(String str) {
        assertStringNotNull(str);
        if (is_Null_or_Empty(str)) {
            return str;
        }
        if (length(str) == 1) {
            return str.ToLower();
        }
        return str.Substring(0, 1).ToLower() + str.Substring(1);
    }

    public static String initUncapTrimmed(String str) {
        assertStringNotNull(str);
        str = str.Trim();
        return initUncap(str);
    }

    // ===================================================================================
    //                                                                       Assert Helper
    //                                                                       =============
    protected static void assertStringNotNull(String str) {
        assertObjectNotNull("str", str);
    }

    protected static void assertStringListNotNull(List<String> strList) {
        assertObjectNotNull("strList", strList);
    }

    protected static void assertElementNotNull(String element) {
        assertObjectNotNull("element", element);
    }

    protected static void assertElementVaryingNotNull(String[] elements) {
        assertObjectNotNull("elements", elements);
    }

    protected static void assertKeywordNotNull(String keyword) {
        assertObjectNotNull("keyword", keyword);
    }

    protected static void assertKeywordVaryingNotNull(String[] keywords) {
        assertObjectNotNull("keywords", keywords);
    }

    protected static void assertPrefixNotNull(String prefix) {
        assertObjectNotNull("prefix", prefix);
    }

    protected static void assertSuffixNotNull(String suffix) {
        assertObjectNotNull("suffix", suffix);
    }

    protected static void assertFromToMapNotNull(Map<String, String> fromToMap) {
        assertObjectNotNull("fromToMap", fromToMap);
    }

    protected static void assertDelimiterNotNull(String delimiter) {
        assertObjectNotNull("delimiter", delimiter);
    }

    protected static void assertFromStringNotNull(String fromStr) {
        assertObjectNotNull("fromStr", fromStr);
    }

    protected static void assertToStringNotNull(String toStr) {
        assertObjectNotNull("toStr", toStr);
    }

    protected static void assertBeginMarkNotNull(String beginMark) {
        assertObjectNotNull("beginMark", beginMark);
    }

    protected static void assertEndMarkNotNull(String endMark) {
        assertObjectNotNull("endMark", endMark);
    }

    protected static void assertDecamelNameNotNull(String decamelName) {
        assertObjectNotNull("decamelName", decamelName);
    }

    protected static void assertCamelNameNotNull(String camelName) {
        assertObjectNotNull("camelName", camelName);
    }

    protected static void assertSqlNotNull(String sql) {
        assertObjectNotNull("sql", sql);
    }

    /**
     * Assert that the object is not null.
     * @param variableName Variable name. (NotNull)
     * @param value Value. (NotNull)
     * @exception IllegalArgumentException
     */
    protected static void assertObjectNotNull(String variableName, Object value) {
        if (variableName == null) {
            String msg = "The value should not be null: variableName=null value=" + value;
            throw new IllegalArgumentException(msg);
        }
        if (value == null) {
            String msg = "The value should not be null: variableName=" + variableName;
            throw new IllegalArgumentException(msg);
        }
    }

    /**
     * Assert that the entity is not null and not trimmed empty.
     * @param variableName Variable name. (NotNull)
     * @param value Value. (NotNull)
     */
    protected static void assertStringNotNullAndNotTrimmedEmpty(String variableName, String value) {
        assertObjectNotNull("variableName", variableName);
        assertObjectNotNull("value", value);
        if (value.Trim().Length == 0) {
            String msg = "The value should not be empty: variableName=" + variableName + " value=" + value;
            throw new IllegalArgumentException(msg);
        }
    }
}

    public class IndexOfInfo {
        protected String _baseString;
        protected int _index;
        protected String _delimiter;

        public String substringFront() {
            return _baseString.Substring(0, getIndex());
        }

        public String substringFrontTrimmed() {
            return substringFront().Trim();
        }

        public String substringRear() {
            return _baseString.Substring(getRearIndex());
        }

        public String substringRearTrimmed() {
            return substringRear().Trim();
        }

        public int getRearIndex() {
            return _index + _delimiter.Length;
        }

        public String getBaseString() {
            return _baseString;
        }

        public void setBaseString(String baseStr) {
            this._baseString = baseStr;
        }

        public int getIndex() {
            return _index;
        }

        public void setIndex(int index) {
            this._index = index;
        }

        public String getDelimiter() {
            return _delimiter;
        }

        public void setDelimiter(String delimiter) {
            this._delimiter = delimiter;
        }
    }
}
