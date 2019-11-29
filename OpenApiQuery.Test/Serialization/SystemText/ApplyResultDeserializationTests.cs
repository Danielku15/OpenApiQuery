using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    [TestClass]
    public class ApplyResultDeserializationTests : SerializationTestBase
    {
        [TestMethod]
        public void TestDeserialize_Simple()
        {
            var objects = Deserialize<OpenApiQueryApplyResult<SimpleClass>>(@"
            {
                ""@odata.count"": 2,
                ""value"": [
                    {
                        ""intProp"": 1,
                        ""doubleProp"": 47.11,
                        ""stringProp"": ""Hello World""
                    },
                    {
                        ""intProp"": 2,
                        ""doubleProp"": 47.12,
                        ""stringProp"": ""Foo Bar""
                    }
                ]
            }");

            Assert.AreEqual(2, objects.TotalCount);
            Assert.AreEqual(2, objects.ResultItems.Length);

            Assert.AreEqual(1, objects.ResultItems[0].IntProp);
            Assert.AreEqual(47.11, objects.ResultItems[0].DoubleProp);
            Assert.AreEqual("Hello World", objects.ResultItems[0].StringProp);

            Assert.AreEqual(2, objects.ResultItems[1].IntProp);
            Assert.AreEqual(47.12, objects.ResultItems[1].DoubleProp);
            Assert.AreEqual("Foo Bar", objects.ResultItems[1].StringProp);
        }

        [TestMethod]
        public void TestSerialize_Polymorphic()
        {
            var objects = Deserialize<OpenApiQueryApplyResult<Base>>(@"
            {
                ""@odata.count"": 2,
                ""value"": [
                    {
                        ""@odata.type"": ""Sub1"",
                        ""baseProp"": 1,
                        ""sub1Prop"": 47.11,
                        ""subProp"": 47
                    },
                    {
                        ""@odata.type"": ""Sub2""
                        ""baseProp"": 2,
                        ""sub2Prop"": ""Test"",
                        ""subProp"": -47
                    }
                ]
            }");

            Assert.AreEqual(2, objects.TotalCount);
            Assert.AreEqual(2, objects.ResultItems.Length);

            Assert.IsInstanceOfType(objects.ResultItems[0], typeof(Sub1));
            var sub1 = (Sub1)objects.ResultItems[0];
            Assert.AreEqual(1, sub1.BaseProp);
            Assert.AreEqual(47.11, sub1.Sub1Prop);
            Assert.AreEqual(47, sub1.SubProp);

            Assert.IsInstanceOfType(objects.ResultItems[1], typeof(Sub2));
            var sub2 = (Sub2)objects.ResultItems[1];
            Assert.AreEqual(1, sub2.BaseProp);
            Assert.AreEqual("Test", sub2.Sub2Prop);
            Assert.AreEqual(-47, sub2.SubProp);
        }

        [TestMethod]
        public void TestDeserialize_ObjectArrays()
        {
        }

        [TestMethod]
        public void TestDeserialize_ObjectArrays_Polymorphic()
        {

        }

        [TestMethod]
        public void TestDeserialize_NativeArrays()
        {
        }

        [TestMethod]
        public void TestDeserialize_Dictionary()
        {
        }

        [TestMethod]
        public void TestDeserialize_PartialProperties()
        {
        }

        [TestMethod]
        public void TestDeserialize_PartialNavigationProperties_Collection()
        {
        }

        [TestMethod]
        public void TestDeserialize_PartialNavigationProperties_Single()
        {
        }
    }
}
