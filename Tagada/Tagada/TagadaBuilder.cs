﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Tagada.Swagger")]
namespace Tagada
{
    public partial interface ITagadaBuilder
    {
        ITagadaBuilder BeforeEach(Action<TagadaRouteResult> action);
        ITagadaBuilder BeforeEach<TQueryOrCommand>(Action<TagadaRouteResult> action);

        ITagadaBuilder Get<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Get<TQuery, TResult>(string path, Func<TQuery, TResult> function) where TQuery : class, new();

        ITagadaBuilder GetAsync<TResult>(string path, Func<Task<TResult>> function);
        ITagadaBuilder GetAsync<TQuery, TResult>(string path, Func<TQuery, Task<TResult>> function) where TQuery : class, new();

        ITagadaBuilder Post(string path, Action action);
        ITagadaBuilder Post<TCommand>(string path, Action<TCommand> action) where TCommand : class, new();
        ITagadaBuilder Post<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Post<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder PostAsync(string path, Func<Task> action);
        ITagadaBuilder PostAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new();
        ITagadaBuilder PostAsync<TResult>(string path, Func<Task<TResult>> function);
        ITagadaBuilder PostAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new();

        ITagadaBuilder Put(string path, Action action);
        ITagadaBuilder Put<TCommand>(string path, Action<TCommand> action) where TCommand : class, new();
        ITagadaBuilder Put<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Put<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder PutAsync(string path, Func<Task> action);
        ITagadaBuilder PutAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new();
        ITagadaBuilder PutAsync<TResult>(string path, Func<Task<TResult>> function);
        ITagadaBuilder PutAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new();

        ITagadaBuilder Delete(string path, Action action);
        ITagadaBuilder Delete<TCommand>(string path, Action<TCommand> action) where TCommand : class, new();
        ITagadaBuilder Delete<TResult>(string path, Func<TResult> function);
        ITagadaBuilder Delete<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new();

        ITagadaBuilder DeleteAsync(string path, Func<Task> action);
        ITagadaBuilder DeleteAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new();
        ITagadaBuilder DeleteAsync<TResult>(string path, Func<Task<TResult>> function);
        ITagadaBuilder DeleteAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new();

        ITagadaBuilder AfterEach(Action<TagadaRouteResult> action);
        ITagadaBuilder AfterEach<TQueryOrCommand>(Action<TagadaRouteResult> action);

        void Use(JsonSerializer serializer = null);
    }

    internal class TagadaBuilder : ITagadaBuilder
    {
        private PathString _pathMatch;
        private JsonSerializer _serializer;
        private readonly List<TagadaRoute> _routes = new List<TagadaRoute>();
        private readonly List<Action<RouteBuilder>> _routeBuilderActions = new List<Action<RouteBuilder>>();
        private readonly List<Action<TagadaRouteResult>> _beforeEachActions = new List<Action<TagadaRouteResult>>();
        private readonly List<Action<TagadaRouteResult>> _afterEachActions = new List<Action<TagadaRouteResult>>();

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

        internal void ExecuteBeforeRoute(TagadaRouteResult tagadaRouteResult)
        {
            foreach (var action in _beforeEachActions)
            {
                action(tagadaRouteResult);
            }
        }
        internal void ExecuteAfterRoute(TagadaRouteResult tagadaRouteResult)
        {
            foreach (var action in _afterEachActions)
            {
                action(tagadaRouteResult);
            }
        }

        public ITagadaBuilder BeforeEach(Action<TagadaRouteResult> action)
        {
            _beforeEachActions.Add(action);
            return this;
        }
        public ITagadaBuilder BeforeEach<TQueryOrCommand>(Action<TagadaRouteResult> action)
        {
            _beforeEachActions.Add(tagadaRoute =>
            {
                if (tagadaRoute.Input is TQueryOrCommand)
                {
                    action.Invoke(tagadaRoute);
                }
            });
            return this;
        }

        public virtual ITagadaBuilder Get<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = null });

                    var result = function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "GET",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Get<TQuery, TResult>(string path, Func<TQuery, TResult> function) where TQuery : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var query = new TQuery();
                    var queryProperties = CachedTypes.GetTypeProperties(typeof(TQuery));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = query });

                    var result = function(query);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = query, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "GET",
                Path = path,
                InputType = typeof(TQuery),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder GetAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = null });

                    var result = await function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "GET",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder GetAsync<TQuery, TResult>(string path, Func<TQuery, Task<TResult>> function) where TQuery : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapGet(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var query = new TQuery();
                    var queryProperties = CachedTypes.GetTypeProperties(typeof(TQuery));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = query });

                    var result = await function(query);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "GET", Path = path, Input = query, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "GET",
                Path = path,
                InputType = typeof(TQuery),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Post(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null });
                    action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null, Result = null });
                    
                    return Task.CompletedTask;
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Post<TCommand>(string path, Action<TCommand> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command });
                    action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Post<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null });

                    var result = function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Post<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command });

                    var result = function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder PostAsync(string path, Func<Task> action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null });
                    await action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PostAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command });
                    await action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PostAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null });

                    var result = await function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PostAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPost(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command });

                    var result = await function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "POST", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "POST",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Put(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null });
                    action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null, Result = null });
                
                    return Task.CompletedTask;
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Put<TCommand>(string path, Action<TCommand> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command });
                    action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Put<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null });

                    var result = function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Put<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command });

                    var result = function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder PutAsync(string path, Func<Task> action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null });
                    await action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PutAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command });
                    await action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PutAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null });

                    var result = await function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder PutAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapPut(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = await request.HttpContext.ReadFromJsonAsync<TCommand>(_serializer);
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

                    foreach (var commandProperty in commandProperties)
                    {
                        // copy route params inside command object
                        if (routeData.Values.ContainsKey(commandProperty.Name))
                        {
                            var copiedValue = Convert.ChangeType(routeData.Values[commandProperty.Name], commandProperty.PropertyType);
                            commandProperty.SetValue(command, copiedValue);
                        }
                    }

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command });

                    var result = await function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "PUT", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "PUT",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder Delete(string path, Action action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null });
                    action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null, Result = null });
                
                    return Task.CompletedTask;
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Delete<TCommand>(string path, Action<TCommand> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), (request, response, routeData) =>
                {
                    var command = new TCommand();
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command });
                    action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command, Result = null });
                
                    return Task.CompletedTask;
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Delete<TResult>(string path, Func<TResult> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null });

                    var result = function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder Delete<TCommand, TResult>(string path, Func<TCommand, TResult> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = new TCommand();
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command });

                    var result = function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public virtual ITagadaBuilder DeleteAsync(string path, Func<Task> action)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null });
                    await action();
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = null,
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder DeleteAsync<TCommand>(string path, Func<TCommand, Task> action) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = new TCommand();
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command });
                    await action(command);
                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command, Result = null });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = null
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder DeleteAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null });

                    var result = await function();
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = null, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = null,
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }
        public virtual ITagadaBuilder DeleteAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function) where TCommand : class, new()
        {
            void addRoute(RouteBuilder routeBuilder)
            {
                routeBuilder.MapDelete(path.TrimStart('/'), async (request, response, routeData) =>
                {
                    var command = new TCommand();
                    var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));

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

                    ExecuteBeforeRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command });

                    var result = await function(command);
                    await response.WriteJsonAsync(_serializer, result);

                    ExecuteAfterRoute(new TagadaRouteResult { HttpVerb = "DELETE", Path = path, Input = command, Result = result });
                });
            }

            _routes.Add(new TagadaRoute
            {
                HttpVerb = "DELETE",
                Path = path,
                InputType = typeof(TCommand),
                ResultType = typeof(TResult)
            });

            AddRouteAction(addRoute);

            return this;
        }

        public ITagadaBuilder AfterEach(Action<TagadaRouteResult> action)
        {
            _afterEachActions.Add(action);
            return this;
        }
        public ITagadaBuilder AfterEach<TQueryOrCommand>(Action<TagadaRouteResult> action)
        {
            _afterEachActions.Add(tagadaRoute =>
            {
                if (tagadaRoute.Input is TQueryOrCommand)
                {
                    action.Invoke(tagadaRoute);
                }
            });
            return this;
        }

        public virtual void Use(JsonSerializer serializer = null)
        {
            if (serializer == null)
            {
                _serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            }
            else
            {
                _serializer = serializer;
            }

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
