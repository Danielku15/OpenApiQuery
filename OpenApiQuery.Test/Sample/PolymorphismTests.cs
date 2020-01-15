using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
                    Title = "A",
                    Slug = "hello",
                    Content = "Hello World"
                },
                new StaticTextPage
                {
                    Title = "B",
                    Slug = "contact",
                    Content = "Contact Details"
                },
                new ExternalPage
                {
                    Title = "C",
                    Slug = "google",
                    ExternalUrl = "https://google.com"
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<StaticPage>("/staticPages?$orderby=title asc");
            Assert.IsNull(response.TotalCount, "response.TotalCount == null");
            Assert.AreEqual(3, response.Items.Length);

            Assert.IsInstanceOfType(response.Items[0], typeof(StaticTextPage));
            Assert.AreEqual("Hello World", ((StaticTextPage)response.Items[0]).Content);

            Assert.IsInstanceOfType(response.Items[1], typeof(StaticTextPage));
            Assert.AreEqual("Contact Details", ((StaticTextPage)response.Items[1]).Content);

            Assert.IsInstanceOfType(response.Items[2], typeof(ExternalPage));
            Assert.AreEqual("https://google.com", ((ExternalPage)response.Items[2]).ExternalUrl);
        }
        [TestMethod]
        public async Task TestGetSingle_DeserializesCorrectTypes()
        {
            using var server = SetupSample(testpages: new StaticPage[]
            {
                new StaticTextPage
                {
                    Title = "A",
                    Slug = "hello",
                    Content = "Hello World"
                },
                new StaticTextPage
                {
                    Title = "B",
                    Slug = "contact",
                    Content = "Contact Details"
                },
                new ExternalPage
                {
                    Title = "C",
                    Slug = "google",
                    ExternalUrl = "https://google.com"
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetSingleQueryAsync<StaticPage>("/staticPages/1");
            Assert.IsInstanceOfType(response.ResultItem, typeof(StaticTextPage));
            Assert.AreEqual("Hello World", ((StaticTextPage)response.ResultItem).Content);
        }

        [TestMethod]
        public async Task TestPost_CreateCorrectType()
        {
            using var server = SetupSample();
            using var client = server.CreateClient();

            var result = await client.PostAsync("/staticPages",
                new StringContent(@"
                {
                    ""@odata.type"": ""ExternalPage"",
                    ""title"": ""Test"",
                    ""slug"": ""Test"",
                    ""externalUrl"": ""http://google.com""
                }", Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);

            var response = await client.GetSingleQueryAsync<StaticPage>("/staticPages/1");
            Assert.IsInstanceOfType(response.ResultItem, typeof(ExternalPage));
            Assert.AreEqual("http://google.com", ((ExternalPage)response.ResultItem).ExternalUrl);
        }
    }
}
