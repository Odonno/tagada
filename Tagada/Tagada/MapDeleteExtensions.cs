using System;
using Microsoft.AspNetCore.Routing;

namespace Tagada
{
    public static class MapDeleteExtensions
    {
        public static TagadaBuilder Delete(this TagadaBuilder tagadaBuilder, string path, Func<object> function)
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

            tagadaBuilder.AddRouteAction(addRoute);

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

            tagadaBuilder.AddRouteAction(addRoute);

            return tagadaBuilder;
        }
    }
}
