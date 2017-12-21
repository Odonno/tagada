using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Tagada.Swagger
{
    public class TagadaDocumentExtensions : IDocumentFilter
    {
        private static List<SwaggerOperationFunc> _swaggerOperationFuncs;

        internal static void SetSwaggerOperations(List<SwaggerOperationFunc> swaggerOperationFuncs)
        {
            _swaggerOperationFuncs = swaggerOperationFuncs;
        }

        private static Operation GetSwaggerOperation(IEnumerable<SwaggerOperationFunc> g, ISchemaRegistry schemaRegistry, SwaggerOperationMethod method)
        {
            return g.FirstOrDefault(sof => sof.Method == method)?.AddSwaggerOperation(schemaRegistry);
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (_swaggerOperationFuncs != null)
            {
                var pathItemsTuple = _swaggerOperationFuncs
                    .GroupBy(sof => sof.Path)
                    .Select(g =>
                    {
                        return (key: g.First().Path, pathItem: new PathItem
                        {
                            Get = GetSwaggerOperation(g, context.SchemaRegistry, SwaggerOperationMethod.Get),
                            Post = GetSwaggerOperation(g, context.SchemaRegistry, SwaggerOperationMethod.Post),
                            Put = GetSwaggerOperation(g, context.SchemaRegistry, SwaggerOperationMethod.Put),
                            Delete = GetSwaggerOperation(g, context.SchemaRegistry, SwaggerOperationMethod.Delete)
                        });
                    });

                foreach ((string key, PathItem pathItem) in pathItemsTuple)
                {
                    swaggerDoc.Paths.Add(key, pathItem);
                }
            }
        }
    }
}
