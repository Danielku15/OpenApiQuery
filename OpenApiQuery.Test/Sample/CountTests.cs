using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class CountTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestCount_NotSpecified_MustNotContainCount()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users");
            Assert.IsNull(response.TotalCount, "response.TotalCount == null");
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_MultipleTimes_BadRequest()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$count=true&$count=true");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestCount_WithFalseInQuery_MustNotContainCount()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$count=false");
            Assert.IsNull(response.TotalCount, "response.TotalCount == null");
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_WithTrueInQuery_MustContainCount()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$count=true");
            Assert.AreEqual(testUserCount, response.TotalCount);
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_WithTrueInQuery_NoDollar_MustContainCount()
        {
            const int testUserCount = 10;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?count=true");
            Assert.AreEqual(testUserCount, response.TotalCount);
            Assert.AreEqual(testUserCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_WithPaging_MustContainTotalValue()
        {
            const int testUserCount = 10;
            const int selectCount = 1;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$count=true&$skip=1&$top={selectCount}");
            Assert.AreEqual(testUserCount, response.TotalCount);
            Assert.AreEqual(selectCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_WithFilter_MustContainFilteredValue()
        {
            const int testUserCount = 10;
            const int filterCount = 5;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$count=true&$filter=Id le {filterCount}");
            Assert.AreEqual(filterCount, response.TotalCount);
            Assert.AreEqual(filterCount, response.ResultItems.Length);
        }

        [TestMethod]
        public async Task TestCount_WithFilterAndPaging_MustContainFilteredValue()
        {
            const int testUserCount = 10;
            const int selectCount = 1;
            const int filterCount = 5;
            using var server = SetupSample(Enumerable.Range(1, testUserCount).Select(i => new User()));
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>($"/users?$count=true&$skip=1&$top={selectCount}&$filter=Id le {filterCount}");
            Assert.AreEqual(filterCount, response.TotalCount);
            Assert.AreEqual(selectCount, response.ResultItems.Length);
        }
    }
}
