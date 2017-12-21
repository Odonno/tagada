using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tagada.Swagger
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void GenerateTagadaSwaggerDoc(this SwaggerGenOptions options)
        {
            options.DocumentFilter<TagadaDocumentExtensions>();
        }
    }
}
