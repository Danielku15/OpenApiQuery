using System;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public interface IOpenApiTypeProperty : IEquatable<IOpenApiTypeProperty>
    {
        PropertyInfo ClrProperty { get; }
        string JsonName { get; }

        object GetValue(object instance);
        void SetValue(object instance, object value);
    }
}
