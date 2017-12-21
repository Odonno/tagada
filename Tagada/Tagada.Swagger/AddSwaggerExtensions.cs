namespace Tagada.Swagger
{
    public static class AddSwaggerExtensions
    {
        public static ITagadaBuilder AddSwagger(this ITagadaBuilder builder)
        {
            if (builder is SwaggerTagadaBuilder tagadaBuilder)
            {
                TagadaDocumentExtensions.SetSwaggerOperations(tagadaBuilder.SwaggerOperationFuncs);
                tagadaBuilder.UseSwagger = true;
            }
            return builder;
        }
    }
}
