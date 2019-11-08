using Microsoft.AspNetCore.Mvc;
using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly BlogDbContext _context;

        public UsersController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAsync(OpenApiQueryOptions<User> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_context.Users));
        }
    }
}