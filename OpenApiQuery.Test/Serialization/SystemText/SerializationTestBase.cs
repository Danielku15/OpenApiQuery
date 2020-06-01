using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    public class SerializationTestBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        protected static JObject Serialize<T>(T obj) => JObject.FromObject(obj, JsonSerializer.Create(JsonSerializerSettings));

        protected static T Deserialize<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);

        protected static void VerifySerialize<T>(Dictionary<string, object> expected, T actual)
        {
            var expectedJson = Serialize(expected);
            var actualJson = Serialize(actual);
            Assert.IsTrue(JToken.DeepEquals(expectedJson, actualJson), $"Expected {expectedJson} but found {actualJson}");
        }
    }
}
