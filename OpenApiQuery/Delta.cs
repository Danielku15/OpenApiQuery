using System;
using System.Collections.Generic;
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
    }

    public class Delta<T> : Delta
    {
        public Delta() : base(typeof(T))
        {
        }
    }
}
