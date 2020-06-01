using System.Runtime.Serialization;

namespace OpenApiQuery
{
    [DataContract]
    public class OpenApiQuerySingleResult<T>
    {
        [DataMember(Name = "value")]
        public T Value { get; set; }

        public OpenApiQuerySingleResult(T value)
        {
            Value = value;
        }
    }
}
