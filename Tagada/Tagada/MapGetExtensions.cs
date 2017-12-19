using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tagada.Swagger;

namespace Tagada
{
    public static class MapGetExtensions
    {
        public static TagadaBuilder Get<TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
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
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Get",
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
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

            return tagadaBuilder;
        }

        public static TagadaBuilder Get<TQuery>(this TagadaBuilder tagadaBuilder, string path, Func<TQuery, object> function) where TQuery : class, new()
        {
            return tagadaBuilder.Get<TQuery, object>(path, function);
        }

        public static TagadaBuilder Get<TQuery, TResult>(this TagadaBuilder tagadaBuilder, string path, Func<TQuery, TResult> function) where TQuery : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var query = new TQuery();

                    var queryType = query.GetType();
                    var queryProperties = queryType.GetProperties();
                    foreach (var queryProperty in queryProperties)
                    {
                        // copy route params inside query object
                        if (routeData.Values.ContainsKey(queryProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[queryProperty.Name], queryProperty.PropertyType);
                            queryProperty.SetValue(query, copiedValue);

                            continue;
                        }

                        // copy query params inside query object
                        if (request.Query.ContainsKey(queryProperty.Name))
                        {
                            var stringValues = request.Query[queryProperty.Name];

                            var copiedValue = Convert.ChangeType(stringValues[0], queryProperty.PropertyType);
                            queryProperty.SetValue(query, copiedValue);
                        }
                    }

                    var result = function(query);
                    await response.WriteJsonAsync(result);

                    tagadaBuilder.ExecuteAfterRoute(new TagadaRoute { Path = path, Input = query, Result = result });
                });
            }

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = tagadaBuilder.TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var queryType = typeof(TQuery);
                var queryProperties = queryType.GetProperties();

                var pathParameters = operationSplittedNames
                    .Where(n => n.StartsWith("{") && n.EndsWith("}"))
                    .Select(n => n.Substring(1, n.Length - 2));

                var pathProperties = queryProperties.Where(property =>
                {
                    string propertyNameToLower = property.Name.ToLower();
                    return pathParameters.Any(rp => rp == propertyNameToLower);
                });

                var operationParameters = queryProperties.Select(property =>
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
                        "Get",
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
            tagadaBuilder.AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

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
