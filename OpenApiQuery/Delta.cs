using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class Delta
    {
        public Type ObjectType { get; set; }
        public IDictionary<PropertyInfo, object> ChangedProperties { get; }

        public Delta(Type objectType)
        {
            ChangedProperties = new Dictionary<PropertyInfo, object>();
            ObjectType = objectType;
        }

        public void SetValue(PropertyInfo property, object value)
        {
            ChangedProperties[property] = value;
        }
    }

    public class Delta<T> : Delta
    {
        public Delta() : base(typeof(T))
        {
        }

        public TValue GetValue<TValue>(Expression<Func<T, TValue>> property)
        {
            if (property.Body is MemberExpression member)
            {
                return GetValue<TValue>((PropertyInfo)member.Member);
            }

            throw new ArgumentException("Accessor was no property access expression", nameof(property));
        }

        public TValue GetValue<TValue>(Expression<Func<T, object>> property)
        {
            if (property.Body is MemberExpression member)
            {
                return GetValue<TValue>((PropertyInfo)member.Member);
            }

            throw new ArgumentException("Accessor was no property access expression", nameof(property));
        }

        public TValue GetValue<TValue>(PropertyInfo property)
        {
            if (ChangedProperties.TryGetValue(property, out var value))
            {
                if (value is TValue casted)
                {
                    return casted;
                }
                if (value == null)
                {
                    return default;
                }
            }

            throw new ArgumentException($"Invalid property type requested, requested type was {typeof(TValue)} actual type was {value?.GetType()}");
        }
    }
}
