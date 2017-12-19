# Tagada

**Be careful, this library is not production-ready yet.**

Tagada is a lightweight framework to create a .NET Web API without effort. And of course it tastes good.

> For those who dream to make an ASP.NET Core Web API in one line of code

## Features

* Add routes based on HTTP methods `GET`, `POST`, `PUT`, `DELETE`
* Add routes based on generic input and output `<TQuery>`, `<TQuery, TResult>`, `<TCommand>`, `<TCommand, TResult>`
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
        .Get<GetContactsQuery>("/contacts", GetContacts)
        .Get<GetContactByIdQuery>("/contacts/{id}", GetContactById)
        .Post<CreateContactCommand>("/contacts", CreateContact)
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

### EventStore in a single line of code

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
}

public void Configure(IApplicationBuilder app)
{
    app.Map("/api")
        .Get<GetContactsQuery>("/contacts", GetContacts)
        .Get<GetContactByIdQuery>("/contacts/{id}", GetContactById)
        .Post<CreateContactCommand>("/contacts", CreateContact)
        .AfterEach(route => Events.Add(route))
        .Use();
}
```