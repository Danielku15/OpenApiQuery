using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class FilterTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestFilter_MultipleTimes_BadRequest()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$filter=true&$filter=false");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestFilter_SimpleTrue_ReturnsAll()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$filter=true");
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestFilter_SimpleFalse_ReturnsNone()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$filter=false");
            Assert.AreEqual(0, response.ResultItems.Length);
        }
    }
}