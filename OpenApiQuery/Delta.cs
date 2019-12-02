using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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

        protected void ApplyPatch(object instance)
        {
            if (instance == null)
            {
                return;
            }

            // TODO: cache getter/setter
            foreach (var changedProperty in ChangedProperties)
            {
                if (changedProperty.Value is Delta d)
                {
                    d.ApplyPatch(changedProperty.Key.GetValue(instance));
                }
                else
                {
                    changedProperty.Key.SetValue(instance, changedProperty.Value);
                }
            }
        }
    }

    public class Delta<T> : Delta
    {
        public Delta() : base(typeof(T))
        {
        }

        public void ApplyPatch(T instance)
        {
            base.ApplyPatch(instance);
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
