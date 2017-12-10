# Tagada

Tagada is a lightweight framework to create a .NET Web API without effort. And of course it tastes good.

> For those who dream to make an ASP.NET Core Web API in one line of code

## Get started

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.Map("/api")
        .Get<GetContactsQuery>("/contacts", GetContacts)
        .Get<GetContactByIdQuery>("/contacts/{id}", GetContactById)
        .Post<CreateContactCommand>("/contacts", CreateContact)
        .Use();
}
```