using System;
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
        [DataRow("concat([1.1,2,3], [4.1,5,6])", "new [] {1.1, Convert(2, Single), Convert(3, Single)}.Concat(new [] {4.1, Convert(5, Single), Convert(6, Single)})", typeof(IEnumerable<float>))]
        public void TestConcat(string actualQuery, string expectedLinq, Type expectedType)
        {
            ExpressionTest(actualQuery, expectedLinq, expectedType);
        }
    }
}
