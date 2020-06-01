using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenApiQuery.Test
{
    [TestClass]
    public class ExpressionParserTypeTests : ExpressionParserTestsBase
    {
        [DataTestMethod]
        [DataRow("3000000000", "3000000000", typeof(long))]
        [DataRow("1000", "1000", typeof(int))]
        [DataRow("47.11", "47.11", typeof(float))]
        [DataRow("3.12E+38", "3.12E+38", typeof(float))]
        [DataRow("4.12E+38", "4.12E+38", typeof(double))]
        [DataRow("'Test'", "\"Test\"", typeof(string))]
        [DataRow("true", "True", typeof(bool))]
        [DataRow("false", "False", typeof(bool))]
        [DataRow("null", "null", typeof(object))]
        public void TestBasicTypes(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [DataTestMethod]
        [DataRow("2019-03-12", "03/12/2019 00:00:00 +00:00", typeof(DateTimeOffset))]
        [DataRow("2019-03-12T16:10:20", "03/12/2019 16:10:20 +00:00", typeof(DateTimeOffset))]
        [DataRow("2019-03-12T16:10:20.123", "03/12/2019 16:10:20 +00:00", typeof(DateTimeOffset))]
        [DataRow("2019-03-12T16:10:20.123Z", "03/12/2019 16:10:20 +00:00", typeof(DateTimeOffset))]
        [DataRow("2019-03-12T16:10:20.123+01:30", "03/12/2019 16:10:20 +01:30", typeof(DateTimeOffset))]
        public void TestDateTimeOffset(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [DataTestMethod]
        [DataRow("[1,2,3]", "new [] {1, 2, 3}", typeof(int[]))]
        [DataRow("['a','b','c']", "new [] {\"a\", \"b\", \"c\"}", typeof(string[]))]
        [DataRow("[1.1,2.2,3.3]", "new [] {1.1, 2.2, 3.3}", typeof(float[]))]
        [DataRow("[1.1,2,3]", "new [] {1.1, Convert(2, Single), Convert(3, Single)}", typeof(float[]))]
        [DataRow("[1, 'Test', 1.1]", "new [] {Convert(1, Object), \"Test\", Convert(1.1, Object)}", typeof(object[]))]
        public void TestArrays(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }
        [DataTestMethod]
        [DataRow("Boolean", "System.Boolean", typeof(Type))]
        [DataRow("Byte", "System.Byte", typeof(Type))]
        [DataRow("Date", "System.DateTime", typeof(Type))]
        [DataRow("DateTimeOffset", "System.DateTimeOffset", typeof(Type))]
        [DataRow("Decimal", "System.Decimal", typeof(Type))]
        [DataRow("Double", "System.Double", typeof(Type))]
        [DataRow("Duration", "System.TimeSpan", typeof(Type))]
        [DataRow("Guid", "System.Guid", typeof(Type))]
        [DataRow("Int16", "System.Int16", typeof(Type))]
        [DataRow("Int32", "System.Int32", typeof(Type))]
        [DataRow("Int64", "System.Int64", typeof(Type))]
        [DataRow("SByte", "System.SByte", typeof(Type))]
        [DataRow("Single", "System.Single", typeof(Type))]
        [DataRow("String", "System.String", typeof(Type))]
        [DataRow("TimeOfDay", "System.TimeSpan", typeof(Type))]
        public void TestTypeNames(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType, true);
        }
    }
}
