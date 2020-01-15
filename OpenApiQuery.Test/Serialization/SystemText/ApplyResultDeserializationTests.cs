using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    [TestClass]
    public class ApplyResultDeserializationTests : SerializationTestBase
    {
        [TestMethod]
        public void TestDeserialize_Simple()
        {
            var objects = Deserialize<OpenApiQueryResult<SimpleClass>>(@"
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
        public void TestDeserialize_Polymorphic()
        {
            var typeHandler = new DefaultOpenApiTypeHandler();
            typeHandler.ResolveType(typeof(Base));
            typeHandler.ResolveType(typeof(Sub1));
            typeHandler.ResolveType(typeof(Sub2));

            var objects = Deserialize<OpenApiQueryResult<Base>>(@"
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
                        ""@odata.type"": ""Sub2"",
                        ""baseProp"": 2,
                        ""sub2Prop"": ""Test"",
                        ""subProp"": -47
                    }
                ]
            }", typeHandler);

            Assert.AreEqual(2, objects.TotalCount);
            Assert.AreEqual(2, objects.ResultItems.Length);

            Assert.IsInstanceOfType(objects.ResultItems[0], typeof(Sub1));
            var sub1 = (Sub1)objects.ResultItems[0];
            Assert.AreEqual(1, sub1.BaseProp);
            Assert.AreEqual(47.11, sub1.Sub1Prop);
            Assert.AreEqual(47, sub1.SubProp);

            Assert.IsInstanceOfType(objects.ResultItems[1], typeof(Sub2));
            var sub2 = (Sub2)objects.ResultItems[1];
            Assert.AreEqual(2, sub2.BaseProp);
            Assert.AreEqual("Test", sub2.Sub2Prop);
            Assert.AreEqual(-47, sub2.SubProp);
        }

        [TestMethod]
        public void TestDeserialize_ObjectArrays()
        {
            var objects = Deserialize<OpenApiQueryResult<ArrayWrapper<SimpleClass>>>(@"
            {
                ""@odata.count"": 2,
                ""value"": [
                    {
                        ""items"": [
                            {
                                ""intProp"": 1,
                                ""doubleProp"": 47.11,
                                ""stringProp"": ""A""
                            },
                            {
                                ""intProp"": 2,
                                ""doubleProp"": 47.12,
                                ""stringProp"": ""B""
                            }
                        ]
                    },
                    {
                        ""items"": [
                            {
                                ""intProp"": 3,
                                ""doubleProp"": 47.13,
                                ""stringProp"": ""C""
                            },
                            {
                                ""intProp"": 4,
                                ""doubleProp"": 47.14,
                                ""stringProp"": ""D""
                            }
                        ]
                    }
                ]
            }");

            Assert.AreEqual(2, objects.TotalCount);
            Assert.AreEqual(2, objects.ResultItems.Length);

            Assert.AreEqual(2, objects.ResultItems[0].Items.Length);
            Assert.AreEqual(1, objects.ResultItems[0].Items[0].IntProp);
            Assert.AreEqual(47.11, objects.ResultItems[0].Items[0].DoubleProp);
            Assert.AreEqual("A", objects.ResultItems[0].Items[0].StringProp);

            Assert.AreEqual(2, objects.ResultItems[0].Items[1].IntProp);
            Assert.AreEqual(47.12, objects.ResultItems[0].Items[1].DoubleProp);
            Assert.AreEqual("B", objects.ResultItems[0].Items[1].StringProp);

            Assert.AreEqual(2, objects.ResultItems[1].Items.Length);
            Assert.AreEqual(3, objects.ResultItems[1].Items[0].IntProp);
            Assert.AreEqual(47.13, objects.ResultItems[1].Items[0].DoubleProp);
            Assert.AreEqual("C", objects.ResultItems[1].Items[0].StringProp);

            Assert.AreEqual(4, objects.ResultItems[1].Items[1].IntProp);
            Assert.AreEqual(47.14, objects.ResultItems[1].Items[1].DoubleProp);
            Assert.AreEqual("D", objects.ResultItems[1].Items[1].StringProp);
        }

        [TestMethod]
        public void TestDeserialize_ObjectArrays_Polymorphic()
        {
            var typeHandler = new DefaultOpenApiTypeHandler();
            typeHandler.ResolveType(typeof(Base));
            typeHandler.ResolveType(typeof(Sub1));
            typeHandler.ResolveType(typeof(Sub2));

            var objects = Deserialize<OpenApiQueryResult<ArrayWrapper<Base>>>(@"
            {
                ""@odata.count"": 2,
                ""value"": [
                    {
                        ""items"": [
                            {
                                ""@odata.type"": ""Sub1"",
                                ""baseProp"": 1,
                                ""sub1Prop"": 47.11,
                                ""subProp"": 47
                            },
                            {
                                ""@odata.type"": ""Sub2"",
                                ""baseProp"": 2,
                                ""sub2Prop"": ""Test"",
                                ""subProp"": -47
                            }
                        ]
                    },
                    {
                        ""items"": [
                            {
                                ""@odata.type"": ""Sub2"",
                                ""baseProp"": 3,
                                ""sub2Prop"": ""A"",
                                ""subProp"": -11
                            },
                            {
                                ""@odata.type"": ""Sub1"",
                                ""baseProp"": 4,
                                ""sub1Prop"": 47.14,
                                ""subProp"": 12
                            }

                        ]
                    }
                ]
            }", typeHandler);

            Assert.AreEqual(2, objects.TotalCount);
            Assert.AreEqual(2, objects.ResultItems.Length);

            Assert.AreEqual(2, objects.ResultItems[0].Items.Length);
            Assert.IsInstanceOfType(objects.ResultItems[0].Items[0], typeof(Sub1));
            Assert.AreEqual(1, ((Sub1)objects.ResultItems[0].Items[0]).BaseProp);
            Assert.AreEqual(47.11, ((Sub1)objects.ResultItems[0].Items[0]).Sub1Prop);
            Assert.AreEqual(47, ((Sub1)objects.ResultItems[0].Items[0]).SubProp);

            Assert.IsInstanceOfType(objects.ResultItems[0].Items[1], typeof(Sub2));
            Assert.AreEqual(2, ((Sub2)objects.ResultItems[0].Items[1]).BaseProp);
            Assert.AreEqual("Test", ((Sub2)objects.ResultItems[0].Items[1]).Sub2Prop);
            Assert.AreEqual(-47, ((Sub2)objects.ResultItems[0].Items[1]).SubProp);

            Assert.AreEqual(2, objects.ResultItems[1].Items.Length);
            Assert.IsInstanceOfType(objects.ResultItems[1].Items[0], typeof(Sub2));
            Assert.AreEqual(3, ((Sub2)objects.ResultItems[1].Items[0]).BaseProp);
            Assert.AreEqual("A", ((Sub2)objects.ResultItems[1].Items[0]).Sub2Prop);
            Assert.AreEqual(-11, ((Sub2)objects.ResultItems[1].Items[0]).SubProp);

            Assert.IsInstanceOfType(objects.ResultItems[1].Items[1], typeof(Sub1));
            Assert.AreEqual(4, ((Sub1)objects.ResultItems[1].Items[1]).BaseProp);
            Assert.AreEqual(47.14, ((Sub1)objects.ResultItems[1].Items[1]).Sub1Prop);
            Assert.AreEqual(12, ((Sub1)objects.ResultItems[1].Items[1]).SubProp);
        }

        [TestMethod]
        public void TestDeserialize_NativeArrays()
        {
            TestDeserialize_NativeArrays(@"{
                    ""value"": [
                        { ""items"": [1,2,3] },
                        { ""items"": [4,5,6] }
                    ]
                }",
                new[]
                {
                    new[]
                    {
                        1, 2, 3
                    },
                    new[]
                    {
                        4, 5, 6
                    },
                }
            );
            TestDeserialize_NativeArrays(@"{
                    ""value"": [
                        { ""items"": [1.0,2.1,3.2] },
                        { ""items"": [4.3,5.4,6.5] }
                    ]
                }",
                new[]
                {
                    new[]
                    {
                        1.0, 2.1, 3.2
                    },
                    new[]
                    {
                        4.3, 5.4, 6.5
                    },
                }
            );
            TestDeserialize_NativeArrays(@"{
                    ""value"": [
                        { ""items"": [""A"", ""B"", ""C""] },
                        { ""items"": [""D"", ""E"", ""F""] }
                    ]
                }",
                new[]
                {
                    new[]
                    {
                        "A", "B", "C"
                    },
                    new[]
                    {
                        "D", "E", "F"
                    },
                }
            );
        }

        private void TestDeserialize_NativeArrays<T>(string json, T[][] expected)
        {
            var objects = Deserialize<OpenApiQueryResult<ArrayWrapper<T>>>(json);

            Assert.AreEqual(expected.Length, objects.ResultItems.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                var actual = objects.ResultItems[i].Items;

                Assert.AreEqual(string.Join(",", expected[i]),
                    string.Join(",", actual),
                    $"Mismatch on item {i}");
            }
        }

        [TestMethod]
        public void TestDeserialize_Dictionary_SimpleTypes()
        {
            var objects = Deserialize<OpenApiQueryResult<Dictionary<string, int>>>(@"{
                ""value"": [
                    { ""A"": 1, ""b"": 2, ""C"": 3 },
                    { ""d"": 4, ""E"": 5, ""F"": 6 }
                ]
            }");

            Assert.AreEqual(2, objects.ResultItems.Length);
            Assert.AreEqual("A:1,b:2,C:3", string.Join(",", objects.ResultItems[0].Select(kvp => $"{kvp.Key}:{kvp.Value}")));
            Assert.AreEqual("d:4,E:5,F:6", string.Join(",", objects.ResultItems[1].Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        }
    }
}
