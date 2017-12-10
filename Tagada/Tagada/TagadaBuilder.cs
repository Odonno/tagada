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
        private List<Action<TagadaRoute>> _afterEachActions = new List<Action<TagadaRoute>>();

        internal TagadaBuilder(IApplicationBuilder app, PathString pathMatch)
        {
            _app = app;
            _pathMatch = pathMatch;
        }

        internal void AddRouteAction(Action<RouteBuilder> addRouteAction)
        {
            _routeBuilderActions.Add(addRouteAction);
        }

        internal void AddAfterEachAction(Action<TagadaRoute> afterEachAction)
        {
            _afterEachActions.Add(afterEachAction);
        }
        internal void AddAfterEachAction<TQueryOrCommand>(Action<TagadaRoute> afterEachAction)
        {
            _afterEachActions.Add((tagadaRoute) => 
            {
                if (tagadaRoute.Input is TQueryOrCommand queryOrCommand)
                {
                    afterEachAction.Invoke(tagadaRoute);
                }
            });
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

        internal void ExecuteAfterRoute(TagadaRoute tagadaRoute)
        {
            foreach (var action in _afterEachActions)
            {
                action(tagadaRoute);
            }
        }
    }
}
