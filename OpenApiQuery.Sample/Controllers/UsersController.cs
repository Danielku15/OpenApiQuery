using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> GetAsync(
            OpenApiQueryOptions<User> queryOptions,
            CancellationToken cancellationToken)
        {
            return Ok(await queryOptions.ApplyToAsync(_context.Users, cancellationToken));
        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> PatchAsync(int id, Delta<User> user, CancellationToken cancellationToken)
        {
            var current = await _context.Users.FindAsync(new object[]
                {
                    id
                },
                cancellationToken);
            if (current == null)
            {
                return NotFound();
            }

            user.ApplyPatch(current);

            TryValidateModel(current);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(current);
        }
    }
}
