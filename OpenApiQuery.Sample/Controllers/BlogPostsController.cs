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
        public IActionResult GetAsync(OpenApiQueryOptions<BlogPost> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_context.BlogPosts));
        }
    }
}