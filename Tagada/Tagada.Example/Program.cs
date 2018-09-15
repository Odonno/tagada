using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Shared.Example;
using Tagada.Swagger;
using static Shared.Example.Functions;

namespace Tagada.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(s => s.AddRouting())
                .ConfigureServices(s => s.AddMvc())
                .ConfigureServices(s =>
                {
                    s.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                        c.GenerateTagadaSwaggerDoc();
                    });
                })
                .Configure(app =>
                {
                    app.Map("/api")
                        .Get("/hello", () => "Hello world!")
                        .Get("/add/{number1}/{number2}", (AddNumbersQuery query) => query.Number1 + query.Number2)
                        .Get("/calculate/{operator}", Calculate)
                        .Get("/contacts", GetContacts)
                        .Get("/contacts/search", SearchContacts)
                        .Get("/contacts/{id}", GetContactById)
                        .Post("/contacts", CreateContact)
                        .Put("/contacts", UpdateContact)
                        .Delete("/contacts/search", DeleteContactBySearch)
                        .Delete("/contacts/{id}", DeleteContact)
                        .Get("/events", () => QueriesOrCommands)
                        .Get("/count", () => GetContactsQueryCount)
                        .AfterEach(routeResult => QueriesOrCommands.Add(routeResult.Input?.GetType().Name ?? routeResult.Path))
                        .AfterEach<GetContactsQuery>(_ => GetContactsQueryCount++)
                        .AddSwagger()
                        .Use();

                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    });
                })
                .Build()
                .Run();
        }
    }
}
