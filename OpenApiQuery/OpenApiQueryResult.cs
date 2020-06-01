using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenApiQuery
{
    [DataContract]
    public class OpenApiQueryResult<T>
    {
        [DataMember(Name = "@odata.count", EmitDefaultValue = false)]
        public long? Count { get; set; }
        [DataMember(Name = "value")]
        public IReadOnlyCollection<T> Value { get; set; }

        public OpenApiQueryResult(long? count, IReadOnlyCollection<T> value)
        {
            Count = count;
            Value = value;
        }

        public OpenApiQueryResult()
        {

        }
    }
}
