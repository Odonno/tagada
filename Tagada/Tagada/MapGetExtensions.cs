using System;
using Microsoft.AspNetCore.Routing;

namespace Tagada
{
    public static class MapGetExtensions
    {
        public static TagadaBuilder Get(this TagadaBuilder tagadaBuilder, string path, Func<object> function)
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

            tagadaBuilder.AddRouteAction(addRoute);

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

            tagadaBuilder.AddRouteAction(addRoute);

            return tagadaBuilder;
        }
    }
}
