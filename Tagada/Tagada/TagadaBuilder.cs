using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Tagada
{
    public class TagadaBuilder
    {
        private IApplicationBuilder _app;
        private PathString _pathMatch;
        private List<Action<RouteBuilder>> _routeBuilderActions = new List<Action<RouteBuilder>>();

        internal TagadaBuilder(IApplicationBuilder app, PathString pathMatch)
        {
            _app = app;
            _pathMatch = pathMatch;
        }

        internal void AddRouteAction(Action<RouteBuilder> addRoute)
        {
            _routeBuilderActions.Add(addRoute);
        }

        internal void CreateRoutes()
        {
            _app.Map(_pathMatch, subApp =>
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
