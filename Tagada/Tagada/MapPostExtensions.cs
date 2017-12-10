using System;
using Microsoft.AspNetCore.Routing;

namespace Tagada
{
    public static class MapPostExtensions
    {
        public static TagadaBuilder Post(this TagadaBuilder tagadaBuilder, string path, Func<object> function)
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

            tagadaBuilder.AddRouteAction(addRoute);

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

            tagadaBuilder.AddRouteAction(addRoute);

            return tagadaBuilder;
        }
    }
}
