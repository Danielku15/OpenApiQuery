using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    public class BlogsController : OpenApiController<Blog>
    {
        public BlogsController(BlogDbContext context)
            : base(context, context.Blogs)
        {
        }
    }
}
