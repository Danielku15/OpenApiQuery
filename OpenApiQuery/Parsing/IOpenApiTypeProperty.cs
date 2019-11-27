using System;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public interface IOpenApiTypeProperty : IEquatable<IOpenApiTypeProperty>
    {
        PropertyInfo ClrProperty { get; }
        string JsonName { get; }
        Type ValueType { get; set; }

        object GetValue(object instance);
    }
}