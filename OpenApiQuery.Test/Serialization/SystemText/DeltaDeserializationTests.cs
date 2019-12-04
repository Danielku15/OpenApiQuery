using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    [TestClass]
    public class DeltaDeserializationTests : SerializationTestBase
    {
        [TestMethod]
        public void TestDeserialize_Simple()
        {
            var objects = Deserialize<Delta<SimpleClass>>(@"
            {
                ""doubleProp"": 47.11,
                ""stringProp"": ""Hello World""
            }");

            Assert.AreEqual(2, objects.ChangedProperties.Count);
            Assert.AreEqual(47.11, objects.GetValue(x => x.DoubleProp));
            Assert.AreEqual("Hello World", objects.GetValue(x => x.StringProp));
        }

        [TestMethod]
        public void TestDeserialize_Navigation()
        {
            var objects = Deserialize<Delta<SimpleNavigation>>(@"
            {
                ""nav1"": { ""doubleProp"": 47.11, ""stringProp"": ""Hello World"" },
                ""nav2"": { ""doubleProp"": 47.12, ""intProp"": 2 }
            }");

            Assert.AreEqual(2, objects.ChangedProperties.Count);

            var nav1 = objects.GetValue<Delta<SimpleClass>>(x => x.Nav1);
            Assert.AreEqual(2, nav1.ChangedProperties.Count);
            Assert.AreEqual(47.11, nav1.GetValue(x => x.DoubleProp));
            Assert.AreEqual("Hello World", nav1.GetValue(x => x.StringProp));

            var nav2 = objects.GetValue<Delta<SimpleClass>>(x => x.Nav2);
            Assert.AreEqual(2, nav2.ChangedProperties.Count);
            Assert.AreEqual(47.12, nav2.GetValue(x => x.DoubleProp));
            Assert.AreEqual(2, nav2.GetValue(x => x.IntProp));
        }
    }
}
