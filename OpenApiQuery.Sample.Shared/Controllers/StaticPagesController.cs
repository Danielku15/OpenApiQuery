using OpenApiQuery.Sample.Data;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Sample.Controllers
{
    public class StaticPagesController : OpenApiController<StaticPage>
    {
        public StaticPagesController(BlogDbContext context)
            : base(context, context.StaticPages)
        {
        }
    }
}
