# Tagada

Tagada is a lightweight framework to create a .NET Web API without effort. And of course it tastes good.

> For those who dream to make an ASP.NET Core Web API in one line of code

## Features

* [x] Add routes based on HTTP methods `GET`, `POST`, `PUT`, `DELETE`
* [x] Add routes based on generic input (and possibly output) `<TQuery>`, `<TQuery, TResult>`, `<TCommand>`, `<TCommand, TResult>`
* [ ] Execute code `AfterEach()` or `AfterEach<T>()`
* [ ] Add swagger documentation

## Get started

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