using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class PolymorphismTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestGet_DeserializesCorrectTypes()
        {
            using var server = SetupSample(testpages: new StaticPage[]
            {
                new StaticTextPage
                {
                    Title = "Hello",
                    Slug = "hello",
                    Content = "Hello World"
                },
                new StaticTextPage
                {
                    Title = "Contact",
                    Slug = "contact",
                    Content = "Contact Details"
                },
                new ExternalPage
                {
                    Title = "Google",
                    Slug = "google",
                    ExternalUrl = "https://google.com"
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<StaticPage>("/staticPages");
            Assert.IsNull(response.TotalCount, "response.TotalCount == null");
            Assert.AreEqual(3, response.ResultItems.Length);

            Assert.IsInstanceOfType(response.ResultItems[0], typeof(StaticTextPage));
            Assert.IsInstanceOfType(response.ResultItems[1], typeof(StaticTextPage));
            Assert.IsInstanceOfType(response.ResultItems[2], typeof(ExternalPage));
        }
    }
}
