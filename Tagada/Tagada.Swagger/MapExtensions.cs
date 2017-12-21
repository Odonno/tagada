using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Tagada.Swagger
{
    public static class MapExtensions
    {
        public static ITagadaBuilder Map(this IApplicationBuilder app, PathString pathMatch)
        {
            return new SwaggerTagadaBuilder(app, pathMatch);
        }
    }
}
