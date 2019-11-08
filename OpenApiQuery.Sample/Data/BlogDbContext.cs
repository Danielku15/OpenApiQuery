using Microsoft.EntityFrameworkCore;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Data
{
    public sealed class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
    }
}