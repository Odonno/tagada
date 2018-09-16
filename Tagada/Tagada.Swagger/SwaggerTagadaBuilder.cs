using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using static Tagada.Swagger.OperationExtensions;

namespace Tagada.Swagger
{
    internal class SwaggerTagadaBuilder : TagadaBuilder
    {
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

        public override void Use()
        {
            if (UseSwagger)
            {
                App.UseSwagger();
            }

            base.Use();
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
                    Consumes = new List<string>
                    {
                        "application/json-patch+json",
                        "application/json",
                        "text/json",
                        "application/*+json"
                    },
                    Produces = _producesJson,
                    Parameters = new List<IParameter>
                    {
                        new BodyParameter
                        {
                            Name = "command",
                            In = "body",
                            Required = false,
                            Schema = schemaRegistry.GetOrRegister(typeof(TCommand))
                        }
                    },
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
                    Consumes = new List<string>
                    {
                        "application/json-patch+json",
                        "application/json",
                        "text/json",
                        "application/*+json"
                    },
                    Produces = _producesJson,
                    Parameters = new List<IParameter>
                    {
                        new BodyParameter
                        {
                            Name = "command",
                            In = "body",
                            Required = false,
                            Schema = schemaRegistry.GetOrRegister(typeof(TCommand))
                        }
                    },
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
