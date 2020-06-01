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
            Assert.AreEqual(2, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().FirstName);
            Assert.AreEqual(null, response.Value.First().LastName);
            Assert.AreEqual("C", response.Value.First().EMail);
            Assert.AreEqual("D", response.Value.ElementAt(1).FirstName);
            Assert.AreEqual(null, response.Value.ElementAt(1).LastName);
            Assert.AreEqual("F", response.Value.ElementAt(1).EMail);
        }

        [TestMethod]
        public async Task TestSelect_SimpleProperties_NoDollar()
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

            var response = await client.GetQueryAsync<User>("/users?select=firstName,email");
            Assert.AreEqual(2, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().FirstName);
            Assert.AreEqual(null, response.Value.First().LastName);
            Assert.AreEqual("C", response.Value.First().EMail);
            Assert.AreEqual("D", response.Value.ElementAt(1).FirstName);
            Assert.AreEqual(null, response.Value.ElementAt(1).LastName);
            Assert.AreEqual("F", response.Value.ElementAt(1).EMail);
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
            Assert.AreEqual(1, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().FirstName);
            Assert.AreEqual("B", response.Value.First().LastName);
            Assert.AreEqual("C", response.Value.First().EMail);
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
                        new Blog
                        {
                            Name = "A1",
                            Description = "A1_Desc"
                        },
                        new Blog
                        {
                            Name = "A2",
                            Description = "A2_Desc"
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs");
            Assert.AreEqual(1, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().Username);
            Assert.AreEqual(null, response.Value.First().Blogs);
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

            var Value = document.RootElement.GetProperty("value");
            Assert.AreEqual(2, Value.GetArrayLength());

            foreach (var item in Value.EnumerateArray())
            {
                Assert.AreEqual("firstname,email",
                    item.EnumerateObject().Where(o => o.Value.ValueKind != JsonValueKind.Null).Select(o => o.Name.ToLowerInvariant()));
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
                        new Blog
                        {
                            Name = "A1",
                            Description = "A1_Desc"
                        },
                        new Blog
                        {
                            Name = "A2",
                            Description = "A2_Desc"
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs/name&$expand=blogs");
            Assert.AreEqual(1, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().Username);
            Assert.AreEqual(null, response.Value.First().FirstName);
            Assert.AreEqual(2, response.Value.First().Blogs.Count);
            Assert.AreEqual("A1", response.Value.First().Blogs.ElementAt(0).Name);
            Assert.AreEqual(null, response.Value.First().Blogs.ElementAt(0).Description);
            Assert.AreEqual("A2", response.Value.First().Blogs.ElementAt(1).Name);
            Assert.AreEqual(null, response.Value.First().Blogs.ElementAt(1).Description);
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
                        new Blog
                        {
                            Name = "A1",
                            Description = "A1_Desc"
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$select=username,blogs/*&$expand=blogs");
            Assert.AreEqual(1, response.Value.Count);
            Assert.AreEqual("A", response.Value.First().Username);
            Assert.AreEqual(null, response.Value.First().LastName);
            Assert.AreEqual(null, response.Value.First().EMail);
            Assert.AreEqual(1, response.Value.First().Blogs.Count);
            Assert.AreEqual("A1", response.Value.First().Blogs.ElementAt(0).Name);
            Assert.AreEqual("A1_Desc", response.Value.First().Blogs.ElementAt(0).Description);
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
                        new Blog
                        {
                            Name = "A1",
                            Description = "A1_Desc"
                        },
                        new Blog
                        {
                            Name = "A2",
                            Description = "A2_Desc"
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            using var response = await client.GetAsync("/users?$select=username,blogs/name&$expand=blogs");
            response.EnsureSuccessStatusCode();
            var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            var Value = document.RootElement.GetProperty("value");

            var user = Value.EnumerateArray().First();
            var blogs = user.GetProperty("blogs");

            Assert.AreEqual(2, blogs.GetArrayLength());

            foreach (var item in blogs.EnumerateArray())
            {
                Assert.AreEqual("name",
                    item.EnumerateObject().Where(o => o.Value.ValueKind != JsonValueKind.Null).Select(o => o.Name.ToLowerInvariant()));
            }
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_SingleCollectionProperty()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog
                        {
                            Name = "A"
                        },
                        new Blog
                        {
                            Name = "B"
                        },
                        new Blog
                        {
                            Name = "C"
                        },
                        new Blog
                        {
                            Name = "D"
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetSingleQueryAsync<User>("/users/1?$expand=blogs");
            Assert.AreEqual("A,B,C,D", string.Join(",", response.Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_NestedCollectionProperty()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog
                        {
                            Name = "A",
                            Posts = Enumerable.Range(1, 1).Select(i => new BlogPost()).ToArray()
                        },
                        new Blog
                        {
                            Name = "B",
                            Posts = Enumerable.Range(1, 2).Select(i => new BlogPost()).ToArray()
                        },
                        new Blog
                        {
                            Name = "C",
                            Posts = Enumerable.Range(1, 3).Select(i => new BlogPost()).ToArray()
                        },
                        new Blog
                        {
                            Name = "D",
                            Posts = Enumerable.Range(1, 4).Select(i => new BlogPost()).ToArray()
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetSingleQueryAsync<User>("/users/1?$expand=blogs($expand=posts)");
            Assert.IsNotNull(response.Blogs);
            Assert.AreEqual("A,B,C,D", string.Join(",", response.Blogs.Select(u => u.Name)));
            Assert.AreEqual(1, response.Blogs.ElementAt(0).Posts.Count);
            Assert.AreEqual(2, response.Blogs.ElementAt(1).Posts.Count);
            Assert.AreEqual(3, response.Blogs.ElementAt(2).Posts.Count);
            Assert.AreEqual(4, response.Blogs.ElementAt(3).Posts.Count);
        }
    }
}
