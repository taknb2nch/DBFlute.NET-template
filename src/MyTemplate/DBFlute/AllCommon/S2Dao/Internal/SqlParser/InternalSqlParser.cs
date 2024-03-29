
using System;
using System.Reflection;
using System.Collections.Generic;

using Seasar.Extension.ADO;
using Seasar.Extension.ADO.Impl;
using Seasar.Framework.Util;
using Seasar.Dao;
using Seasar.Dao.Node;
using Seasar.Dao.Parser;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.OutsideSql;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.Util;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Util;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlParser {

    public class InternalSqlParser : ISqlParser {
	
        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected String _specifiedSql;
        protected bool _blockNullParameter;
        protected readonly InternalSqlTokenizer _tokenizer;
        protected readonly System.Collections.Stack _nodeStack = new System.Collections.Stack();

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public InternalSqlParser(String sql, bool blockNullParameter) {
            sql = sql.Trim();
            if (sql.EndsWith(";")) {
                sql = sql.Substring(0, sql.Length - 1);
            }
            _specifiedSql = sql;
            _blockNullParameter = blockNullParameter;
            _tokenizer = new InternalSqlTokenizer(sql);
        }

        // ===============================================================================
        //                                                                           Parse
        //                                                                           =====
        public INode Parse() {
            Push(new ContainerNode());
            while (InternalTokenType.EOF != _tokenizer.Next()) {
                ParseToken();
            }
            return Pop();
        }

        protected void ParseToken() {
            switch (_tokenizer.TokenType) {
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
            }
        }

        protected void ParseSql() {
            string sql = _tokenizer.Token;
            if (IsElseMode()) {
                sql = sql.Replace("--", string.Empty);
            }
            INode node = Peek();

            if ((node is InternalIfNode || node is ElseNode) && node.ChildSize == 0) {
                InternalSqlTokenizer st = new InternalSqlTokenizer(sql);
                st.SkipWhitespace();
                string token = st.SkipToken();
                st.SkipWhitespace();
                if ("AND".Equals(token.ToUpper()) || "OR".Equals(token.ToUpper())) {
                    node.AddChild(new PrefixSqlNode(st.Before, st.After));
                } else {
                    node.AddChild(new SqlNode(sql));
                }
            } else {
                node.AddChild(new SqlNode(sql));
            }
        }

        protected void ParseComment() {
            string comment = _tokenizer.Token;
            if (IsTargetComment(comment)) {
                if (IsIfComment(comment)) {
                    ParseIf();
                } else if (IsBeginComment(comment)) {
                    ParseBegin();
                } else if (IsEndComment(comment)) {
                    return;
                } else {
                    ParseCommentBindVariable();
                }
            } else if (comment != null && 0 < comment.Length) {
    			// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    	        // [UnderReview]: Should I resolve bind character on scope comment(normal comment)?
    		    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				String before = _tokenizer.Before; // for Hint Clause of Database
				Peek().AddChild(new SqlNode(before.Substring(before.LastIndexOf("/*"))));
			}
        }

        protected void ParseIf() {
            string condition = _tokenizer.Token.Substring(2).Trim();
            if (StringUtil.IsEmpty(condition)) {
                ThrowIfCommentConditionNotFoundException();
            }
            ContainerNode ifNode = CreateIfNode(condition);
            Peek().AddChild(ifNode);
            Push(ifNode);
            ParseEnd();
        }

        protected void ThrowIfCommentConditionNotFoundException() {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The condition of IF comment was Not Found!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the IF comment expression." + GetLineSeparator();
            msg = msg + "It may exist the IF comment that DOESN'T have a condition." + GetLineSeparator();
            msg = msg + "  For example:" + GetLineSeparator();
            msg = msg + "    before (x) -- /*IF*/XXX_ID = /*pmb.XxxId*/3/*END*/" + GetLineSeparator();
            msg = msg + "    after  (o) -- /*IF pmb.XxxId != null*/XXX_ID = /*pmb.XxxId*/3/*END*/" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[IF Comment Expression]" + GetLineSeparator() + _tokenizer.Token + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + _specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new IfCommentConditionNotFoundException(msg);
        }

        protected void ParseBegin() {
            BeginNode beginNode = new BeginNode();
            Peek().AddChild(beginNode);
            Push(beginNode);
            ParseEnd();
        }

        protected void ParseEnd() {
            while (InternalTokenType.EOF != _tokenizer.Next()) {
                if (_tokenizer.TokenType == InternalTokenType.COMMENT
                    && IsEndComment(_tokenizer.Token)) {
                    Pop();
                    return;
                }
                ParseToken();
            }
            ThrowEndCommentNotFoundException();
        }

        protected void ThrowEndCommentNotFoundException() {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The end comment was Not Found!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the parameter comment logic." + GetLineSeparator();
            msg = msg + "It may exist the parameter comment that DOESN'T have an end comment." + GetLineSeparator();
            msg = msg + "  For example:" + GetLineSeparator();
            msg = msg + "    before (x) -- /*IF pmb.XxxId != null*/XXX_ID = /*pmb.XxxId*/3" + GetLineSeparator();
            msg = msg + "    after  (o) -- /*IF pmb.XxxId != null*/XXX_ID = /*pmb.XxxId*/3/*END*/" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + _specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new EndCommentNotFoundException(msg);
        }
	
        protected void ParseElse() {
            INode parent = Peek();
            if (!(parent is InternalIfNode)) {
                return;
            }
            InternalIfNode ifNode = (InternalIfNode) Pop();
            ElseNode elseNode = new ElseNode();
            ifNode.ElseNode = elseNode;
            Push(elseNode);
            _tokenizer.SkipWhitespace();
        }

        protected void ParseCommentBindVariable() {
            string expr = _tokenizer.Token;
            string s = _tokenizer.SkipToken();
            if (expr.StartsWith("$")) {
                Peek().AddChild(CreateEmbeddedValueNode(expr.Substring(1), s));
            } else {
                Peek().AddChild(CreateBindVariableNode(expr, s));
            }
        }

        protected void ParseBindVariable() {
            string expr = _tokenizer.Token;
            Peek().AddChild(CreateBindVariableNode(expr, null));
        }

        protected AbstractNode CreateBindVariableNode(String expr, String textValue) {
            return new InternalBindVariableNode(expr, textValue, _specifiedSql, _blockNullParameter);
        }

        protected AbstractNode CreateEmbeddedValueNode(string expr, String textValue) {
            return new InternalEmbeddedValueNode(expr, textValue, _specifiedSql, _blockNullParameter);
        }

        protected ContainerNode CreateIfNode(string expr) {
            return new InternalIfNode(expr, _specifiedSql);
        }

        protected INode Pop() {
            return (INode) _nodeStack.Pop();
        }

        protected INode Peek() {
            return (INode) _nodeStack.Peek();
        }

        protected void Push(INode node) {
            _nodeStack.Push(node);
        }

        protected bool IsElseMode() {
            for (int i = 0; i < _nodeStack.Count; ++i) {
                if (_nodeStack.ToArray()[i] is ElseNode) {
                    return true;
                }
            }
            return false;
        }

        private static bool IsTargetComment(string comment) {
            return comment != null && comment.Length > 0
                && IsCSharpIdentifierStart(comment.ToCharArray()[0]);
        }

        private static bool IsCSharpIdentifierStart(Char c) {
            return Char.IsLetterOrDigit(c) || c == '_' || c == '\\' || c == '$' || c == '@';
        }

        private static bool IsIfComment(string comment) {
            return comment.StartsWith("IF");
        }

        private static bool IsBeginComment(string content) {
            return content != null && "BEGIN".Equals(content);
        }

        private static bool IsEndComment(string content) {
            return content != null && "END".Equals(content);
        }
		
	
    	// ===============================================================================
        //                                                                         Convert
        //                                                                         =======
    	public static String ConvertTwoWaySql2DisplaySql(String twoWaySql, Object arg) {
    	    String[] argNames = new String[]{"pmb"};
    	    Type[] argTypes = new Type[]{arg.GetType()};
    	    Object[] args = new Object[]{arg};
    		return ConvertTwoWaySql2DisplaySql(twoWaySql, argNames, argTypes, args);
    	}
    	
    	public static String ConvertTwoWaySql2DisplaySql(String twoWaySql, String[] argNames, Type[] argTypes, Object[] args) {
            ICommandContext context;
    		{
                InternalSqlParser parser = new InternalSqlParser(twoWaySql, false);
                INode node = parser.Parse();
                InternalCommandContextCreator creator = new InternalCommandContextCreator(argNames, argTypes);
                context = creator.CreateCommandContext(args);
                node.Accept(context);
    		}
            String preparedSql = context.Sql;
    		return InternalBindVariableUtil.GetCompleteSql(preparedSql, context.BindVariables);
    	}
	
        // ===============================================================================
        //                                                                  General Helper
        //                                                                  ==============
        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }
    }

    // -----------------------------------------------------
    //                                      BindVariableNode
    //                                      ----------------
    public class InternalBindVariableNode : AbstractNode {
        protected String _expression;
        protected String _testValue;
        protected String[] _names;
        protected String _specifiedSql;
        protected bool _blockNullParameter;

        public InternalBindVariableNode(String expression, String testValue
                                      , String specifiedSql, bool blockNullParameter) : base () {
            _expression = expression;
            _testValue = testValue;
            _names = expression.Split('.');
            _specifiedSql = specifiedSql;
            _blockNullParameter = blockNullParameter;
        }

        public override void Accept(ICommandContext ctx) {
            object value = ctx.GetArg(_names[0]);
            Type type = (value != null ? value.GetType() : null);
            InternalValueAndType valueAndType = new InternalValueAndType();
            valueAndType.TargetValue = value;
            valueAndType.TargetType = type;
            SetupValueAndType(valueAndType);

			if (_blockNullParameter && valueAndType.TargetValue == null) {
			    ThrowBindOrEmbeddedParameterNullValueException(valueAndType);
			}
			if (!IsInScope()) {
			    // Main Root
				ctx.AddSql(valueAndType.TargetValue, valueAndType.TargetType, _expression.Replace('.', '_'));
			} else {
                if (typeof(System.Collections.IList).IsAssignableFrom(valueAndType.TargetType)) {
                    System.Collections.IList list = valueAndType.TargetValue as System.Collections.IList;
                    Array array = new Object[list.Count];
                    list.CopyTo(array, 0);
                    BindArray(ctx, array);
                } else if (valueAndType.TargetType.IsArray) {
                    BindArray(ctx, valueAndType.TargetValue);
                } else {
                    ctx.AddSql(valueAndType.TargetValue, valueAndType.TargetType, _expression.Replace('.', '_'));
                }
			}
            if (valueAndType.IsValidRearOption()) {
                ctx.AddSql(valueAndType.BuildRearOptionOnSql());
            }
        }

        protected void SetupValueAndType(InternalValueAndType valueAndType) {
            InternalValueAndTypeSetuper setupper = new InternalValueAndTypeSetuper(_expression, _names, _specifiedSql, true);
            setupper.SetupValueAndType(valueAndType);
        }
		
		protected void ThrowBindOrEmbeddedParameterNullValueException(InternalValueAndType valueAndType) {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterNullValueException(_expression, valueAndType.TargetType, _specifiedSql, true);
		}
		
		protected bool IsInScope() {
		    return _testValue != null && _testValue.StartsWith("(") && _testValue.EndsWith(")");
		}
		
        protected void BindArray(ICommandContext ctx, object arrayArg) {
            if (arrayArg == null) {
                return;
            }
            object[] array = arrayArg as object[];
            int length = array.Length;
            if (length == 0) {
			    ThrowBindOrEmbeddedParameterEmptyListException();
			}
            Type type = null;
            for (int i = 0; i < length; ++i) {
                Object currentElement = array[i];
                if (currentElement != null) {
				    type = currentElement.GetType();
					break;
				}
            }
			if (type == null) {
			    ThrowBindOrEmbeddedParameterNullOnlyListException();
			}
            ctx.AddSql("(");
			int bindCount = 0;
            for (int i = 0; i < length; ++i) {
				Object currentElement = array[i];
				if (currentElement != null) {
					++bindCount;
				    if (bindCount == 1) {// FirstElement!
					    ctx.AddSql(currentElement, type, _expression + bindCount);
					} else {
					    ctx.AppendSql(currentElement, type, _expression + bindCount);
					}
				}
            }
            ctx.AddSql(")");
        }
		
		protected void ThrowBindOrEmbeddedParameterEmptyListException() {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterEmptyListException(_expression, _specifiedSql, true);
		}
		
		protected void ThrowBindOrEmbeddedParameterNullOnlyListException() {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterNullOnlyListException(_expression, _specifiedSql, true);
		}
    }

    // -----------------------------------------------------
    //                                     EmbeddedValueNode
    //                                     -----------------
    public class InternalEmbeddedValueNode : AbstractNode {
        protected String _expression;
		protected String _testValue;
        protected String[] _names;
        protected String _specifiedSql;
        protected bool _blockNullParameter;

        public InternalEmbeddedValueNode(String expression, String testValue
                                       , String specifiedSql, bool blockNullParameter) : base () {
            _expression = expression;
            _testValue = testValue;
            _names = expression.Split('.');
            _specifiedSql = specifiedSql;
            _blockNullParameter = blockNullParameter;
        }

        public override void Accept(ICommandContext ctx) {
            Object value = ctx.GetArg(_names[0]);
            Type type = (value != null ? value.GetType() : null);
            InternalValueAndType valueAndType = new InternalValueAndType();
            valueAndType.TargetValue = value;
            valueAndType.TargetType = type;
            SetupValueAndType(valueAndType);
			
			if (_blockNullParameter && valueAndType.TargetValue == null) {
			    ThrowBindOrEmbeddedParameterNullValueException(valueAndType);
			}
			if (!IsInScope()) {
			    // Main Root
		        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	            // [UnderReview]: Should I make an original exception instead of this exception?
		        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                // if (valueAndType.TargetValue != null && valueAndType.TargetValue.ToString().IndexOf("?") > -1) {
                //     throw new org.seasar.framework.exception.SRuntimeException("EDAO0023");
                // }
                ctx.AddSql(valueAndType.TargetValue.ToString());
			} else {
                if (IsInScope() && typeof(System.Collections.IList).IsAssignableFrom(valueAndType.TargetType)) {
                    System.Collections.IList list = valueAndType.TargetValue as System.Collections.IList;
                    Array array = new Object[list.Count];
                    list.CopyTo(array, 0);
                    EmbedArray(ctx, array);
                } else if (IsInScope() && valueAndType.TargetType.IsArray) {
                    EmbedArray(ctx, valueAndType.TargetValue);
                } else {
    		        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    	            // [UnderReview]: Should I make an original exception instead of this exception?
    		        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                    // if (valueAndType.TargetValue != null && valueAndType.TargetValue.ToString().IndexOf("?") > -1) {
                    //     throw new org.seasar.framework.exception.SRuntimeException("EDAO0023");
                    // }
                    ctx.AddSql(valueAndType.TargetValue.ToString());
                }
			}
            if (valueAndType.IsValidRearOption()) {
                ctx.AddSql(valueAndType.BuildRearOptionOnSql());
            }
        }

        protected void SetupValueAndType(InternalValueAndType valueAndType) {
            InternalValueAndTypeSetuper setupper = new InternalValueAndTypeSetuper(_expression, _names, _specifiedSql, false);
            setupper.SetupValueAndType(valueAndType);
        }
		
		protected void ThrowBindOrEmbeddedParameterNullValueException(InternalValueAndType valueAndType) {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterNullValueException(_expression, valueAndType.TargetType, _specifiedSql, false);
		}
		
		protected bool IsInScope() {
		    return _testValue != null && _testValue.StartsWith("(") && _testValue.EndsWith(")");
		}
		
        protected void EmbedArray(ICommandContext ctx, object arrayArg) {
            if (arrayArg == null) {
                return;
            }
            object[] array = arrayArg as object[];
            int length = array.Length;
            if (length == 0) {
			    ThrowBindOrEmbeddedParameterEmptyListException();
			}
            String quote = null;
            for (int i = 0; i < length; ++i) {
                Object currentElement = array[i];
                if (currentElement != null) {
				    quote = !IsNumeric(currentElement) ? "'" : "";
					break;
				}
            }
			if (quote == null) {
			    ThrowBindOrEmbeddedParameterNullOnlyListException();
			}
			bool existsValidElements = false;
            ctx.AddSql("(");
            for (int i = 0; i < length; ++i) {
				Object currentElement = array[i];
				if (currentElement != null) {
					if (!existsValidElements) {
                        ctx.AddSql(quote + currentElement + quote);
				        existsValidElements = true;
					} else {
					    ctx.AddSql(", " + quote + currentElement + quote);
					}
				}
            }
            ctx.AddSql(")");
        }

        protected bool IsNumeric(object Expression) {
            // Variable to collect the Return value of the TryParse method.
            bool isNum;

            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            double retNum;

            // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
            // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
		
		protected void ThrowBindOrEmbeddedParameterEmptyListException() {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterEmptyListException(_expression, _specifiedSql, false);
		}
		
		protected void ThrowBindOrEmbeddedParameterNullOnlyListException() {
		    ParameterCommentExceptionProvider.ThrowBindOrEmbeddedParameterNullOnlyListException(_expression, _specifiedSql, false);
		}
    }

    // -----------------------------------------------------
    //                                        Value and Type
    //                                        --------------
    public class InternalValueAndType {
        protected Object _value;
        protected Type _type;
        protected String _rearOption;
        public bool IsValidRearOption() {
            return _value != null && _rearOption != null && _rearOption.Trim().Length > 0;
        }
        public String BuildRearOptionOnSql() {
            return " " + _rearOption.Trim() + " ";
        }
        public Object TargetValue {
            get { return _value; }
            set { _value = value; }
        }
        public Type TargetType {
            get { return _type; }
            set { _type = value; }
        }
        public String RearOption {
            get { return _rearOption; }
            set { _rearOption = value; }
        }
    }

    public class InternalValueAndTypeSetuper {
        protected String _expression;
        protected String[] _names;
        protected String _specifiedSql;
        protected bool _bind;
        public InternalValueAndTypeSetuper(String expression, String[] names, String specifiedSql, bool bind) {
            this._expression = expression;
            this._names = names;
            this._specifiedSql = specifiedSql;
            this._bind = bind;
        }

        public void SetupValueAndType(InternalValueAndType valueAndType) {
            Object value = valueAndType.TargetValue;
            Type type = valueAndType.TargetType;

            // LikeSearchOption handling here is for OutsideSql.
            LikeSearchOption likeSearchOption = null;
            String rearOption = null;

            for (int pos = 1; pos < _names.Length; ++pos) {
                if (value == null) {
                    break;
                }
                String currentName = _names[pos];
                if (pos == 1) {// at the First Loop
                    if (HasLikeSearchOption(type, currentName)) {
                        likeSearchOption = GetLikeSearchOption(type, currentName, value);
                    }
                }
                if (value is System.Collections.IDictionary) {
                    System.Collections.IDictionary map = (System.Collections.IDictionary) value;
                    value = map[currentName];
                    if (IsLastLoop4LikeSearch(pos, likeSearchOption) && IsValidStringValue(value)) {// at the Last Loop
                        value = likeSearchOption.generateRealValue((String) value);
                        rearOption = likeSearchOption.getRearOption();
                    }
                    type = (value != null ? value.GetType() : type);
                    continue;
                }
                if (value is NgMap) {
                    NgMap map = (NgMap) value;
                    value = map.getAsNg(currentName);
                    if (IsLastLoop4LikeSearch(pos, likeSearchOption) && IsValidStringValue(value)) {// at the Last Loop
                        value = likeSearchOption.generateRealValue((String) value);
                        rearOption = likeSearchOption.getRearOption();
                    }
                    type = (value != null ? value.GetType() : type);
                    continue;
                }
                PropertyInfo pi = type.GetProperty(currentName);
                if (pi != null) {
                    value = pi.GetValue(value, null);
                    if (IsLastLoop4LikeSearch(pos, likeSearchOption) && IsValidStringValue(value)) {// at the Last Loop
                        value = likeSearchOption.generateRealValue((String) value);
                        rearOption = likeSearchOption.getRearOption();
                    }
                    type = (value != null ? value.GetType() : pi.PropertyType);
                    continue;
                }
                if (pos == 1 && typeof(MapParameterBean).IsAssignableFrom(type)) {
                    MapParameterBean pmb = (MapParameterBean)value;
                    IDictionary<String, Object> map = pmb.ParameterMap;
                    Object elementValue = (map != null && map.ContainsKey(currentName) ? map[currentName] : null);
                    if (elementValue != null) {
                        value = elementValue;
                        type = elementValue.GetType();
                        continue;
                    }
                }
                ThrowBindOrEmbeddedCommentNotFoundPropertyException(_expression, type, currentName, _specifiedSql, _bind);
            }
            valueAndType.TargetValue = value;
            valueAndType.TargetType = type;
            valueAndType.RearOption = rearOption;
        }

        // for OutsideSql
        protected bool IsLastLoop4LikeSearch(int pos, LikeSearchOption likeSearchOption) {
            return _names.Length == (pos + 1) && likeSearchOption != null;
        }

        protected bool IsValidStringValue(Object value) {
            return value != null && value is String && ((String) value).Length > 0;
        }

        // for OutsideSql
        protected bool HasLikeSearchOption(Type type, String currentName) {
            return type.GetProperty(currentName + "InternalLikeSearchOption") != null;
        }

        // for OutsideSql
        protected LikeSearchOption GetLikeSearchOption(Type type, String currentName, Object resourceBean) {
            PropertyInfo pi = type.GetProperty(currentName + "InternalLikeSearchOption");
            LikeSearchOption option = (LikeSearchOption)pi.GetValue(resourceBean, null);
			if (option == null) {
			    ThrowLikeSearchOptionNotFoundException(resourceBean, currentName);
			}
			if (option.isSplit()) {
			    ThrowOutsideSqlLikeSearchOptionSplitUnsupportedException(option, resourceBean, currentName);
			}
            return option;
        }

        protected void ThrowLikeSearchOptionNotFoundException(Object resourceBean, String currentName) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The likeSearchOption was Not Found! (Should not be null!)" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm your method call:"  + GetLineSeparator();
			String beanName = resourceBean.GetType().Name;
			String methodName = "Set" + InitCap(currentName) + "_LikeSearch(value, likeSearchOption);";
            msg = msg + "    " + beanName + "." + methodName + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Target ParameterBean]" + GetLineSeparator() + resourceBean + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new RequiredOptionNotFoundException(msg);
        }

        // for OutsideSql
        protected void ThrowOutsideSqlLikeSearchOptionSplitUnsupportedException(LikeSearchOption option, Object resourceBean, String currentName) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The splitByXxx() of LikeSearchOption is unsupported at OutsideSql!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm your method call:"  + GetLineSeparator();
            msg = msg + "  For example:"  + GetLineSeparator();
            msg = msg + "    before (x):" + GetLineSeparator();
            String beanName = resourceBean.GetType().Name;
            String methodName = "Set" + InitCap(currentName) + "_LikeSearch(value, likeSearchOption);";
            msg = msg + "      " + beanName + " pmb = new " + beanName + "();"  + GetLineSeparator();
            msg = msg + "      LikeSearchOption likeSearchOption = new LikeSearchOption().likeContain();"  + GetLineSeparator();
            msg = msg + "      likeSearchOption.SplitBySpace(); // *No! Don't invoke this!"  + GetLineSeparator();
            msg = msg + "      pmb." + methodName + GetLineSeparator();
            msg = msg + "    after  (o):" + GetLineSeparator();
            msg = msg + "      " + beanName + " pmb = new " + beanName + "();"  + GetLineSeparator();
            msg = msg + "      LikeSearchOption likeSearchOption = new LikeSearchOption().LikeContain();"  + GetLineSeparator();
            msg = msg + "      pmb." + methodName + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Target LikeSearchOption]" + GetLineSeparator() + option + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Target ParameterBean]" + GetLineSeparator() + resourceBean + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new UnsupportedOperationException(msg);
        }

        protected String InitCap(String name) {
            return SimpleStringUtil.InitCap(name);
        }

        protected virtual void ThrowBindOrEmbeddedCommentNotFoundPropertyException(String expression, Type targetType, String notFoundProperty, String specifiedSql, bool bind) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The property on the " + (bind ? "bind variable" : "embedded value") + " comment was Not Found!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the existence of your property on your arguments." + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Parameter Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[NotFound Property]" + GetLineSeparator() + (targetType != null ? targetType.Name + "#" : "") + notFoundProperty + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            if (bind) {
                throw new BindVariableCommentNotFoundPropertyException(msg);
            } else {
                throw new EmbeddedValueCommentNotFoundPropertyException(msg);
            }
        }

        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }
    }
	
    // -----------------------------------------------------
    //                                    Exception Provider
    //                                    ------------------
	public class ParameterCommentExceptionProvider {
        public static void ThrowBindOrEmbeddedParameterNullValueException(String expression, Type targetType, String specifiedSql, bool bind) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The value of " + (bind ? "bind variable" : "embedded value") + " was Null!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Is it within the scope of your assumption?" + GetLineSeparator();
            msg = msg + "If the answer is YES, please confirm your application logic about the parameter." + GetLineSeparator();
            msg = msg + "If the answer is NO, please confirm the logic of parameter comment(especially IF comment)." + GetLineSeparator();
            msg = msg + "  --> For example:" + GetLineSeparator();
            msg = msg + "        before -- XXX_ID = /*pmb.XxxId*/3" + GetLineSeparator();
            msg = msg + "        after  -- /*IF pmb.XxxId != null*/XXX_ID = /*pmb.XxxId*/3/*END*/" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[" + (bind ? "Bind Variable" : "Embedded Value") + " Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
			msg = msg + "[Parameter Property Type]" + GetLineSeparator() + targetType + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            if (bind) {
                throw new BindVariableParameterNullValueException(msg);
            } else {
                throw new EmbeddedValueParameterNullValueException(msg);
            }
        }
		
        public static void ThrowBindOrEmbeddedParameterEmptyListException(String expression, String specifiedSql, bool bind) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The list of " + (bind ? "bind variable" : "embedded value") + " was empty!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm your application logic." + GetLineSeparator();
            msg = msg + "  For example:" + GetLineSeparator();
            msg = msg + "    before (x):" + GetLineSeparator();
            msg = msg + "      IList<int?> xxxIdList = new List<int?>();" + GetLineSeparator();
            msg = msg + "      cb.Query().SetXxxId_InScope(xxxIdList);// Or pmb.XxxIdList = xxxIdList;" + GetLineSeparator();
            msg = msg + "    after  (o):" + GetLineSeparator();
            msg = msg + "      IList<int?> xxxIdList = new List<int?>();" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(3);" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(7);" + GetLineSeparator();
            msg = msg + "      cb.Query().setXxxId_InScope(xxxIdList);// Or pmb.XxxIdList = xxxIdList;" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[" + (bind ? "Bind Variable" : "Embedded Value") + " Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new IllegalArgumentException(msg);
        }
		
        public static void ThrowBindOrEmbeddedParameterNullOnlyListException(String expression, String specifiedSql, bool bind) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The list of " + (bind ? "bind variable" : "embedded value") + " was 'Null Only List'!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm your application logic." + GetLineSeparator();
            msg = msg + "  For example:" + GetLineSeparator();
            msg = msg + "    before (x):" + GetLineSeparator();
            msg = msg + "      IList<int?> xxxIdList = new ArrayList<int?>();" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(null);" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(null);" + GetLineSeparator();
            msg = msg + "      cb.Query().SetXxxId_InScope(xxxIdList);// Or pmb.XxxIdList = xxxIdList;" + GetLineSeparator();
            msg = msg + "    after  (o):" + GetLineSeparator();
            msg = msg + "      IList<int?> xxxIdList = new ArrayList<int?>();" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(3);" + GetLineSeparator();
            msg = msg + "      xxxIdList.Add(7);" + GetLineSeparator();
            msg = msg + "      cb.Query().SetXxxId_InScope(xxxIdList);// Or pmb.XxxIdList = xxxIdList;" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[" + (bind ? "Bind Variable" : "Embedded Value") + " Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new IllegalArgumentException(msg);
        }
		
		
        protected static String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }
	}
	
    // -----------------------------------------------------
    //                                                IfNode
    //                                                ------
    public class InternalIfNode : ContainerNode {
        private String _expression;
        private ElseNode _elseNode;
        private readonly ExpressionUtil _expressionUtil;
        private String _specifiedSql;

        public InternalIfNode(String expression, String specifiedSql) {
            _expressionUtil = new ExpressionUtil();
            _expression = _expressionUtil.parseExpression(expression);
            if (_expression == null)
                throw new ApplicationException("IllegalBoolExpression=[" + _expression + "]");
            this._specifiedSql = specifiedSql;
        }

        public string Expression {
            get { return _expression; }
        }

        public ElseNode ElseNode {
            get { return _elseNode; }
            set { _elseNode = value; }
        }

        public override void Accept(ICommandContext ctx) {
            Object result = null;
            try {
                result = InvokeExpression(_expression, ctx);
            } catch (Exception e) {
                if (!_expression.Contains("pmb.")) {
                    ThrowIfCommentWrongExpressionException(_expression, e, _specifiedSql);
                }
                String replaced = _expression.Replace("pmb.", "pmb.ParameterMap.");
                String secondParsedExpression = _expressionUtil.parseExpression(replaced);
                try {
                    result = InvokeExpression(secondParsedExpression, ctx);
                } catch (Exception) {
                    ThrowIfCommentWrongExpressionException(_expression, e, _specifiedSql);
                }
                if (result == null) {
                    ThrowIfCommentWrongExpressionException(_expression, e, _specifiedSql);
                }
                _expression = secondParsedExpression;
            }
            if (result != null) {
                if (Convert.ToBoolean(result)) {
                    base.Accept(ctx);
                    ctx.IsEnabled = true;
                } else if (_elseNode != null) {
                    _elseNode.Accept(ctx);
                    ctx.IsEnabled = true;
                }
            } else {
                ThrowIfCommentNotBooleanResultException(_expression, result, _specifiedSql);
            }
        }

        protected virtual void ThrowIfCommentWrongExpressionException(String expression, Exception cause, String specifiedSql) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The IF comment of your specified SQL was Wrong!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the existence of your property on your arguments." + GetLineSeparator();
            msg = msg + "And confirm the IF comment of your specified SQL." + GetLineSeparator();
			msg = msg + "  --> For example, correct IF comment is as below:" + GetLineSeparator();
            msg = msg + "        /*IF pmb.XxxId != null*/XXX_ID = .../*END*/" + GetLineSeparator();
            msg = msg + "        /*IF pmb.Paging*/.../*END*/" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[IF Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Cause Message]" + GetLineSeparator() + cause.Message + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new IfCommentWrongExpressionException(msg, cause);
        }

        protected virtual void ThrowIfCommentNotBooleanResultException(String expression, Object result, String specifiedSql) {
            String msg = "Look! Read the message below." + GetLineSeparator();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + GetLineSeparator();
            msg = msg + "The boolean expression on IF comment of your specified SQL was Wrong!" + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Advice]" + GetLineSeparator();
            msg = msg + "Please confirm the grammar of your IF comment. Does it really express boolean?" + GetLineSeparator();
            msg = msg + "And confirm the existence of your property on your arguments if you use parameterMap." + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[IF Comment Expression]" + GetLineSeparator() + expression + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[IF Comment Result Value]" + GetLineSeparator() + result + GetLineSeparator();
            msg = msg + GetLineSeparator();
            msg = msg + "[Specified SQL]" + GetLineSeparator() + specifiedSql + GetLineSeparator();
            msg = msg + "* * * * * * * * * */";
            throw new IfCommentNotBooleanResultException(msg);
        }

        protected String GetLineSeparator() {
            return SimpleSystemUtil.GetLineSeparator();
        }
    }
}
