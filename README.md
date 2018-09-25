# <span style="margin-bottom: -5px;"><img src="https://github.com/Odonno/tagada/blob/master/Images/tagada.png?raw=true" width="30" height="30" /></span> Tagada

[![CodeFactor](https://www.codefactor.io/repository/github/odonno/tagada/badge)](https://www.codefactor.io/repository/github/odonno/tagada)

| Package        | NuGet                                                                                                         |
|----------------|---------------------------------------------------------------------------------------------------------------|
| Tagada         | [![NuGet](https://img.shields.io/nuget/v/Tagada.svg)](https://www.nuget.org/packages/Tagada/)                 |
| Tagada.Swagger | [![NuGet](https://img.shields.io/nuget/v/Tagada.Swagger.svg)](https://www.nuget.org/packages/Tagada.Swagger/) |

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

### Storing Events in a single line of code

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
        .AfterEach(routeResult => Events.Add(routeResult))
        .Use();
}
```

## Resources

List of tools / articles that inspired me to write this library:

* https://www.strathweb.com/2017/01/building-microservices-with-asp-net-core-without-mvc/
* https://github.com/filipw/aspnetcore-api-samples/blob/master/01%20Lightweight%20API%20(no%20MVC)/LightweightApi/Program.cs
* http://www.koderdojo.com/blog/asp-net-core-routing-and-routebuilder-mapget-for-calculating-a-factorial
