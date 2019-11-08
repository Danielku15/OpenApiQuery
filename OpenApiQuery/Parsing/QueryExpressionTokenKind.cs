namespace OpenApiQuery.Parsing
{
    internal enum QueryExpressionTokenKind
    {
        No,
        Eof,
        Comma,
        Identifier,
        Keyword,
        Minus,
        Slash,
        OpenParenthesis,
        CloseParenthesis,
        OpenBracket,
        CloseBracket,
        BooleanLiteral,
        DoubleLiteral,
        SingleLiteral,
        StringLiteral,
        LongLiteral,
        IntegerLiteral,
        DateTimeOffsetLiteral,
        GuidLiteral,
        NullLiteral,
        Equal,
        Semicolon
    }
}