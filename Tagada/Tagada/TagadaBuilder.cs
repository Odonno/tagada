using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tagada.Swagger")]
namespace Tagada
{
    public partial interface ITagadaBuilder
    {
        ITagadaBuilder Get<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Get<TQuery>(string path, Func<TQuery, object> function) where TQuery : class, new();
        ITagadaBuilder Get<TQuery, TResult>(string path, Func<TQuery, TResult> function) where TQuery : class, new();

        ITagadaBuilder Post(string path, Action action);
        ITagadaBuilder Post<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Post<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new();
        ITagadaBuilder Post<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder Put(string path, Action action);
        ITagadaBuilder Put<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Put<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new();
        ITagadaBuilder Put<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder Delete(string path, Action action);
        ITagadaBuilder Delete<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Delete<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new();
        ITagadaBuilder Delete<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder AfterEach(Action<TagadaRoute> action);
        ITagadaBuilder AfterEach<TQueryOrCommand>(Action<TagadaRoute> action);

        void Use();
    }

    internal class TagadaBuilder : ITagadaBuilder
    {
        private PathString _pathMatch;
        private List<Action<RouteBuilder>> _routeBuilderActions = new List<Action<RouteBuilder>>();
        private List<Action<TagadaRoute>> _afterEachActions = new List<Action<TagadaRoute>>();

        protected IApplicationBuilder App { get; set; }
        protected string TopPath => _pathMatch.Value;

        internal TagadaBuilder(IApplicationBuilder app, PathString pathMatch)
        {
            App = app;
            _pathMatch = pathMatch;
        }

        internal void AddRouteAction(Action<RouteBuilder> addRouteAction)
        {
            _routeBuilderActions.Add(addRouteAction);
        }

        internal void ExecuteAfterRoute(TagadaRoute tagadaRoute)
        {
            foreach (var action in _afterEachActions)
            {
                action(tagadaRoute);
            }
        }

        public virtual ITagadaBuilder Get<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var result = function();
                    await response.WriteJsonAsync(result);

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Get<TQuery>(string path, Func<TQuery, object> function) where TQuery : class, new()
        {
            return Get<TQuery, object>(path, function);
        }
        public virtual ITagadaBuilder Get<TQuery, TResult>(string path, Func<TQuery, TResult> function) where TQuery : class, new()
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

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = query, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Post(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    action();
                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = null });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Post<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var result = function();
                    await response.WriteJsonAsync(result);

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Post<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new()
        {
            return Post<TCommand, object>(path, function);
        }
        public virtual ITagadaBuilder Post<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
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

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = command, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Put(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    action();
                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = null });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Put<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var result = function();
                    await response.WriteJsonAsync(result);

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Put<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new()
        {
            return Put<TCommand, object>(path, function);
        }
        public virtual ITagadaBuilder Put<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
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

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = command, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Delete(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    action();
                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = null });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Delete<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var result = function();
                    await response.WriteJsonAsync(result);

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = null, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Delete<TCommand>(string path, Func<TCommand, object> function) where TCommand : class, new()
        {
            return Delete<TCommand, object>(path, function);
        }
        public virtual ITagadaBuilder Delete<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
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

                    ExecuteAfterRoute(new TagadaRoute { Path = path, Input = command, Result = result });
                });
            }

            AddRouteAction(addRoute);

            return this;
        }

        public ITagadaBuilder AfterEach(Action<TagadaRoute> action)
        {
            _afterEachActions.Add(action);
            return this;
        }
        public ITagadaBuilder AfterEach<TQueryOrCommand>(Action<TagadaRoute> action)
        {
            _afterEachActions.Add((tagadaRoute) =>
            {
                if (tagadaRoute.Input is TQueryOrCommand queryOrCommand)
                {
                    action.Invoke(tagadaRoute);
                }
            });
            return this;
        }

        public virtual void Use()
        {
            App.Map(_pathMatch, subApp =>
            {
                var routeBuilder = new RouteBuilder(subApp);

                foreach (var action in _routeBuilderActions)
                {
                    action(routeBuilder);
                }

                subApp.UseRouter(routeBuilder.Build());
            });
        }
    }
}
