using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Tagada.Swagger
{
    internal class SwaggerOperationFunc
    {
        public string Path { get; set; }
        public SwaggerOperationMethod Method { get; set; }
        public Func<ISchemaRegistry, Operation> AddSwaggerOperation { get; set; }
    }
}
