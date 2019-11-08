using System;

namespace OpenApiQuery.Parsing
{
    internal class ParseException : Exception
    {
        public int Position { get; }

        public ParseException(string message, int position)
            : base(message)
        {
            Position = position;
        }

        public ParseException(string message, int position, Exception inner)
            : base(message, inner)
        {
            Position = position;
        }
    }
}