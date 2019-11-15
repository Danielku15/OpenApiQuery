using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class PaginationTests : SampleTestBase
    {
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
    }
}