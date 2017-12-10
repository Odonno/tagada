using System;

namespace Tagada
{
    public static class AfterEachExtensions
    {
        public static TagadaBuilder AfterEach(this TagadaBuilder tagadaBuilder, Action<TagadaRoute> action)
        {
            tagadaBuilder.AddAfterEachAction(action);
            return tagadaBuilder;
        }

        public static TagadaBuilder AfterEach<TQueryOrCommand>(this TagadaBuilder tagadaBuilder, Action<TagadaRoute> action)
        {
            tagadaBuilder.AddAfterEachAction<TQueryOrCommand>(action);
            return tagadaBuilder;
        }
    }
}
