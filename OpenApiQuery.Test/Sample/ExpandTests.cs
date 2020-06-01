using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class ExpandTests : SampleTestBase
    {
        [TestMethod]
        public async Task TestExpand_NoExpand_SingleCollectionProperty()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "A"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNull(response.Value.First().Blogs);
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
                        new Blog {Name = "A"},
                        new Blog {Name = "B"},
                        new Blog {Name = "C"},
                        new Blog {Name = "D"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("A,B,C,D", string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
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
                        new Blog {Name = "A", Posts = Enumerable.Range(1, 1).Select(i => new BlogPost()).ToArray()},
                        new Blog {Name = "B", Posts = Enumerable.Range(1, 2).Select(i => new BlogPost()).ToArray()},
                        new Blog {Name = "C", Posts = Enumerable.Range(1, 3).Select(i => new BlogPost()).ToArray()},
                        new Blog {Name = "D", Posts = Enumerable.Range(1, 4).Select(i => new BlogPost()).ToArray()}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($expand=posts)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("A,B,C,D", string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
            Assert.AreEqual(1, response.Value.First().Blogs.ElementAt(0).Posts.Count);
            Assert.AreEqual(2, response.Value.First().Blogs.ElementAt(1).Posts.Count);
            Assert.AreEqual(3, response.Value.First().Blogs.ElementAt(2).Posts.Count);
            Assert.AreEqual(4, response.Value.First().Blogs.ElementAt(3).Posts.Count);
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_ExpandFilter()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "Match1"},
                        new Blog {Name = "Match2"},
                        new Blog {Name = "NotMatch3"},
                        new Blog {Name = "NotMatch4"},
                        new Blog {Name = "Match5"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($filter=startswith(name, 'Match'))");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("Match1,Match2,Match5",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_AllOptions()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "Match1"},
                        new Blog {Name = "Match2"},
                        new Blog {Name = "NotMatch3"},
                        new Blog {Name = "NotMatch4"},
                        new Blog {Name = "Match5"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($filter=startswith(name, 'Match');$orderby=name desc;$skip=1;$top=2)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("Match2,Match1",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }
        [TestMethod]
        public async Task TestExpand_WithExpand_AllOptions_NoDollar()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "Match1"},
                        new Blog {Name = "Match2"},
                        new Blog {Name = "NotMatch3"},
                        new Blog {Name = "NotMatch4"},
                        new Blog {Name = "Match5"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?expand=blogs(filter=startswith(name, 'Match');orderby=name desc;skip=1;top=2)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("Match2,Match1",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_ExpandOrderBy()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "D"},
                        new Blog {Name = "C"},
                        new Blog {Name = "B"},
                        new Blog {Name = "A"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($orderby=name)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("A,B,C,D",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_ExpandTop()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "D"},
                        new Blog {Name = "C"},
                        new Blog {Name = "B"},
                        new Blog {Name = "A"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($top=2)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("D,C",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_ExpandSkip()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Blogs = new[]
                    {
                        new Blog {Name = "D"},
                        new Blog {Name = "C"},
                        new Blog {Name = "B"},
                        new Blog {Name = "A"}
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<User>("/users?$expand=blogs($skip=2)");
            Assert.AreEqual(1, response.Value.Count);
            Assert.IsNotNull(response.Value.First().Blogs);
            Assert.AreEqual("B,A",
                string.Join(",", response.Value.First().Blogs.Select(u => u.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_SingleProperty()
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
                            Posts = new[]
                            {
                                new BlogPost(),
                                new BlogPost()
                            }
                        },
                        new Blog
                        {
                            Name = "B",
                            Posts = new[]
                            {
                                new BlogPost()
                            }
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<BlogPost>("/blogPosts?$expand=blog");
            Assert.AreEqual(3, response.Value.Count);
            Assert.AreEqual("A,A,B", string.Join(",", response.Value.Select(p => p.Blog.Name)));
        }

        [TestMethod]
        public async Task TestExpand_WithExpand_NoDollar_SingleProperty()
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
                            Posts = new[]
                            {
                                new BlogPost(),
                                new BlogPost()
                            }
                        },
                        new Blog
                        {
                            Name = "B",
                            Posts = new[]
                            {
                                new BlogPost()
                            }
                        }
                    }
                }
            });
            using var client = server.CreateClient();

            var response = await client.GetQueryAsync<BlogPost>("/blogPosts?expand=blog");
            Assert.AreEqual(3, response.Value.Count);
            Assert.AreEqual("A,A,B", string.Join(",", response.Value.Select(p => p.Blog.Name)));
        }
    }
}
