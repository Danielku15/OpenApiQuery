using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    public class UsersController : OpenApiController<User>
    {
        public UsersController(BlogDbContext context)
            : base(context, context.Users)
        {
        }
    }
}
