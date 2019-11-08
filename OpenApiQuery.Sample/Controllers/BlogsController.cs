using Microsoft.AspNetCore.Mvc;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlogsController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogsController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAsync(OpenApiQueryOptions<Blog> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_context.Blogs));
        }
    }
}