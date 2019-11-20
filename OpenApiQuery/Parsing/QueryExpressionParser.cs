using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    internal class QueryExpressionParser
    {
        private const char Eof = '\0';

        private readonly string _value;
        private readonly IOpenApiQueryExpressionBinder _binder;

        private char _currentCharacter;
        private Stack<System.Linq.Expressions.Expression> _thisValues = new Stack<System.Linq.Expressions.Expression>();

        public QueryExpressionTokenKind CurrentTokenKind { get; private set; }

        public object TokenData { get; private set; }

        public int Position { get; private set; } = -1;

        public QueryExpressionParser(string raw, IOpenApiQueryExpressionBinder binder)
        {
            _value = raw;
            _binder = binder;
            Init();
        }

        #region Lexer

        public void NextToken()
        {
            CurrentTokenKind = QueryExpressionTokenKind.No;
            do
            {
                if (_currentCharacter == Eof)
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Eof;
                }
                else if (char.IsWhiteSpace(_currentCharacter))
                {
                    // skip whitespaces
                    NextCharacter();
                }
                else if (_currentCharacter == '=')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Equal;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == ',')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Comma;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == ';')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Semicolon;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '-')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Minus;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '/')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Slash;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '(')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.OpenParenthesis;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == ')')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.CloseParenthesis;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '[')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.OpenBracket;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == ']')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.CloseBracket;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '*')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.Star;
                    TokenData = null;
                    NextCharacter();
                }
                else if (_currentCharacter == '\'' || _currentCharacter == '"')
                {
                    CurrentTokenKind = QueryExpressionTokenKind.StringLiteral;
                    TokenData = ReadString();
                }
                else
                {
                    var token = ReadFullToken();

                    switch (token)
                    {
                        case "true":
                            TokenData = true;
                            CurrentTokenKind = QueryExpressionTokenKind.BooleanLiteral;
                            break;
                        case "false":
                            TokenData = false;
                            CurrentTokenKind = QueryExpressionTokenKind.BooleanLiteral;
                            break;
                        case "null":
                            TokenData = null;
                            CurrentTokenKind = QueryExpressionTokenKind.NullLiteral;
                            break;
                        case "asc":
                        case "desc":
                        case "not":
                        case "mod":
                        case "div":
                        case "mul":
                        case "sub":
                        case "add":
                        case "in":
                        case "has":
                        case "le":
                        case "lt":
                        case "ge":
                        case "gt":
                        case "ne":
                        case "eq":
                        case "and":
                        case "or":
                            TokenData = token;
                            CurrentTokenKind = QueryExpressionTokenKind.Keyword;
                            break;
                        default:
                            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out var integer))
                            {
                                if (integer >= int.MinValue && integer <= int.MaxValue)
                                {
                                    TokenData = (int) integer;
                                    CurrentTokenKind = QueryExpressionTokenKind.IntegerLiteral;
                                }
                                else
                                {
                                    TokenData = integer;
                                    CurrentTokenKind = QueryExpressionTokenKind.LongLiteral;
                                }
                            }
                            else if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture,
                                out var floating))
                            {
                                if (floating >= float.MinValue && floating <= float.MaxValue)
                                {
                                    TokenData = (int) floating;
                                    CurrentTokenKind = QueryExpressionTokenKind.SingleLiteral;
                                }
                                else
                                {
                                    TokenData = floating;
                                    CurrentTokenKind = QueryExpressionTokenKind.DoubleLiteral;
                                }
                            }
                            else if (Guid.TryParse(token, out var guid))
                            {
                                TokenData = guid;
                                CurrentTokenKind = QueryExpressionTokenKind.GuidLiteral;
                            }
                            else if (DateTimeOffset.TryParse(token, out var dateTimeOffset))
                            {
                                TokenData = dateTimeOffset;
                                CurrentTokenKind = QueryExpressionTokenKind.DateTimeOffsetLiteral;
                            }
                            else
                            {
                                TokenData = token;
                                CurrentTokenKind = QueryExpressionTokenKind.Identifier;
                            }

                            break;
                    }
                }
            } while (CurrentTokenKind == QueryExpressionTokenKind.No);
        }

        private string ReadFullToken()
        {
            var startPos = Position;
            while (Position < _value.Length)
            {
                var ch = _currentCharacter;
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '$')
                {
                    // valid char, continue search for end
                    NextCharacter();
                }
                else
                {
                    break;
                }
            }

            return _value.Substring(startPos, Position - startPos);
        }

        private string ReadString()
        {
            var startChar = _currentCharacter;
            NextCharacter();

            var startPos = Position;
            while (Position < _value.Length)
            {
                var ch = _currentCharacter;
                var prevChar = _value[Position - 1];
                NextCharacter();
                if (ch == startChar && prevChar != '\\')
                {
                    return _value.Substring(startPos, Position - startPos - 1);
                }
            }

            ReportError($"Missing string end character {startChar}");
            return null;
        }

        private void NextCharacter()
        {
            Position++;
            _currentCharacter = Position < _value.Length ? _value[Position] : Eof;
        }

        public void ReportError(string message)
        {
            throw new ParseException(message, Position);
        }

        #endregion


        public void Init()
        {
            NextCharacter();
            NextToken();
        }

        public void PushThis(Expression it)
        {
            _thisValues.Push(it);
        }

        public void PopThis()
        {
            _thisValues.Pop();
        }

        public Expression PeekThis()
        {
            return _thisValues.Peek();
        }

        public System.Linq.Expressions.Expression CommonExpr()
        {
            return LogicalOr();
        }

        private System.Linq.Expressions.Expression LogicalOr()
        {
            var left = LogicalAnd();
            while (CurrentTokenKind == QueryExpressionTokenKind.Keyword && "or".Equals(TokenData))
            {
                NextToken();
                var right = LogicalAnd();
                left = System.Linq.Expressions.Expression.Or(left, right);
            }

            return left;
        }

        private System.Linq.Expressions.Expression LogicalAnd()
        {
            var left = Comparison();
            while (CurrentTokenKind == QueryExpressionTokenKind.Keyword && "and".Equals(TokenData))
            {
                NextToken();
                var right = Comparison();
                left = System.Linq.Expressions.Expression.And(left, right);
            }

            return left;
        }

        private System.Linq.Expressions.Expression Comparison()
        {
            var left = Additive();
            while (CurrentTokenKind == QueryExpressionTokenKind.Keyword)
            {
                var op = (string) TokenData;

                ExpressionType? expression = null;
                switch (op.ToLowerInvariant())
                {
                    case "eq":
                        expression = ExpressionType.Equal;
                        break;
                    case "ne":
                        expression = ExpressionType.NotEqual;
                        break;
                    case "gt":
                        expression = ExpressionType.GreaterThan;
                        break;
                    case "ge":
                        expression = ExpressionType.GreaterThanOrEqual;
                        break;
                    case "lt":
                        expression = ExpressionType.LessThan;
                        break;
                    case "le":
                        expression = ExpressionType.LessThanOrEqual;
                        break;
                    case "has":
                        NextToken();
                        var flag = System.Linq.Expressions.Expression.Convert(Additive(), typeof(Enum));
                        left = System.Linq.Expressions.Expression.Call(null, HasFlagMethodInfo, left, flag);
                        return left;
                    case "in":
                        NextToken();
                        var collection = List(left.Type) ?? CommonExpr();
                        // TODO: cache
                        left = System.Linq.Expressions.Expression.Call(null, ContainsMethodInfo.MakeGenericMethod(left.Type), collection, left);
                        return left;
                }

                if (expression == null)
                {
                    break;
                }

                NextToken();
                var right = Additive();
                left = System.Linq.Expressions.Expression.MakeBinary(expression.Value, Promote(left, right), Promote(right, left));
            }

            return left;
        }

        private System.Linq.Expressions.Expression List(Type itemType)
        {
            if (CurrentTokenKind != QueryExpressionTokenKind.OpenBracket)
            {
                return null;
            }

            NextToken();

            var expressions = new List<System.Linq.Expressions.Expression>();

            var expr = CommonExpr();
            expressions.Add(expr);

            while (CurrentTokenKind == QueryExpressionTokenKind.Comma)
            {
                NextToken();

                expr = CommonExpr();
                expressions.Add(expr);
            }

            if (CurrentTokenKind != QueryExpressionTokenKind.OpenBracket)
            {
                ReportError($"Expected close bracket at position {Position}");
            }

            NextToken();


            return System.Linq.Expressions.Expression.NewArrayInit(itemType, expressions.Select(e => Promote(e, itemType)));
        }

        private static readonly MethodInfo HasFlagMethodInfo =
            typeof(Enum).GetMethod(nameof(Enum.HasFlag), BindingFlags.Static | BindingFlags.Public);

        private static readonly MethodInfo ContainsMethodInfo =
            typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);


        private System.Linq.Expressions.Expression Additive()
        {
            var left = Multiplicative();
            while (CurrentTokenKind == QueryExpressionTokenKind.Keyword)
            {
                var op = (string) TokenData;

                ExpressionType? expression = null;
                switch (op.ToLowerInvariant())
                {
                    case "add":
                        expression = ExpressionType.Add;
                        break;
                    case "sub":
                        expression = ExpressionType.Subtract;
                        break;
                }

                if (expression != null)
                {
                    NextToken();
                    var right = Multiplicative();
                    left = System.Linq.Expressions.Expression.MakeBinary(expression.Value, Promote(left, right), Promote(right, left));
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private System.Linq.Expressions.Expression Multiplicative()
        {
            var left = Unary();
            while (CurrentTokenKind == QueryExpressionTokenKind.Keyword)
            {
                var op = (string) TokenData;

                ExpressionType? expression = null;
                switch (op.ToLowerInvariant())
                {
                    case "mul":
                        expression = ExpressionType.Multiply;
                        break;
                    case "div":
                        expression = ExpressionType.Divide;
                        break;
                    case "mod":
                        expression = ExpressionType.Modulo;
                        break;
                }

                if (expression != null)
                {
                    NextToken();
                    var right = Unary();
                    left = System.Linq.Expressions.Expression.MakeBinary(expression.Value, Promote(left, right), Promote(right, left));
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private System.Linq.Expressions.Expression Promote(System.Linq.Expressions.Expression toPromote, System.Linq.Expressions.Expression other)
        {
            return Promote(toPromote, other.Type);
        }

        private System.Linq.Expressions.Expression Promote(System.Linq.Expressions.Expression toPromote, Type otherType)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions#implicit-numeric-conversions
            if (toPromote.Type == typeof(sbyte))
            {
                if (otherType == typeof(short) ||
                    otherType == typeof(int) ||
                    otherType == typeof(long) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(byte))
            {
                if (otherType == typeof(short) ||
                    otherType == typeof(ushort) ||
                    otherType == typeof(int) ||
                    otherType == typeof(uint) ||
                    otherType == typeof(long) ||
                    otherType == typeof(ulong) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(short))
            {
                if (otherType == typeof(int) ||
                    otherType == typeof(long) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(ushort))
            {
                if (otherType == typeof(int) ||
                    otherType == typeof(uint) ||
                    otherType == typeof(long) ||
                    otherType == typeof(ulong) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(int))
            {
                if (otherType == typeof(long) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(uint))
            {
                if (otherType == typeof(long) ||
                    otherType == typeof(ulong) ||
                    otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(long))
            {
                if (otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(ulong))
            {
                if (otherType == typeof(float) ||
                    otherType == typeof(double) ||
                    otherType == typeof(decimal))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            if (toPromote.Type == typeof(float))
            {
                if (otherType == typeof(double))
                {
                    return System.Linq.Expressions.Expression.Convert(toPromote, otherType);
                }
            }

            return toPromote;
        }

        private System.Linq.Expressions.Expression Unary()
        {
            if (CurrentTokenKind == QueryExpressionTokenKind.Keyword && (string) TokenData == "not")
            {
                NextToken();
                var unary = Unary();
                return System.Linq.Expressions.Expression.Not(unary);
            }

            if (CurrentTokenKind == QueryExpressionTokenKind.Minus)
            {
                NextToken();
                if (IsNumeric(CurrentTokenKind))
                {
                    return Primary();
                }
                else
                {
                    var unary = Unary();
                    return System.Linq.Expressions.Expression.Negate(unary);
                }
            }

            return Primary();
        }

        private System.Linq.Expressions.Expression Primary()
        {
            var expression = Segment(_thisValues.Peek());
            while (CurrentTokenKind == QueryExpressionTokenKind.Slash)
            {
                NextToken();
                expression = Segment(expression);
            }

            return expression;
        }

        private System.Linq.Expressions.Expression Segment(System.Linq.Expressions.Expression expression)
        {
            switch (CurrentTokenKind)
            {
                case QueryExpressionTokenKind.Identifier:
                    var identifier = (string) TokenData;
                    NextToken();

                    // function call
                    if (CurrentTokenKind == QueryExpressionTokenKind.OpenParenthesis)
                    {
                        NextToken();
                        
                        var arguments = new List<System.Linq.Expressions.Expression>();

                        while (CurrentTokenKind != QueryExpressionTokenKind.CloseParenthesis)
                        {
                            var expr = CommonExpr();
                            arguments.Add(expr);
                            if (CurrentTokenKind == QueryExpressionTokenKind.Comma)
                            {
                                NextToken();
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (CurrentTokenKind != QueryExpressionTokenKind.CloseParenthesis)
                        {
                            ReportError("Missing close parenthesis after function call");
                            return null;
                        }

                        return BindFunctionCall(identifier, arguments);
                    }
                    else
                    {
                        var member = BindMember(expression, identifier);
                        return Expression.MakeMemberAccess(expression, member);
                    }

                case QueryExpressionTokenKind.OpenParenthesis:
                    return Parenthsis();
                default:
                    return Literal();
            }
        }


        private System.Linq.Expressions.Expression Literal()
        {
            switch (CurrentTokenKind)
            {
                case QueryExpressionTokenKind.BooleanLiteral:
                case QueryExpressionTokenKind.DoubleLiteral:
                case QueryExpressionTokenKind.SingleLiteral:
                case QueryExpressionTokenKind.StringLiteral:
                case QueryExpressionTokenKind.LongLiteral:
                case QueryExpressionTokenKind.IntegerLiteral:
                case QueryExpressionTokenKind.DateTimeOffsetLiteral:
                case QueryExpressionTokenKind.GuidLiteral:
                    var data = TokenData;
                    NextToken();
                    return System.Linq.Expressions.Expression.Constant(data, data.GetType());
                case QueryExpressionTokenKind.NullLiteral:
                    NextToken();
                    return System.Linq.Expressions.Expression.Constant(null);
                default:
                    ReportError($"Unexpected expression at position {Position}");
                    return null;
            }
        }

        private System.Linq.Expressions.Expression Parenthsis()
        {
            NextToken();
            var result = CommonExpr();

            if (CurrentTokenKind != QueryExpressionTokenKind.CloseParenthesis)
            {
                ReportError($"Missing close parenthesis on position {Position}");
            }

            NextToken();

            return result;
        }

        public MemberInfo BindMember(string tokenData)
        {
            return BindMember(_thisValues.Peek(), tokenData);
        }

        public MemberInfo BindMember(System.Linq.Expressions.Expression expression, string tokenData)
        {
            try
            {
                return _binder.BindMember(expression, tokenData);
            }
            catch (BindException e)
            {
                throw new ParseException(e.Message, Position, e);
            }
        }

        
        
        private System.Linq.Expressions.Expression BindFunctionCall(string identifier, List<System.Linq.Expressions.Expression> arguments)
        {
            try
            {
                return _binder.BindFunctionCall(identifier, arguments);
            }
            catch (BindException e)
            {
                throw new ParseException(e.Message, Position, e);
            }        
        }
        private bool IsNumeric(QueryExpressionTokenKind currentQueryExpressionTokenKind)
        {
            switch (currentQueryExpressionTokenKind)
            {
                case QueryExpressionTokenKind.DoubleLiteral:
                case QueryExpressionTokenKind.SingleLiteral:
                case QueryExpressionTokenKind.LongLiteral:
                case QueryExpressionTokenKind.IntegerLiteral:
                    return true;
            }

            return false;
        }
    }
}