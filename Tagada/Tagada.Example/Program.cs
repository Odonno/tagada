using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;
using Tagada.Swagger;

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
                        c.DocumentFilter<TagadaDocumentExtensions>();
                    });
                })
                .Configure(app =>
                {
                    app.Map("/api")
                        .Get("/hello", () => "Hello world!")
                        .Get<GetContactsQuery>("/contacts", GetContacts)
                        .Get<GetContactByIdQuery>("/contacts/{id}", GetContactById)
                        .Post<CreateContactCommand>("/contacts", CreateContact)
                        .Get("/events", () => QueriesOrCommands)
                        .Get("/count", () => GetContactsQueryCount)
                        .AfterEach(route => QueriesOrCommands.Add(route.Input != null ? route.Input.GetType().Name : route.Path))
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

        public static int GetContactsQueryCount = 0;

        public static List<string> QueriesOrCommands = new List<string>();

        public static List<Contact> Contacts = new List<Contact>
        {
            new Contact
            {
                Id = 1,
                Name = "Peter Parker"
            },
            new Contact
            {
                Id = 2,
                Name = "Tony Stark"
            }
        };

        private static List<Contact> GetContacts(GetContactsQuery query)
        {
            return Contacts;
        }

        private static Contact GetContactById(GetContactByIdQuery query)
        {
            return Contacts.FirstOrDefault(c => c.Id == query.Id);
        }

        private static Contact CreateContact(CreateContactCommand command)
        {
            var newContact = new Contact
            {
                Id = Contacts.Count + 1,
                Name = command.Name
            };

            Contacts.Add(newContact);

            return newContact;
        }
    }

    public class GetContactsQuery
    {
    }

    public class GetContactByIdQuery
    {
        public int Id { get; set; }
    }

    public class CreateContactCommand
    {
        public string Name { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
