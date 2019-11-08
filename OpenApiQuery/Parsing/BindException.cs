using System;
using System.Runtime.Serialization;

namespace OpenApiQuery.Parsing
{
    [Serializable]
    public class BindException : Exception
    {
        public BindException()
        {
        }

        public BindException(string message) : base(message)
        {
        }

        public BindException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BindException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}