using System;
using System.Linq;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Data
{
    public static class DataSeeder
    {
        public static void SeedIfEmpty(BlogDbContext context)
        {
            if (!context.Users.Any())
            {
                Seed(context);
            }
        }

        private static void Seed(BlogDbContext context)
        {
            context.AddRange(Enumerable.Range(1, 20).Select(i => RandomUser(i)));
            context.SaveChanges();
        }

        private static readonly Random Rand = new Random();

        private static User RandomUser(int userId)
        {
            return new User
            {
                Username = $"User{Rand.Next()}",
                FirstName = $"Name{Rand.Next()}",
                LastName = $"LastName{Rand.Next()}",
                EMail = $"test{Rand.Next()}@mail.com",
                Blogs = Enumerable.Range(1, 5).Select(i => RandomBlog(userId)).ToList()
            };
        }

        private static Blog RandomBlog(int userId)
        {
            var blogId = Rand.Next();
            return new Blog
            {
                Name = $"User{userId}, Blog {blogId}",
                Description = $"Random blog{Rand.Next()}",
                Posts = Enumerable.Range(1, 30).Select(i => RandomBlogPost(blogId)).ToList()
            };
        }

        private static BlogPost RandomBlogPost(int blogId)
        {
            return new BlogPost
            {
                Title = $"Random blog {blogId} post {Rand.Next()}",
                Text = "Random blog post"
            };
        }
    }
}
