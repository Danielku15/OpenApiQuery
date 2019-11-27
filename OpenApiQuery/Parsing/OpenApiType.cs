using System;
using System.Collections.Generic;

namespace OpenApiQuery.Parsing
{
    public class OpenApiType : IOpenApiType
    {
        public Type ClrType { get; }
        public string JsonName { get; }
        public IDictionary<string, IOpenApiTypeProperty> Properties { get; }

        public OpenApiType(Type clrType, string jsonName)
        {
            ClrType = clrType;
            JsonName = jsonName;
            Properties = new Dictionary<string, IOpenApiTypeProperty>();
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

            return Equals(ClrType, other.ClrType);
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

            return Equals((OpenApiType) obj);
        }

        public override int GetHashCode()
        {
            return (ClrType != null ? ClrType.GetHashCode() : 0);
        }
    }
}