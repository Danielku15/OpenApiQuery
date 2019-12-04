using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenApiQuery.Parsing
{
    public class OpenApiType : IOpenApiType
    {
        private readonly IDictionary<string, IOpenApiTypeProperty> _propertiesByName;
        private readonly IDictionary<PropertyInfo, IOpenApiTypeProperty> _propertiesByClr;

        public Type ClrType { get; }
        public string JsonName { get; }

        public IEnumerable<IOpenApiTypeProperty> Properties => _propertiesByName.Values;

        public OpenApiType(Type clrType, string jsonName)
        {
            ClrType = clrType;
            JsonName = jsonName;
            _propertiesByName = new Dictionary<string, IOpenApiTypeProperty>(StringComparer.InvariantCultureIgnoreCase);
            _propertiesByClr = new Dictionary<PropertyInfo, IOpenApiTypeProperty>();
        }

        public void RegisterProperty(IOpenApiTypeProperty property)
        {
            _propertiesByName[property.JsonName] = property;
            _propertiesByClr[property.ClrProperty] = property;
        }

        public bool TryGetProperty(PropertyInfo clrProperty, out IOpenApiTypeProperty property)
        {
            return _propertiesByClr.TryGetValue(clrProperty, out property);
        }

        public bool TryGetProperty(string propertyName, out IOpenApiTypeProperty property)
        {
            return _propertiesByName.TryGetValue(propertyName, out property);
        }

        public bool Equals(IOpenApiType other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ClrType == other.ClrType;
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

            return Equals((OpenApiType)obj);
        }

        public override int GetHashCode()
        {
            return (ClrType != null ? ClrType.GetHashCode() : 0);
        }
    }
}
