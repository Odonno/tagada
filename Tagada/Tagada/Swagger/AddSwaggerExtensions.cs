namespace Tagada.Swagger
{
    public static class AddSwaggerExtensions
    {
        public static TagadaBuilder AddSwagger(this TagadaBuilder tagadaBuilder)
        {
            tagadaBuilder.AddSwagger();
            return tagadaBuilder;
        }
    }
}
