using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Test
{
    [TestClass]
    public class ExpressionParserFunctionTests : ExpressionParserTestsBase
    {
        [DataTestMethod]
        [DataRow("concat('Hello', 'World')", "Concat(new [] {\"Hello\", \"World\"})", typeof(string))]
        [DataRow("concat('A', 'B', 'C')", "Concat(new [] {\"A\", \"B\", \"C\"})", typeof(string))]
        [DataRow("concat([1,2,3], [4,5,6])", "new [] {1, 2, 3}.Concat(new [] {4, 5, 6})", typeof(IEnumerable<int>))]
        [DataRow("concat([1.1,2,3], [4.1,5,6])",
            "new [] {1.1, Convert(2, Single), Convert(3, Single)}.Concat(new [] {4.1, Convert(5, Single), Convert(6, Single)})",
            typeof(IEnumerable<float>))]
        public void TestConcat(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [DataTestMethod]
        [DataRow("contains('Hello World', 'Hello')", "\"Hello World\".Contains(\"Hello\")")]
        [DataRow("contains([1,2,3], 1)", "new [] {1, 2, 3}.Contains(1)")]
        public void TestContains(string actualQuery, string expectedLinq)
        {
            ExpressionTest(actualQuery, expectedLinq, typeof(bool));
        }

        [DataTestMethod]
        [DataRow("endswith('Hello World', 'Hello')", "\"Hello World\".EndsWith(\"Hello\")")]
        public void TestEndsWith(string actualQuery, string expectedLinq)
        {
            ExpressionTest(actualQuery, expectedLinq, typeof(bool));
        }

        [DataTestMethod]
        [DataRow("indexof('Hello World', 'Hello')", "\"Hello World\".IndexOf(\"Hello\")")]
        public void TestIndexOf(string actualQuery, string expectedLinq)
        {
            ExpressionTest(actualQuery, expectedLinq, typeof(int));
        }

        [DataTestMethod]
        [DataRow("length('Hello World')", "\"Hello World\".Length")]
        [DataRow("length([1,2,3])", "new [] {1, 2, 3}.Count()")]
        public void TestLength(string actualQuery, string expectedLinq)
        {
            ExpressionTest(actualQuery, expectedLinq, typeof(int));
        }

        [DataTestMethod]
        [DataRow("startswith('Hello World', 'Hello')", "\"Hello World\".StartsWith(\"Hello\")")]
        public void TestStartsWith(string actualQuery, string expectedLinq)
        {
            ExpressionTest(actualQuery, expectedLinq, typeof(bool));
        }

        [DataTestMethod]
        [DataRow("substring('Hello World', 5)", "\"Hello World\".Substring(5)", typeof(string))]
        [DataRow("substring('Hello World', 5, 5)", "\"Hello World\".Substring(5, 5)", typeof(string))]
        [DataRow("substring([1,2,3], 2)", "new [] {1, 2, 3}.Skip(2)", typeof(IEnumerable<int>))]
        [DataRow("substring([1,2,3], 2, 3)", "new [] {1, 2, 3}.Skip(2).Take(3)", typeof(IEnumerable<int>))]
        public void TestSubstring(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [DataTestMethod]
        [DataRow("tolower('Hello World')", "\"Hello World\".ToLowerInvariant()", typeof(string))]
        [DataRow("toupper('Hello World')", "\"Hello World\".ToUpperInvariant()", typeof(string))]
        [DataRow("trim('Hello World')", "\"Hello World\".Trim()", typeof(string))]
        public void TestStringFunction(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [DataTestMethod]
        [DataRow("date(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Date", typeof(DateTime))]
        [DataRow("time(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.TimeOfDay", typeof(TimeSpan))]
        [DataRow("totaloffsetminutes(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Offset.TotalMinutes", typeof(TimeSpan))]
        [DataRow("day(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Day", typeof(int))]
        [DataRow("month(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Month", typeof(int))]
        [DataRow("year(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Year", typeof(int))]
        [DataRow("hour(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Hour", typeof(int))]
        [DataRow("minute(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Minute", typeof(int))]
        [DataRow("second(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Second", typeof(int))]
        [DataRow("fractionalseconds(2019-12-04T16:17:18)", "12/04/2019 16:17:18 +00:00.Millisecond", typeof(int))]
        [DataRow("maxdatetime()", "12/31/9999 23:59:59 +00:00", typeof(DateTimeOffset))]
        [DataRow("mindatetime()", "01/01/0001 00:00:00 +00:00", typeof(DateTimeOffset))]
        public void TestDateTimeFunctions(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }

        [TestMethod]
        public void TestDateTimeNow()
        {
            var parser = new QueryExpressionParser("now()", new DefaultOpenApiTypeHandler());
            var expr = parser.CommonExpr();

            Assert.AreEqual(typeof(DateTimeOffset), expr.Type);
        }
    }
}
