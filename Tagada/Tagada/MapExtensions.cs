using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Tagada
{
    public static class MapExtensions
    {
        public static TagadaBuilder Map(this IApplicationBuilder app, PathString pathMatch)
        {
            return new TagadaBuilder(app, pathMatch);
        }

        public static void Use(this TagadaBuilder tagadaBuilder)
        {
            tagadaBuilder.CreateRoutes();
        }
    }
}
