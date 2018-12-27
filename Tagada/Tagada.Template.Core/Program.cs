using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;
using Tagada;

namespace $safeprojectname$
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; private set; }
        public static readonly Dictionary<string, TodoItem> TodoItems = new Dictionary<string, TodoItem>();

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    HostingEnvironment = builderContext.HostingEnvironment;
                })
                .ConfigureServices(s => s.AddRouting())
                .ConfigureServices(s => s.AddMvc())
                .Configure(app =>
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }

                    app.Map("/api")
                        .Get("/todo/list", GetTodoList)
                        .Get("/todo/{id}", GetTodoItemById)
                        .Post("/todo/add", AddTodoItem)
                        .Post("/todo/update/{id}", UpdateTodoItem)
                        .Delete("/todo/{id}", RemoveTodoItem)
                        .Use();
                })
                .Build()
                .Run();
        }

        public static Func<GetTodoListQuery, IEnumerable<TodoItem>> GetTodoList = _ => TodoItems.Values.ToList();

        public static Func<GetTodoItemByIdQuery, TodoItem> GetTodoItemById =
            query => TodoItems.GetValueOrDefault(query.Id);

        public static Func<AddTodoItemCommand, TodoItem> AddTodoItem = command =>
        {
            string newId = Guid.NewGuid().ToString();
            var newItem = new TodoItem
            {
                Id = newId,
                Title = command.Title,
                Completed = command.Completed
            };

            bool success = TodoItems.TryAdd(newId, newItem);

            if (success)
                return newItem;
            return null;
        };

        public static Func<UpdateTodoItemCommand, TodoItem> UpdateTodoItem = command =>
        {
            TodoItems.TryGetValue(command.Id, out var existingItem);
            if (existingItem != null)
            {
                existingItem.Title = command.Title;
                existingItem.Completed = command.Completed;
                return existingItem;
            }

            return null;
        };

        public static Func<RemoveTodoItemCommand, bool> RemoveTodoItem = command =>
        {
            return TodoItems.Remove(command.Id);
        };
    }

    public class TodoItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }

    public class GetTodoListQuery { }

    public class GetTodoItemByIdQuery
    {
        public string Id { get; set; }
    }

    public class AddTodoItemCommand
    {
        public string Title { get; set; }
        public bool Completed { get; set; }
    }

    public class UpdateTodoItemCommand
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }

    public class RemoveTodoItemCommand
    {
        public string Id { get; set; }
    }
}
