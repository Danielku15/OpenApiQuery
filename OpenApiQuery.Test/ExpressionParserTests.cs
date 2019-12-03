using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Test
{
    [TestClass]
    public class ExpressionParserTests
    {
        // TODO: test all expressions whether they result in the correct LINQ expression tree.
        [TestMethod]
        public void TestParse_DateTimeOffset()
        {
            TestParse_DateTimeOffset("2019-03-12", new DateTimeOffset(2019, 03, 12, 0, 0, 0, TimeSpan.Zero));
            TestParse_DateTimeOffset("2019-03-12T08:10:20", new DateTimeOffset(2019, 03, 12, 8, 10, 20, TimeSpan.Zero));
            TestParse_DateTimeOffset("2019-03-12T08:10:20.123",
                new DateTimeOffset(2019, 03, 12, 8, 10, 20, 123, TimeSpan.Zero));
            TestParse_DateTimeOffset("2019-03-12T08:10:20.123Z",
                new DateTimeOffset(2019, 03, 12, 8, 10, 20, 123, TimeSpan.Zero));
            TestParse_DateTimeOffset("2019-03-12T08:10:20.123+01:30",
                new DateTimeOffset(2019, 03, 12, 8, 10, 20, 123, new TimeSpan(1, 30, 0)));
        }

        private void TestParse_DateTimeOffset(string str, DateTimeOffset expected)
        {
            var parser = new QueryExpressionParser(str, new DefaultOpenApiTypeHandler());
            parser.PushThis(Expression.Constant(this));
            var expr = parser.CommonExpr();
            parser.PopThis();
            Assert.IsInstanceOfType(((ConstantExpression)expr).Value, typeof(DateTimeOffset));
            Assert.AreEqual(expected, (DateTimeOffset)((ConstantExpression)expr).Value);
        }
    }
}
