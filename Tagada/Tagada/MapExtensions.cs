using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Reflection;

namespace Tagada
{
    public static class MapExtensions
    {
        public static ITagadaBuilder Map(this IApplicationBuilder app, PathString pathMatch)
        {
            try
            {
                var tagadaSwaggerAssembly = Assembly.Load("Tagada.Swagger");
                if (tagadaSwaggerAssembly != null)
                {
                    var exportedType = tagadaSwaggerAssembly.ExportedTypes.FirstOrDefault(t => t.Name == "MapExtensions");
                    if (exportedType != null)
                    {
                        var mapMethod = exportedType.GetMethod("Map");
                        if (mapMethod != null)
                        {
                            var swaggerTagadaBuilder = mapMethod.Invoke(null, new object[] { app, pathMatch }) as ITagadaBuilder;
                            if (swaggerTagadaBuilder != null)
                            {
                                return swaggerTagadaBuilder;
                            }
                        }
                    }
                }
            }
            catch { }

            return new TagadaBuilder(app, pathMatch);
        }
    }
}
