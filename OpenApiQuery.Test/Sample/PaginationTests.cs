using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class PaginationTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestSkip_MultipleTimes_BadRequest()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$skip=1&$skip=2");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestSkip_InRange_ReturnsRest()
        {
            const int testUserCount = 10;
            const int skipCount = 3;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$skip={skipCount}");
            Assert.AreEqual(testUserCount - skipCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestSkip_InRange_NoDollar_ReturnsRest()
        {
            const int testUserCount = 10;
            const int skipCount = 3;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?skip={skipCount}");
            Assert.AreEqual(testUserCount - skipCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestSkip_OutOfRange_ReturnsEmpty()
        {
            const int testUserCount = 10;
            const int skipCount = 11;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$skip={skipCount}");
            Assert.AreEqual(0, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestTop_InRange_ReturnsTop()
        {
            const int testUserCount = 10;
            const int topCount = 3;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$top={topCount}");
            Assert.AreEqual(topCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestTop_OutOfRange_ReturnsAll()
        {
            const int testUserCount = 10;
            const int skipCount = 20;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$top={skipCount}");
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestTop_MultipleTimes_BadRequest()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$top=1&$top=2");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestSkipTop_InRange_ReturnsItems()
        {
            const int testUserCount = 10;
            const int skipCount = 2;
            const int topCount = 2;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$skip={skipCount}&$top={topCount}");
            Assert.AreEqual(topCount, response.ResultItems.Length);
        }


        [TestMethod]
        public async Task TestSkipTop_OutOfRange_ReturnsRest()
        {
            const int testUserCount = 10;
            const int skipCount = 5;
            const int topCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$skip={skipCount}&$top={topCount}");
            Assert.AreEqual(5, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestSkipTop_OutOfRange_ReturnsEmpty()
        {
            const int testUserCount = 10;
            const int skipCount = 20;
            const int topCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$skip={skipCount}&$top={topCount}");
            Assert.AreEqual(0, response.ResultItems.Length);
        }


    }
}
