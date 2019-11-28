using System;
using System.Collections.Generic;

namespace OpenApiQuery.Parsing
{
    public interface IOpenApiType : IEquatable<IOpenApiType>
    {
        string JsonName { get; }
        Type ClrType { get; }
        IDictionary<string, IOpenApiTypeProperty> Properties { get; }
    }
}