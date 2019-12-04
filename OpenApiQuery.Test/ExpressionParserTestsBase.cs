using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Test
{
    public class ExpressionParserTestsBase
    {
        protected void ExpressionTest(string actualQuery, string expectedLinq, Type type, bool tolerantTypeCheck = false)
        {
            var parser = new QueryExpressionParser(actualQuery, new DefaultOpenApiTypeHandler());
            var expr = parser.CommonExpr();

            if (tolerantTypeCheck)
            {
                Assert.IsTrue(type.IsAssignableFrom(expr.Type), "type.IsAssignableFrom(expr.Type)");
            }
            else
            {
                Assert.AreEqual(type, expr.Type);
            }
            Assert.AreEqual(expectedLinq, Convert.ToString(expr, CultureInfo.InvariantCulture));
        }
    }
}
