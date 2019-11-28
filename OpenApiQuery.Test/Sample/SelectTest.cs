using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class SelectTest : SampleTestBase
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

            var response = await client.GetQueryAsync<User>("/users?$select=firstName,email");
            Assert.AreEqual(2, response.ResultItems.Length);
            Assert.AreEqual("A", response.ResultItems[0].FirstName);
            Assert.AreEqual(null, response.ResultItems[0].LastName);
            Assert.AreEqual("C", response.ResultItems[0].EMail);
            Assert.AreEqual("D", response.ResultItems[1].FirstName);
            Assert.AreEqual(null, response.ResultItems[1].LastName);
            Assert.AreEqual("F", response.ResultItems[1].EMail);
        }

        [TestMethod]
        public async Task TestSelect_SimpleProperties_Star()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    FirstName = "A",
                    LastName = "B",
                    EMail = "C"
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=*");
            Assert.AreEqual(1, response.ResultItems.Length);
            Assert.AreEqual("A", response.ResultItems[0].FirstName);
            Assert.AreEqual("B", response.ResultItems[0].LastName);
            Assert.AreEqual("C", response.ResultItems[0].EMail);
        }

        [TestMethod]
        public async Task TestSelect_SimpleProperties_NavigationPropertyNoExpand()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Username = "A",
                    Blogs = new List<Blog>
                    {
                        new Blog {Name = "A1", Description = "A1_Desc"},
                        new Blog {Name = "A2", Description = "A2_Desc"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs");
            Assert.AreEqual(1, response.ResultItems.Length);
            Assert.AreEqual("A", response.ResultItems[0].Username);
            Assert.AreEqual(null, response.ResultItems[0].Blogs);
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

            using var response = await client.GetAsync("/users?$select=firstName,email");
            response.EnsureSuccessStatusCode();

            var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var resultItems = document.RootElement.GetProperty("resultItems");
            Assert.AreEqual(2, resultItems.GetArrayLength());

            foreach (var item in resultItems.EnumerateArray())
            {
                Assert.AreEqual("firstname,email",
                    string.Join(",", item.EnumerateObject().Select(o => o.Name.ToLowerInvariant())));
            }
        }

        [TestMethod]
        public async Task TestSelect_NestedProperties()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Username = "A",
                    Blogs = new List<Blog>
                    {
                        new Blog {Name = "A1", Description = "A1_Desc"},
                        new Blog {Name = "A2", Description = "A2_Desc"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs/name&$expand=blogs");
            Assert.AreEqual(1, response.ResultItems.Length);
            Assert.AreEqual("A", response.ResultItems[0].Username);
            Assert.AreEqual(null, response.ResultItems[0].FirstName);
            Assert.AreEqual(2, response.ResultItems[0].Blogs.Count);
            Assert.AreEqual("A1", response.ResultItems[0].Blogs.ElementAt(0).Name);
            Assert.AreEqual(null, response.ResultItems[0].Blogs.ElementAt(0).Description);
            Assert.AreEqual("A2", response.ResultItems[0].Blogs.ElementAt(1).Name);
            Assert.AreEqual(null, response.ResultItems[0].Blogs.ElementAt(1).Description);
        }

        [TestMethod]
        public async Task TestSelect_NestedProperties_Star()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Username = "A",
                    LastName = "B",
                    EMail = "C",
                    Blogs = new List<Blog>
                    {
                        new Blog {Name = "A1", Description = "A1_Desc"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs/*&$expand=blogs");
            Assert.AreEqual(1, response.ResultItems.Length);
            Assert.AreEqual("A", response.ResultItems[0].Username);
            Assert.AreEqual(null, response.ResultItems[0].LastName);
            Assert.AreEqual(null, response.ResultItems[0].EMail);
            Assert.AreEqual(1, response.ResultItems[0].Blogs.Count);
            Assert.AreEqual("A1", response.ResultItems[0].Blogs.ElementAt(0).Name);
            Assert.AreEqual("A1_Desc", response.ResultItems[0].Blogs.ElementAt(0).Description);
        }

        [TestMethod]
        public async Task TestSelect_NestedProperties_OnlySelectedPropertiesInJson()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Username = "A",
                    Blogs = new List<Blog>
                    {
                        new Blog {Name = "A1", Description = "A1_Desc"},
                        new Blog {Name = "A2", Description = "A2_Desc"}
                    }
                }
            });
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$select=username,blogs/name&$expand=blogs");
            response.EnsureSuccessStatusCode();
            var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var resultItems = document.RootElement.GetProperty("resultItems");

            var user = resultItems.EnumerateArray().First();
            var blogs = user.GetProperty("Blogs");

            Assert.AreEqual(2, blogs.GetArrayLength());

            foreach (var item in blogs.EnumerateArray())
            {
                Assert.AreEqual("name",
                    string.Join(",", item.EnumerateObject().Select(o => o.Name.ToLowerInvariant())));
            }
        }
    }
}
