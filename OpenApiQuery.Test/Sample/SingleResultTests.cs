using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class SingleResultTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestSelect_SimpleProperties()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    FirstName = "A",
                    LastName = "B",
                    EMail = "C"
                },
                new User
                {
                    FirstName = "D",
                    LastName = "E",
                    EMail = "F"
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetSingleQueryAsync<User>("/users/1?$select=firstName,email");
            Assert.AreEqual("A", response.ResultItem.FirstName);
            Assert.AreEqual(null, response.ResultItem.LastName);
            Assert.AreEqual("C", response.ResultItem.EMail);
        }

        [TestMethod]
        public async Task TestNotFound()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    FirstName = "A",
                    LastName = "B",
                    EMail = "C"
                },
                new User
                {
                    FirstName = "D",
                    LastName = "E",
                    EMail = "F"
                }
            });
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users/1000");
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestSelect_SimpleProperties_OnlySelectedPropertiesInJson()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    FirstName = "A",
                    LastName = "B",
                    EMail = "C"
                },
                new User
                {
                    FirstName = "D",
                    LastName = "E",
                    EMail = "F"
                }
            });
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users/1?$select=firstName,email");
            response.EnsureSuccessStatusCode();

            var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var item = document.RootElement;
            Assert.AreEqual("firstname,email",
                string.Join(",", item.EnumerateObject().Select(o => o.Name.ToLowerInvariant())));
        }
    }
}
