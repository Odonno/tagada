using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tagada.Swagger;

namespace Tagada
{
    public static class MapDeleteExtensions
    {
        public static TagadaBuilder Delete(this TagadaBuilder tagadaBuilder, string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
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
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
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
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return tagadaBuilder;
        }

        public static TagadaBuilder Delete<TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
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
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
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
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return tagadaBuilder;
        }

        public static TagadaBuilder Delete<TCommand>(this TagadaBuilder tagadaBuilder, string path, Func<TCommand, object> function) where TCommand : class, new()
        {
            return tagadaBuilder.Delete<TCommand, object>(path, function);
        }

        public static TagadaBuilder Delete<TCommand, TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = new TCommand();

                    var commandType = command.GetType();
                    var commandProperties = commandType.GetProperties();
                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside query object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);

                            continue;
                        }

                        // copy query params inside query object
                        if (request.Query.ContainsKey(commandProperty.Name))
                        {
                            var stringValues = request.Query[commandProperty.Name];

                            var copiedValue = Convert.ChangeType(stringValues[0], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
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
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var commandType = typeof(TCommand);
                var commandProperties = commandType.GetProperties();

                var pathParameters = operationSplittedNames
                    .Where(n => n.StartsWith("{") && n.EndsWith("}"))
                    .Select(n => n.Substring(1, n.Length - 2));

                var pathProperties = commandProperties.Where(property =>
                {
                    string propertyNameToLower = property.Name.ToLower();
                    return pathParameters.Any(rp => rp == propertyNameToLower);
                });

                var operationParameters = commandProperties.Select(property =>
                {
                    var propertyTypeSchema = schemaRegistry.GetOrRegister(property.PropertyType);

                    if (pathProperties.Contains(property))
                    {
                        return new NonBodyParameter
                        {
                            Name = property.Name.LowerCapitalize(),
                            In = "path",
                            Required = true,
                            Type = propertyTypeSchema.Type,
                            Format = propertyTypeSchema.Format
                        };
                    }

                    return new NonBodyParameter
                    {
                        Name = property.Name.LowerCapitalize(),
                        In = "query",
                        Required = false,
                        Type = propertyTypeSchema.Type,
                        Format = propertyTypeSchema.Format
                    };
                });

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Delete",
                    Tags = new List<string> { operationName },
                    Produces = new List<string>
                    {
                        "text/plain",
                        "application/json",
                        "text/json"
                    },
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
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
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return tagadaBuilder;
        }

        private static string GetOperationPartName(string n)
        {
            if (n.StartsWith("{") && n.EndsWith("}"))
            {
                return "By" + n.Substring(1, n.Length - 2).Capitalize();
            }
            return n.Capitalize();
        }
    }
}
