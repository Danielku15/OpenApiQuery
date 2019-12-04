using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    [TestClass]
    public class DeltaSerializationTests : SerializationTestBase
    {
        private Delta<T> CreateDelta<T>(Expression<Func<T>> item)
        {
            var delta = new Delta<T>();

            var init = ((MemberInitExpression)item.Body).Bindings;
            foreach (var memberBinding in init)
            {
                switch (memberBinding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        switch (((MemberAssignment)memberBinding).Expression)
                        {
                            case ConstantExpression c:
                                delta.SetValue((PropertyInfo)memberBinding.Member, c.Value);
                                break;
                            case NewArrayExpression a:
                                var l = Expression.Lambda<Func<object>>(a).Compile()();
                                delta.SetValue((PropertyInfo)memberBinding.Member, l);
                                break;
                        }

                        break;
                    case MemberBindingType.MemberBinding:
                    case MemberBindingType.ListBinding:
                    default:
                        throw new ArgumentException("Only assignment bindings are supported");
                }
            }

            return delta;
        }

        [TestMethod]
        public void TestSerialize_Simple()
        {
            var actual = CreateDelta(() => new SimpleClass
            {
                IntProp = 1,
                StringProp = "Hello World"
            });
            VerifySerialize(new Dictionary<string, object>
                {
                    ["intProp"] = 2,
                    ["stringProp"] = "Hello World",
                },
                actual);
        }


        [TestMethod]
        public void TestSerialize_NestedDeltas()
        {
            var actual = new Delta<SimpleNavigation>();

            actual.SetValue(typeof(SimpleNavigation).GetProperty(nameof(SimpleNavigation.Nav1)), CreateDelta(() => new SimpleClass
            {
                IntProp = 1,
                StringProp = "Hello World"
            }));

            actual.SetValue(typeof(SimpleNavigation).GetProperty(nameof(SimpleNavigation.Nav2)), CreateDelta(() => new SimpleClass
            {
                IntProp = 2,
                DoubleProp = 47.12
            }));
            VerifySerialize(new Dictionary<string, object>
                {
                    ["nav1"] = new Dictionary<string, object>
                    {
                        ["intProp"] = 1,
                        ["stringProp"] = "Hello World"
                    },
                    ["nav2"] = new Dictionary<string, object>
                    {
                        ["intProp"] = 2,
                        ["doubleProp"] = 47.12
                    }
                },
                actual);
        }


        [TestMethod]
        public void TestSerialize_ObjectArrays()
        {
            var actual = CreateDelta(() => new ArrayWrapper<SimpleClass>
            {
                Items = new[]
                {
                    new SimpleClass
                    {
                        IntProp = 1,
                        StringProp = "Test"
                    },
                    new SimpleClass
                    {
                        IntProp = 2,
                        DoubleProp = 47.12
                    }
                }
            });
            VerifySerialize(new Dictionary<string, object>
                {
                    ["items"] = new object[]
                    {
                        new
                        {
                            intProp = 1,
                            doubleProp = 0.0,
                            stringProp = "Test"
                        },
                        new
                        {
                            intProp = 2,
                            doubleProp = 47.12,
                            stringProp = (string)null
                        }
                    }
                },
                actual);
        }

    }
}
