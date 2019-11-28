using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlogPostsController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogPostsController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(OpenApiQueryOptions<BlogPost> queryOptions, CancellationToken cancellationToken)
        {
            return Ok(await queryOptions.ApplyToAsync(_context.BlogPosts, cancellationToken));
        }
    }
}
