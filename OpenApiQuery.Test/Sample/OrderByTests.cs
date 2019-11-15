using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class OrderByTests : SampleTestBase
    {
        private static User[] OrderByTestUsers => new[]
        {
            new User {Username = "A", FirstName = "Name2"},
            new User {Username = "B", FirstName = "Name1"},
            new User {Username = "C", FirstName = "Name2"},
            new User {Username = "D", FirstName = "Name1"}
        };

        [TestMethod]
        public async Task TestOrderBy_SingleProperty_DefaultOrderIsAscending()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=username");
            Assert.AreEqual("A,B,C,D", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_SingleProperty_Ascending()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=username asc");
            Assert.AreEqual("A,B,C,D", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_SingleProperty_Desc()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=username desc");
            Assert.AreEqual("D,C,B,A", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_MultiProperty_DefaultOrderIsAscending()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=firstName, username");
            Assert.AreEqual("B,D,A,C", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_MultiProperty_Ascending()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=firstName asc, username asc");
            Assert.AreEqual("B,D,A,C", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_MultiProperty_Descending()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=firstName desc, username desc");
            Assert.AreEqual("C,A,D,B", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }

        [TestMethod]
        public async Task TestOrderBy_MultiProperty_Mixed()
        {
            using var server = SetupSample(OrderByTestUsers);
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$orderby=firstName asc, username desc");
            Assert.AreEqual("D,B,C,A", string.Join(",", response.ResultItems.Select(u => u.Username)));
        }
    }
}