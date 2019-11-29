using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public interface IOpenApiType : IEquatable<IOpenApiType>
    {
        string JsonName { get; }
        Type ClrType { get; }
        IEnumerable<IOpenApiTypeProperty> Properties { get; }
        void RegisterProperty(IOpenApiTypeProperty property);
        bool TryGetProperty(PropertyInfo clrProperty, out IOpenApiTypeProperty property);
        bool TryGetProperty(string propertyName, out IOpenApiTypeProperty property);
    }
}
