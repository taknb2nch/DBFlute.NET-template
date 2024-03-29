
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlParser {

    public enum InternalTokenType {
        SQL = 0,
        COMMENT = 1,
        ELSE = 2,
        BIND_VARIABLE = 3,
        EOF = 4,
    }
	
    public class InternalSqlTokenizer {
	
        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected readonly string _sql;
        protected int _position = 0;
        protected string _token;
        protected InternalTokenType _tokenType = InternalTokenType.SQL;
        protected InternalTokenType _nextTokenType = InternalTokenType.SQL;
        protected int _bindVariableNum = 0;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalSqlTokenizer(string sql) {
            _sql = sql;
        }

        // ===============================================================================
        //                                                                        Tokenize
        //                                                                        ========
        public InternalTokenType Next()
        {
            if (_position >= _sql.Length)
            {
                _token = null;
                _tokenType = InternalTokenType.EOF;
                _nextTokenType = InternalTokenType.EOF;
                return _tokenType;
            }
            switch (_nextTokenType)
            {
                case InternalTokenType.SQL:
                    ParseSql();
                    break;
                case InternalTokenType.COMMENT:
                    ParseComment();
                    break;
                case InternalTokenType.ELSE:
                    ParseElse();
                    break;
                case InternalTokenType.BIND_VARIABLE:
                    ParseBindVariable();
                    break;
                default:
                    ParseEof();
                    break;
            }
            return _tokenType;
        }

        public string SkipToken()
        {
            int index = _sql.Length;
            char quote = _position < _sql.Length ? _sql.ToCharArray()[_position] : '\0';
            bool quoting = quote == '\'' || quote == '(';
            if (quote == '(') quote = ')';

            for (int i = quoting ? _position + 1 : _position; i < _sql.Length; ++i)
            {
                char c = _sql.ToCharArray()[i];
                if ((Char.IsWhiteSpace(c) || c == ',' || c == ')' || c == '(')
                    && !quoting)
                {
                    index = i;
                    break;
                }
                else if (c == '/' && i + 1 < _sql.Length
                    && _sql.ToCharArray()[i + 1] == '*')
                {
                    index = i;
                    break;
                }
                else if (c == '-' && i + 1 < _sql.Length
                    && _sql.ToCharArray()[i + 1] == '-')
                {
                    index = i;
                    break;
                }
                else if (quoting && quote == '\'' && c == '\''
                    && (i + 1 >= _sql.Length || _sql.ToCharArray()[i + 1] != '\''))
                {
                    index = i + 1;
                    break;
                }
                else if (quoting && c == quote)
                {
                    index = i + 1;
                    break;
                }
            }
            _token = _sql.Substring(_position, (index - _position));
            _tokenType = InternalTokenType.SQL;
            _nextTokenType = InternalTokenType.SQL;
            _position = index;
            return _token;
        }

        public string SkipWhitespace()
        {
            int index = SkipWhitespace(_position);
            _token = _sql.Substring(_position, (index - _position));
            _position = index;
            return _token;
        }

        protected void ParseSql()
        {
            int commentStartPos = _sql.IndexOf("/*", _position);
            int lineCommentStartPos = _sql.IndexOf("--", _position);
            int bindVariableStartPos = _sql.IndexOf("?", _position);
            int elseCommentStartPos = -1;
            int elseCommentLength = -1;

            if (bindVariableStartPos < 0)
            {
                bindVariableStartPos = _sql.IndexOf("?", _position);
            }
            if (lineCommentStartPos >= 0)
            {
                int skipPos = SkipWhitespace(lineCommentStartPos + 2);
                if (skipPos + 4 < _sql.Length
                    && "ELSE" == _sql.Substring(skipPos, ((skipPos + 4) - skipPos)))
                {
                    elseCommentStartPos = lineCommentStartPos;
                    elseCommentLength = skipPos + 4 - lineCommentStartPos;
                }
            }
            int nextStartPos = GetNextStartPos(commentStartPos,
                elseCommentStartPos, bindVariableStartPos);
            if (nextStartPos < 0)
            {
                _token = _sql.Substring(_position);
                _nextTokenType = InternalTokenType.EOF;
                _position = _sql.Length;
                _tokenType = InternalTokenType.SQL;
            }
            else
            {
                _token = _sql.Substring(_position, nextStartPos - _position);
                _tokenType = InternalTokenType.SQL;
                bool needNext = nextStartPos == _position;
                if (nextStartPos == commentStartPos)
                {
                    _nextTokenType = InternalTokenType.COMMENT;
                    _position = commentStartPos + 2;
                }
                else if (nextStartPos == elseCommentStartPos)
                {
                    _nextTokenType = InternalTokenType.ELSE;
                    _position = elseCommentStartPos + elseCommentLength;
                }
                else if (nextStartPos == bindVariableStartPos)
                {
                    _nextTokenType = InternalTokenType.BIND_VARIABLE;
                    _position = bindVariableStartPos;
                }
                if (needNext) Next();
            }
        }

        protected int GetNextStartPos(int commentStartPos, int elseCommentStartPos,
            int bindVariableStartPos)
        {
            int nextStartPos = -1;
            if (commentStartPos >= 0)
                nextStartPos = commentStartPos;

            if (elseCommentStartPos >= 0
                && (nextStartPos < 0 || elseCommentStartPos < nextStartPos))
                nextStartPos = elseCommentStartPos;

            if (bindVariableStartPos >= 0
                && (nextStartPos < 0 || bindVariableStartPos < nextStartPos))
                nextStartPos = bindVariableStartPos;

            return nextStartPos;
        }

        protected string NextBindVariableName {
            get { return "$" + ++_bindVariableNum; }
        }

        protected void ParseComment() {
            int commentEndPos = _sql.IndexOf("*/", _position);
            if (commentEndPos < 0) {
				ThrowEndCommentNotFoundException(_sql.Substring(_position));
			}
            _token = _sql.Substring(_position, (commentEndPos - _position));
            _nextTokenType = InternalTokenType.SQL;
            _position = commentEndPos + 2;
            _tokenType = InternalTokenType.COMMENT;
        }


        protected void ThrowEndCommentNotFoundException(String expression) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The end comment was Not Found!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the parameter comment logic." + GetLineSeparator();
            msg = msg + "It may exist the parameter comment that DOESN'T have an end comment." + GetLineSeparator();
            msg = msg + "  For example:" + GetLineSeparator();
            msg = msg + "    before (x) -- /*IF pmb.xxxId != null*/XXX_ID = /*pmb.xxxId*/3" + GetLineSeparator();
            msg = msg + "    after  (o) -- /*IF pmb.xxxId != null*/XXX_ID = /*pmb.xxxId*/3/*END*/" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[End Comment Expected Place]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + _sql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */" + GetLineSeparator();
            throw new EndCommentNotFoundException(msg);
        }

        protected void ParseBindVariable()
        {
            _token = NextBindVariableName;
            _nextTokenType = InternalTokenType.SQL;
            _position++;
            _tokenType = InternalTokenType.BIND_VARIABLE;
        }

        protected void ParseElse()
        {
            _token = null;
            _nextTokenType = InternalTokenType.SQL;
            _tokenType = InternalTokenType.ELSE;
        }

        protected void ParseEof()
        {
            _token = null;
            _tokenType = InternalTokenType.EOF;
            _nextTokenType = InternalTokenType.EOF;
        }

        private int SkipWhitespace(int position)
        {
            int index = _sql.Length;
            for (int i = position; i < _sql.Length; ++i)
            {
                char c = _sql.ToCharArray()[i];
                if (!Char.IsWhiteSpace(c))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
		

        // ===============================================================================
        //                                                                  General Helper
        //                                                                  ==============
        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public string Token
        {
            get { return _token; }
        }

        public string Before
        {
            get { return _sql.Substring(0, _position); }
        }

        public string After
        {
            get { return _sql.Substring(_position); }
        }

        public int Position
        {
            get { return _position; }
        }

        public InternalTokenType TokenType
        {
            get { return _tokenType; }
        }

        public InternalTokenType NextTokenType
        {
            get { return NextTokenType; }
        }
    }
}
