using System;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public class OpenApiTypeProperty : IOpenApiTypeProperty
    {
        private readonly Func<object, object> _get;
        public PropertyInfo ClrProperty { get; }
        public string JsonName { get; }
        public Type ValueType { get; set; }

        public OpenApiTypeProperty(
            PropertyInfo clrProperty, string jsonName, Type valueType,
            Func<object, object> get
        )
        {
            _get = get;
            ClrProperty = clrProperty;
            JsonName = jsonName;
            ValueType = valueType;
        }

        public object GetValue(object instance)
        {
            return _get(instance);
        }


        public bool Equals(IOpenApiTypeProperty other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(ClrProperty, other.ClrProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((IOpenApiTypeProperty) obj);
        }

        public override int GetHashCode()
        {
            return (ClrProperty != null ? ClrProperty.GetHashCode() : 0);
        }
    }
}
