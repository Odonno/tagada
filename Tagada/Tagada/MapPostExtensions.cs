using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tagada.Swagger;

namespace Tagada
{
    public static class MapPostExtensions
    {
        public static TagadaBuilder Post(this TagadaBuilder tagadaBuilder, string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    action();
                    tagadaBuilder.ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = null });
                });
            }

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = tagadaBuilder.TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Responses = new Dictionary<string, Response>
                    {
                        {
                            "200",
                            new Response
                            {
                                Description = "Success"
                            }
                        }
                    }
                };
            }

            tagadaBuilder.AddRouteAction(addRoute);
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return tagadaBuilder;
        }

        public static TagadaBuilder Post<TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var result = function();
                    await response.WriteJsonAsync(result);

                    tagadaBuilder.ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = result });
                });
            }

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = tagadaBuilder.TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Produces = new List<string>
                    {
                        "text/plain",
                        "application/json",
                        "text/json"
                    },
                    Responses = new Dictionary<string, Response>
                    {
                        {
                            "200",
                            new Response
                            {
                                Description = "Success",
                                Schema = schemaRegistry.GetOrRegister(function.Method.ReturnType)
                            }
                        }
                    }
                };
            }

            tagadaBuilder.AddRouteAction(addRoute);
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return tagadaBuilder;
        }

        public static TagadaBuilder Post<TCommand>(this TagadaBuilder tagadaBuilder, string path, Func<TCommand, object> function) where TCommand : class, new()
        {
            return tagadaBuilder.Post<TCommand, object>(path, function);
        }

        public static TagadaBuilder Post<TCommand, TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>();

                    var commandType = command.GetType();
                    var commandProperties = commandType.GetProperties();
                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);

                            continue;
                        }
                    }

                    var result = function(command);
                    await response.WriteJsonAsync(result);

                    tagadaBuilder.ExecuteAfterRoute(new TagadaRoute { Path = path, Input = command, Result = result });
                });
            }

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = tagadaBuilder.TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>
                    {
                        "application/json-patch+json",
                        "application/json",
                        "text/json",
                        "application/*+json"
                    },
                    Produces = new List<string>
                    {
                        "text/plain",
                        "application/json",
                        "text/json"
                    },
                    Parameters = new List<IParameter>
                    {
                        new BodyParameter
                        {
                            Name = "command",
                            In = "body",
                            Required = true,
                            Schema = schemaRegistry.GetOrRegister(typeof(TCommand))
                        }
                    },
                    Responses = new Dictionary<string, Response>
                    {
                        {
                            "200",
                            new Response
                            {
                                Description = "Success",
                                Schema = schemaRegistry.GetOrRegister(function.Method.ReturnType)
                            }
                        }
                    }
                };
            }

            tagadaBuilder.AddRouteAction(addRoute);
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return tagadaBuilder;
        }
    }
}
