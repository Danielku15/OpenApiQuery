using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    public class BlogPostsController : OpenApiController<BlogPost>
    {
        public BlogPostsController(BlogDbContext context)
            : base(context, context.BlogPosts)
        {
        }
    }
}
