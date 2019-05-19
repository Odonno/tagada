using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Tagada.Swagger.OperationExtensions;

namespace Tagada.Swagger
{
    internal class SwaggerTagadaBuilder : TagadaBuilder
    {
        private List<string> _consumesJson = new List<string>
        {
            "application/json-patch+json",
            "application/json",
            "text/json",
            "application/*+json"
        };

        private List<string> _producesJson = new List<string>
        {
            "text/plain",
            "application/json",
            "text/json"
        };

        private readonly Func<ISchemaRegistry, Type, Dictionary<string, Response>> _createSuccessResponses = 
            (schemaRegistry, returnType) =>
                new Dictionary<string, Response>
                {
                    {
                        "200",
                        new Response
                        {
                            Description = "Success",
                            Schema = schemaRegistry.GetOrRegister(returnType)
                        }
                    }
                };

        private readonly Func<ISchemaRegistry, PropertyInfo[], string[], IEnumerable<NonBodyParameter>> _getNonBodyParametersFromQueryCommand = 
            (schemaRegistry, properties, operationSplittedNames) =>
                {
                    var pathParameters = operationSplittedNames
                        .Where(n => n.StartsWith("{") && n.EndsWith("}"))
                        .Select(n => n.Substring(1, n.Length - 2));

                    var pathProperties = properties.Where(property =>
                    {
                        string propertyNameToLower = property.Name.ToLower();
                        return pathParameters.Any(rp => rp == propertyNameToLower);
                    });

                    return properties.Select(property =>
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
                };

        private readonly Func<ISchemaRegistry, Type, IList<IParameter>> _getCommandParameters =
            (schemaRegistry, commandType) =>
                new List<IParameter>
                {
                    new BodyParameter
                    {
                        Name = "command",
                        In = "body",
                        Required = false,
                        Schema = schemaRegistry.GetOrRegister(commandType)
                    }
                };

        internal List<SwaggerOperationFunc> SwaggerOperationFuncs { get; } = new List<SwaggerOperationFunc>();
        internal bool UseSwagger { get; set; } = false;

        internal SwaggerTagadaBuilder(IApplicationBuilder app, PathString pathMatch) : base(app, pathMatch)
        {
        }

        internal void AddSwaggerOperationFunc(string path, SwaggerOperationMethod method, Func<ISchemaRegistry, Operation> addSwaggerOperation)
        {
            SwaggerOperationFuncs.Add(new SwaggerOperationFunc
            {
                Path = TopPath + path,
                Method = method,
                AddSwaggerOperation = addSwaggerOperation
            });
        }

        public override void Use(JsonSerializer serializer = null)
        {
            if (UseSwagger)
            {
                App.UseSwagger();
            }

            base.Use(serializer);
        }

        public override ITagadaBuilder Get<TResult>(string path, Func<TResult> function)
        {
            base.Get(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                    {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Get",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = new List<IParameter>(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Get<TQuery, TResult>(string path, Func<TQuery, TResult> function)
        {
            base.Get(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var queryProperties = CachedTypes.GetTypeProperties(typeof(TQuery));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, queryProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Get",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder GetAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            base.GetAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Get",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = new List<IParameter>(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder GetAsync<TQuery, TResult>(string path, Func<TQuery, Task<TResult>> function)
        {
            base.GetAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var queryProperties = CachedTypes.GetTypeProperties(typeof(TQuery));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, queryProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Get",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Get, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder Post(string path, Action action)
        {
            base.Post(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Post<TCommand>(string path, Action<TCommand> action)
        {
            base.Post(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Post<TResult>(string path, Func<TResult> function)
        {
            base.Post(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Post<TCommand, TResult>(string path, Func<TCommand, TResult> function)
        {
            base.Post(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder PostAsync(string path, Func<Task> action)
        {
            base.PostAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PostAsync<TCommand>(string path, Func<TCommand, Task> action)
        {
            base.PostAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PostAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            base.PostAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PostAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function)
        {
            base.PostAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Post",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Post, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder Put(string path, Action action)
        {
            base.Put(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Put<TCommand>(string path, Action<TCommand> action)
        {
            base.Put(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Put<TResult>(string path, Func<TResult> function)
        {
            base.Put(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Put<TCommand, TResult>(string path, Func<TCommand, TResult> function)
        {
            base.Put(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder PutAsync(string path, Func<Task> action)
        {
            base.PutAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PutAsync<TCommand>(string path, Func<TCommand, Task> action)
        {
            base.PutAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PutAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            base.PutAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder PutAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function)
        {
            base.PutAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Put",
                    Tags = new List<string> { operationName },
                    Consumes = _consumesJson,
                    Produces = _producesJson,
                    Parameters = _getCommandParameters(schemaRegistry, typeof(TCommand)),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Put, addSwaggerOperation);

            return this;
        }

        public override ITagadaBuilder Delete(string path, Action action)
        {
            base.Delete(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Delete<TCommand>(string path, Action<TCommand> action)
        {
            base.Delete(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, commandProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Delete<TResult>(string path, Func<TResult> function)
        {
            base.Delete(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder Delete<TCommand, TResult>(string path, Func<TCommand, TResult> function)
        {
            base.Delete(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, commandProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        
        public override ITagadaBuilder DeleteAsync(string path, Func<Task> action)
        {
            base.DeleteAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder DeleteAsync<TCommand>(string path, Func<TCommand, Task> action)
        {
            base.DeleteAsync(path, action);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, commandProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
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
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder DeleteAsync<TResult>(string path, Func<Task<TResult>> function)
        {
            base.DeleteAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                string operationName = path.Split("/", StringSplitOptions.RemoveEmptyEntries)[0];

                return new Operation
                {
                    OperationId = topPath.Capitalize() + operationName.Capitalize() + "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
        public override ITagadaBuilder DeleteAsync<TCommand, TResult>(string path, Func<TCommand, Task<TResult>> function)
        {
            base.DeleteAsync(path, function);

            Operation addSwaggerOperation(ISchemaRegistry schemaRegistry)
            {
                string topPath = TopPath.Trim('/');
                var operationSplittedNames = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string operationName = operationSplittedNames[0];

                var commandProperties = CachedTypes.GetTypeProperties(typeof(TCommand));
                var operationParameters = _getNonBodyParametersFromQueryCommand(schemaRegistry, commandProperties, operationSplittedNames);

                return new Operation
                {
                    OperationId = topPath.Capitalize() +
                        string.Join("", operationSplittedNames.Select(n => GetOperationPartName(n))) +
                        "Delete",
                    Tags = new List<string> { operationName },
                    Consumes = new List<string>(),
                    Produces = _producesJson,
                    Parameters = operationParameters.Cast<IParameter>().ToList(),
                    Responses = _createSuccessResponses(schemaRegistry, function.Method.ReturnType)
                };
            }
            AddSwaggerOperationFunc(path, SwaggerOperationMethod.Delete, addSwaggerOperation);

            return this;
        }
    }
}
