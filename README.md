# <span style="margin-bottom: -5px;"><img src="https://github.com/Odonno/tagada/blob/master/Images/tagada.png?raw=true" width="30" height="30" /></span> Tagada

[![CodeFactor](https://www.codefactor.io/repository/github/odonno/tagada/badge)](https://www.codefactor.io/repository/github/odonno/tagada)

| Package        | Versions                                                                                                         |
|----------------|---------------------------------------------------------------------------------------------------------------|
| Tagada         | [![NuGet](https://img.shields.io/nuget/v/Tagada.svg)](https://www.nuget.org/packages/Tagada/)                 |
| Tagada.Swagger | [![NuGet](https://img.shields.io/nuget/v/Tagada.Swagger.svg)](https://www.nuget.org/packages/Tagada.Swagger/) |
| Visual Studio extension (project templates)  | [![VSMarketplace](https://img.shields.io/vscode-marketplace/v/odonno.tagada-extensions.svg)](https://marketplace.visualstudio.com/items?itemName=odonno.tagada-extensions) |              

Tagada is a lightweight functional framework to create a .NET Core Web API without effort. And of course it tastes good.

> For those who dream to make an ASP.NET Core Web API in one line of code

## Features

* Add routes based on HTTP methods `GET`, `POST`, `PUT`, `DELETE`
* Add routes based on generic input and output `<TQuery, TResult>` or `<TCommand, TResult>`
* Execute code `BeforeEach()` or `BeforeEach<T>()`
* Execute code `AfterEach()` or `AfterEach<T>()`
* Add swagger documentation

## Get started

### A simple Hello World

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
}

public void Configure(IApplicationBuilder app)
{
    app.Map("/api")
        .Get("/hello", () => "Hello world!")
        .Use();
}
```

### Add Swagger documentation

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();

    services.AddSwaggerGen(c =>
    {
        c.GenerateTagadaSwaggerDoc();
    });
}

public void Configure(IApplicationBuilder app)
{
    app.Map("/api")
        .Get("/hello", () => "Hello world!")
        .AddSwagger()
        .Use();
}
```

### CQRS-ready

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
}

public void Configure(IApplicationBuilder app)
{
    app.Map("/api")
        .Get("/contacts", GetContacts)
        .Get("/contacts/{id}", GetContactById)
        .Post("/contacts", CreateContact)
        .Use();
}
```

```csharp
public class GetContactsQuery { }

public class GetContactByIdQuery
{
    public int Id { get; set; }
}

public class CreateContactCommand
{
    public string Name { get; set; }
}
```

### Storing Commands in a single line of code

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
}

public void Configure(IApplicationBuilder app)
{
    app.Map("/api")
        .Get("/contacts", GetContacts)
        .Get("/contacts/{id}", GetContactById)
        .Post("/contacts", CreateContact)
        .AfterEach(routeResult => QueriesOrCommands.Add(routeResult.Input))
        .Use();
}
```

### Writing queries

* Return result without parameters

```csharp
.Get("/hello", () => "Hello world!")
```

* Return result from query (extracted from parameters)

```csharp
.Get("/add/{number1}/{number2}", (AddNumbersQuery query) => query.Number1 + query.Number2)
```

* Return result from query with a function

```csharp
.Get("/contacts", GetContacts)
```

```csharp
public static Func<GetContactsQuery, IEnumerable<Contact>> GetContacts = _ => Contacts;
```

### Writing commands

* Command without result

```csharp
.Post<Command>("/command", Dispatch)
```

```csharp
public static void Dispatch(Command command)
{
    Commands.Add(command);
}
```

* Command with a result

```csharp
.Post("/contacts", CreateContact)
```

```csharp
public static Func<CreateContactCommand, Contact> CreateContact = command =>
{
    var newContact = new Contact
    {
        Id = Contacts.Count + 1,
        FirstName = command.FirstName,
        LastName = command.LastName
    };

    Contacts.Add(newContact);

    return newContact;
};
```

## Resources

List of tools / articles that inspired me to write this library:

* https://www.strathweb.com/2017/01/building-microservices-with-asp-net-core-without-mvc/
* https://github.com/filipw/aspnetcore-api-samples/blob/master/01%20Lightweight%20API%20(no%20MVC)/LightweightApi/Program.cs
* http://www.koderdojo.com/blog/asp-net-core-routing-and-routebuilder-mapget-for-calculating-a-factorial
