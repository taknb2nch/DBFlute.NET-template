using System;
using System.Collections.Generic;
using System.Text;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Helper
{
    public class MapListStringImpl : MapListString
    {
        public  string NEW_LINE
        {
            get { return "\n"; }
        }

        public   string DEFAULT_MAP_MARK 
        {
            get
            {
                return  "map:";
            }
        }

        public   string DEFAULT_LIST_MARK 
        {
            get
            {
                return  "list:";
            }
        }

        public   string DEFAULT_DELIMITER 
        {
            get
            {
                return  ";";
            }
        }

        public   string DEFAULT_START_BRACE 
        {
            get
            {
                return  "{";
            }
        }

        public   string DEFAULT_END_BRACE 
        {
            get
            {
                return  "}";
            }
        }

        public   string DEFAULT_EQUAL 
        {
            get
            {
                return "=";
            }
        }

        /** Map-mark. */
        protected String _mapMark;

        /** List-mark. */
        protected String _listMark;

        /** Start-brace. */
        protected String _startBrace;

        /** End-brace. */
        protected String _endBrace;

        /** Delimiter. */
        protected String _delimiter;

        /** Equal. */
        protected String _equal;

        /** Remainder string. */
        protected String _topString;

        /** Remainder string. */
        protected String _remainderString;

        public MapListStringImpl()
        {
            _mapMark = DEFAULT_MAP_MARK;
            _listMark = DEFAULT_LIST_MARK;
            _startBrace = DEFAULT_START_BRACE;
            _endBrace = DEFAULT_END_BRACE;
            _delimiter = DEFAULT_DELIMITER;
            _equal = DEFAULT_EQUAL;
        }

        public String MapMark {
            get { return _mapMark; }
            set { _mapMark = value ; }
        }

        public String ListMark {
            get { return _listMark; }
            set { _listMark = value ; }
        }

        public String StartBrace {
            get { return _startBrace; }
            set { _startBrace = value ; }
        }

        public String EndBrace {
            get { return _endBrace; }
            set { _endBrace = value ; }
        }

        public String Delimiter {
            get { return _delimiter; }
            set { _delimiter = value ; }
        }

        public String Equal {
            get { return _equal; }
            set { _equal = value ; }
        }

        // ****************************************************************************************************
        //                                                                                          Main Method
        //                                                                                          ***********

        // ==========================================================================================
        //                                                                                   Generate
        //                                                                                   ========
        /**
         * Generate map from map-string. {Implement}
         * 
         * @param mapString Map-string (NotNull)
         * @return Generated map. (NotNull)
         */
        public Dictionary<String, Object> generateMap(string mapString)
        {
            assertMapString(mapString);

            _topString = mapString;
            _remainderString = mapString;

            removeBothSideSpaceAndTabAndNewLine();
            removePrefixMapMarkAndStartBrace();

            Dictionary<String, Object> generatedMap = newStringObjectMap();
            parseRemainderMapString(generatedMap);
            if (!"".Equals(_remainderString) && !_endBrace.Equals(_remainderString))
            {
                string msg = "remainderString must be empty or end-brace string:";
                msg = msg + getNewLineAndIndent() + " # remainderString --> " + _remainderString;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString;
                msg = msg + getNewLineAndIndent() + " # generatedMap --> " + generatedMap;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalStateException(msg);
            }
            return generatedMap;
        }

        /**
         * Generate map from list-string. {Implement}
         * 
         * @param listString List-string (NotNull)
         * @return Generated list. (NotNull)
         */
        public List<Object> generateList(string listString)
        {
            assertListString(listString);

            _topString = listString;
            _remainderString = listString;

            removeBothSideSpaceAndTabAndNewLine();
            removePrefixListMarkAndStartBrace();

            List<Object> generatedList = newObjectList();
            parseRemainderListString(generatedList);
            if (!"".Equals(_remainderString) && !_endBrace.Equals(_remainderString))
            {
                string msg = "rRemainderString must be empty or end-brace string:";
                msg = msg + getNewLineAndIndent() + " # remainderString --> " + _remainderString;
                msg = msg + getNewLineAndIndent() + " # listString --> " + listString;
                msg = msg + getNewLineAndIndent() + " # generatedList --> " + generatedList;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalStateException(msg);
            }
            return generatedList;
        }

        // ==========================================================================================
        //                                                                                      Parse
        //                                                                                      =====
        protected void parseRemainderMapString(Dictionary<String, Object> currentMap)
        {
            while (true)
            {
                if (initializeAtLoopBeginning())
                {
                    return;
                }

                // *** Now, _remainderString should starts with the key of the map. ***

                int equalIndex = _remainderString.IndexOf(_equal);
                assertEqualIndex(_remainderString, equalIndex, _topString, currentMap);
                string mapKey = _remainderString.Substring(0, equalIndex).Trim();
                removePrefixTargetIndexPlus(equalIndex, _equal.Length);
                removeBothSideSpaceAndTabAndNewLine();

                // *** Now, _remainderString should starts with the value of the map. ***

                if (isStartsWithMapPrefix(_remainderString))
                {
                    removePrefixMapMarkAndStartBrace();
                    parseRemainderMapString(setupNestMap(currentMap, mapKey));
                    if (closingAfterParseNestMapList())
                    {
                        return;
                    }
                    continue;
                }

                if (isStartsWithListPrefix(_remainderString))
                {
                    removePrefixListMarkAndStartBrace();
                    parseRemainderListString(setupNestList(currentMap, mapKey));
                    if (closingAfterParseNestMapList())
                    {
                        return;
                    }
                    continue;
                }

                int delimiterIndex = _remainderString.IndexOf(_delimiter);
                int endBraceIndex = _remainderString.IndexOf(_endBrace);
                assertEndBracekIndex(_remainderString, endBraceIndex, _topString, currentMap);

                // If delimiter exists and delimiter is closer than end brace, 
                // Everything from the head of the present remainder string to the delimiter becomes map value.
                //   ex) value1,key2=value2}
                if (delimiterIndex >= 0 && delimiterIndex < endBraceIndex)
                {
                    string mapValue = _remainderString.Substring(0, delimiterIndex);
                    currentMap.Add(mapKey, filterMapListValue(mapValue));

                    // Because the map element continues since the delimiter, skip the delimiter and continue the loop.
                    removePrefixTargetIndexPlus(delimiterIndex, _delimiter.Length);
                    continue;
                }

                // Everything from the head of the present remainder string to the delimiter becomes map value.
                //   ex) value1}, key2=value2}
                string mapValue2 = _remainderString.Substring(0, endBraceIndex);
                currentMap.Add(mapKey, filterMapListValue(mapValue2));

                // Analyzing map is over. So closing and return.
                closingByEndBraceIndex(endBraceIndex);
                return;
            }
        }

        protected void parseRemainderListString(List<Object> currentList)
        {
            while (true)
            {
                if (initializeAtLoopBeginning())
                {
                    return;
                }

                // *** Now, _remainderString should starts with the value of the list. ***

                if (isStartsWithMapPrefix(_remainderString))
                {
                    removePrefixMapMarkAndStartBrace();
                    parseRemainderMapString(setupNestMap(currentList));
                    if (closingAfterParseNestMapList())
                    {
                        return;
                    }
                    continue;
                }

                if (isStartsWithListPrefix(_remainderString))
                {
                    removePrefixListMarkAndStartBrace();
                    parseRemainderListString(setupNestList(currentList));
                    if (closingAfterParseNestMapList())
                    {
                        return;
                    }
                    continue;
                }

                int delimiterIndex = _remainderString.IndexOf(_delimiter);
                int endBraceIndex = _remainderString.IndexOf(_endBrace);
                assertEndBraceIndex(_remainderString, endBraceIndex, _topString, currentList);

                // If delimiter exists and delimiter is closer than end brace, 
                // Everything from the head of the present remainder string to the delimiter becomes list value.
                //   ex) value1,value2,value3}
                if (delimiterIndex >= 0 && delimiterIndex < endBraceIndex)
                {
                    string listValue = _remainderString.Substring(0, delimiterIndex);
                    currentList.Add(filterMapListValue(listValue));

                    // Because the list element continues since the delimiter, skip the delimiter and continue the loop.
                    removePrefixTargetIndexPlus(delimiterIndex, _delimiter.Length);
                    continue;
                }

                // Everything from the head of the present remainder string to the delimiter becomes list value.
                //   ex) value1}, value2, }
                string listValue2 = _remainderString.Substring(0, endBraceIndex);
                currentList.Add(filterMapListValue(listValue2));

                // Analyzing list is over. So closing and return.
                closingByEndBraceIndex(endBraceIndex);
                return;
            }
        }

        /**
         * @return Is return?
         */
        protected bool initializeAtLoopBeginning()
        {
            // Remove prefix delimiter. (Result string is always trimmed.)
            removePrefixAllDelimiter();

            // If the remainder string is empty-string, Analyzing is over!
            if (_remainderString.Equals(""))
            {
                return true;
            }

            // If the remainder string starts with end-brace, Analyzing current map is over!
            // And then remove the end-brace.
            if (isStartsWithEndBrace(_remainderString))
            {
                removePrefixEndBrace();
                return true;
            }
            return false;
        }

        /**
         * @return Is return?
         */
        protected bool closingAfterParseNestMapList()
        {
            // If the remainder string starts with end-brace, remove it and return true.
            if (isStartsWithEndBrace(_remainderString))
            {
                removePrefixEndBrace();
                return true;
            }
            return false;
        }

        protected void closingByEndBraceIndex(int endBraceIndex)
        {
            // Remove the value that was finished analyzing and end-brace.
            _remainderString = _remainderString.Substring(endBraceIndex);
            removePrefixEndBrace();
        }

        // ****************************************************************************************************
        //                                                                                      StateFul Method
        //                                                                                      ***************

        // ==========================================================================================
        //                                                                                     Remove
        //                                                                                     ======
        protected void removePrefixMapMarkAndStartBrace()
        {
            removePrefix(_mapMark + _startBrace);
        }

        protected void removePrefixListMarkAndStartBrace()
        {
            removePrefix(_listMark + _startBrace);
        }

        protected void removePrefixDelimiter()
        {
            removePrefix(_delimiter);
        }

        protected void removePrefixEndBrace()
        {
            removePrefix(_endBrace);
        }

        protected void removePrefix(String prefixString)
        {
            if (_remainderString == null)
            {
                String msg = "Argument[remainderString] must not be null: " + _remainderString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            removeBothSideSpaceAndTabAndNewLine();
            if (prefixString == null)
            {
                String msg = "Argument[prefixString] must not be null: " + prefixString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            if (_remainderString.Length < prefixString.Length)
            {
                String msg = "Argument[remainderString] length must be larger than Argument[prefixString] length:";
                msg = msg + getNewLineAndIndent() + " # remainderString --> " + _remainderString;
                msg = msg + getNewLineAndIndent() + " # prefixString=" + prefixString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            if (!_remainderString.StartsWith(prefixString))
            {
                String msg = "Argument[remainderString] must start with Argument[prefixString:]";
                msg = msg + getNewLineAndIndent() + " # remainderString --> " + _remainderString;
                msg = msg + getNewLineAndIndent() + " # prefixString --> " + prefixString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            
            _remainderString = _remainderString.Substring(prefixString.Length);
            removeBothSideSpaceAndTabAndNewLine();
        }

        protected void removePrefixAllDelimiter()
        {
            removeBothSideSpaceAndTabAndNewLine();

            while (true)
            {
                if (!isStartsWithDelimiter(_remainderString))
                {
                    break;
                }

                if (isStartsWithDelimiter(_remainderString))
                {
                    removePrefixDelimiter();
                    removeBothSideSpaceAndTabAndNewLine();
                }
            }
        }

        protected void removeBothSideSpaceAndTabAndNewLine()
        {
            _remainderString = _remainderString.Trim();
        }

        protected void removePrefixTargetIndexPlus(int index, int plusCount)
        {
            _remainderString = _remainderString.Substring(index + plusCount);
        }

        // ****************************************************************************************************
        //                                                                                     StateLess Method
        //                                                                                     ****************

        // ==========================================================================================
        //                                                                                     Assert
        //                                                                                     ======
        protected void assertMapString(String mapString)
        {
            if (mapString == null)
            {
                String msg = "Argument[mapString] must not be null: ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "mapString=" + mapString);
            }
            mapString = mapString.Trim();
            if (!isStartsWithMapPrefix(mapString))
            {
                String msg = "Argument[mapString] must start with '" + _mapMark + _startBrace + "': ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "mapString=" + mapString);
            }
            if (!isEndsWithEndMark(mapString))
            {
                String msg = "Argument[mapString] must end with '" + _endBrace + "': ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "mapString=" + mapString);
            }

            int startBraceCount = getDelimiterCount(mapString, _startBrace);
            int endBraceCount = getDelimiterCount(mapString, _endBrace);
            if (startBraceCount != endBraceCount)
            {
                String msg = "It is necessary to have braces of the same number on start and end:";
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString;
                msg = msg + getNewLineAndIndent() + " # startBraceCount --> " + startBraceCount;
                msg = msg + getNewLineAndIndent() + " # endBraceCount --> " + endBraceCount;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
        }

        protected void assertListString(String listString)
        {
            if (listString == null)
            {
                String msg = "Argument[listString] must not be null: ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "liststring=" + listString);
            }
            listString = listString.Trim();
            if (!isStartsWithListPrefix(listString))
            {
                string msg = "Argument[listString] must start with '" + _mapMark + "': ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "listString=" + listString);
            }
            if (!isEndsWithEndMark(listString))
            {
                string msg = "Argument[listString] must end with '" + _endBrace + "': ";
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg + "listString=" + listString);
            }

            int startBraceCount = getDelimiterCount(listString, _startBrace);
            int endBraceCount = getDelimiterCount(listString, _endBrace);
            if (startBraceCount != endBraceCount)
            {
                string msg = "It is necessary to have braces of the same number on start and end:";
                msg = msg + getNewLineAndIndent() + " # listString --> " + listString;
                msg = msg + getNewLineAndIndent() + " # startBraceCount --> " + startBraceCount;
                msg = msg + getNewLineAndIndent() + " # endBraceCount --> " + endBraceCount;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
        }

        protected void assertEqualIndex(string remainderMapString, int equalIndex, string mapString4Log, Dictionary<string,object> currentMap4Log)
        {
            if (remainderMapString == null)
            {
                string msg = "Argument[remainderMapString] must not be null:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # equalIndex --> " + equalIndex;
                msg = msg + getNewLineAndIndent() + " # mapString4Log --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap4Log --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (equalIndex < 0)
            {
                string msg = "Argument[equalIndex] must be plus or zero:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # equalIndex --> " + equalIndex;
                msg = msg + getNewLineAndIndent() + " # mapString4Log --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap4Log --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (remainderMapString.Length < equalIndex)
            {
                string msg = "Argument[remainderMapString] length must be larger than equalIndex value:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # equalIndex --> " + equalIndex;
                msg = msg + getNewLineAndIndent() + " # mapString4Log --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap4Log --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            String expectedAsEndMark = remainderMapString.Substring(equalIndex, _equal.Length);
            if (!expectedAsEndMark.Equals(_equal))
            {
                string msg = "Argument[remainderMapString] must have '" + _equal + "' at Argument[equalIndex]:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # equalIndex --> " + equalIndex;
                msg = msg + getNewLineAndIndent() + " # expectedAsEndMark --> " + expectedAsEndMark;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
        }

        protected void assertEndBracekIndex(string remainderMapString, int endBraceIndex, string mapString4Log,
                Dictionary<string, object> currentMap4Log)
        {
            if (remainderMapString == null)
            {
                string msg = "Argument[remainderMapString] must not be null:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (endBraceIndex < 0)
            {
                string msg = "Argument[endMarkIndex] must be plus or zero:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> =" + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (remainderMapString.Length < endBraceIndex)
            {
                string msg = "Argument[remainderMapString] length must be larger than endMarkIndex value:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            String expectedAsEndMark = remainderMapString.Substring(endBraceIndex, _endBrace.Length);
            if (!expectedAsEndMark.Equals(_endBrace))
            {
                string msg = "Argument[remainderMapString] must have '" + _endBrace + "' at Argument[endBraceIndex]:";
                msg = msg + getNewLineAndIndent() + " # remainderMapString --> " + remainderMapString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # expectedAsEndMark --> " + expectedAsEndMark;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentMap --> " + currentMap4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
        }

        protected void assertEndBraceIndex(string remainderListString, int endBraceIndex, string mapString4Log,
                List<Object> currentList4Log)
        {
            if (remainderListString == null)
            {
                string msg = "Argument[remainderMapString] must not be null:";
                msg = msg + getNewLineAndIndent() + " # remainderListString --> " + remainderListString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentList --> " + currentList4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (endBraceIndex < 0)
            {
                string msg = "Argument[endMarkIndex] must be plus or zero:";
                msg = msg + getNewLineAndIndent() + " # remainderListString --> " + remainderListString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentList --> " + currentList4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            if (remainderListString.Length < endBraceIndex)
            {
                string msg = "Argument[remainderMapString] length must be larger than endMarkIndex value:";
                msg = msg + getNewLineAndIndent() + " # remainderListString --> " + remainderListString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentList --> " + currentList4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }

            String expectedAsEndBrace = remainderListString.Substring(endBraceIndex,  _endBrace.Length);
            if (!expectedAsEndBrace.Equals(_endBrace))
            {
                string msg = "Argument[remainderMapString] must have '" + _endBrace + "' at Argument[endBraceIndex]:";
                msg = msg + getNewLineAndIndent() + " # remainderListString --> " + remainderListString;
                msg = msg + getNewLineAndIndent() + " # endBraceIndex --> " + endBraceIndex;
                msg = msg + getNewLineAndIndent() + " # expectedAsEndBrace --> " + expectedAsEndBrace;
                msg = msg + getNewLineAndIndent() + " # mapString --> " + mapString4Log;
                msg = msg + getNewLineAndIndent() + " # currentList --> " + currentList4Log;
                msg = msg + getNewLineAndIndent() + " # _startBrace --> " + _startBrace;
                msg = msg + getNewLineAndIndent() + " # _endBrace --> " + _endBrace;
                msg = msg + getNewLineAndIndent() + " # _delimiter --> " + _delimiter;
                msg = msg + getNewLineAndIndent() + " # _equal --> " + _equal;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
        }

        // ==========================================================================================
        //                                                                                     Filter
        //                                                                                     ======
        /**
         * Filter map or list value.
         * <p>
         * <pre>
         * # The value is trimmed.
         * # If the value is null, this returns null.
         * # If the value is 'null', this returns null.
         * # If the trimmed value is empty string, this returns null.
         * </pre>
         * @param value value. (NullAllowed)
         * @return Filtered value. (NullAllowed)
         */
        protected String filterMapListValue(string value)
        {
            if (value == null)
            {
                return null;
            }
            value = value.Trim();
            return (("".Equals(value) || "null".Equals(value)) ? null : value);
        }

        // ==========================================================================================
        //                                                                                  Judgement
        //                                                                                  =========
        protected bool isStartsWithMapPrefix(string targetString)
        {
            if (targetString == null)
            {
                string msg = "Argument[targetString] must not be null: " + targetString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            targetString = targetString.Trim();
            if (targetString.StartsWith(_mapMark + _startBrace))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool isStartsWithListPrefix(string targetString)
        {
            if (targetString == null)
            {
                string msg = "Argument[targetString] must not be null: " + targetString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            targetString = targetString.Trim();
            if (targetString.StartsWith(_listMark + _startBrace))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool isStartsWithDelimiter(string targetString)
        {
            if (targetString == null)
            {
                string msg = "Argument[targetString] must not be null: " + targetString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            targetString = targetString.Trim();
            if (targetString.StartsWith(_delimiter))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool isStartsWithEndBrace(string targetString)
        {
            if (targetString == null)
            {
                string msg = "Argument[targetString] must not be null: " + targetString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            targetString = targetString.Trim();
            if (targetString.StartsWith(_endBrace))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool isEndsWithEndMark(string targetString)
        {
            if (targetString == null)
            {
                string msg = "Argument[targetString] must not be null: " + targetString;
                throw new Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike.IllegalArgumentException(msg);
            }
            targetString = targetString.Trim();
            if (targetString.EndsWith(_endBrace))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ==========================================================================================
        //                                                                                      Other
        //                                                                                      =====
        protected Dictionary<String, Object> setupNestMap(Dictionary<String, Object> currentMap, string mapKey)
        {
            Dictionary<String, Object> nestMap = newStringObjectMap();
            currentMap.Add(mapKey, nestMap);
            return nestMap;
        }

        protected Dictionary<String, Object> setupNestMap(List<Object> currentList)
        {
            Dictionary<String, Object> nestMap = newStringObjectMap();
            currentList.Add(nestMap);
            return nestMap;
        }

        protected List<Object> setupNestList(Dictionary<String, Object> currentMap, string mapKey)
        {
            List<Object> nestList = newObjectList();
            currentMap.Add(mapKey, nestList);
            return nestList;
        }

        protected List<Object> setupNestList(List<Object> currentList)
        {
            List<Object> nestList = newObjectList();
            currentList.Add(nestList);
            return nestList;
        }

        protected Dictionary<String, Object> newStringObjectMap()
        {
            return new Dictionary<String, Object>();
        }

        protected List<Object> newObjectList()
        {
            return new List<Object>();
        }

        protected String getNewLineAndIndent()
        {
            return NEW_LINE + "    ";
        }

        /**
         * Get count that target string exist in the base string.
         * 
         * @param targetString �Ώە�����
         * @param delimiter �f���~�^
         * @return �c��̕�����Ɋ܂܂�Ă���f���~�^�̐�
         */
        protected int getDelimiterCount(string targetString, string delimiter)
        {
            int result = 0;
            for (int i = 0; ; )
            {
                if (targetString.IndexOf(delimiter, i) != -1)
                {
                    result++;
                    i = targetString.IndexOf(delimiter, i) + 1;
                }
                else
                {
                    break;
                }
            }
            if (result == 0)
            {
                result = -1;
            }
            return result;
        }

    }
}
