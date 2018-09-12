using System;

namespace Tagada
{
    public class TagadaRoute
    {
        public string HttpVerb { get; set; }
        public string Path { get; set; }

        public bool HasInput => InputType != null;
        public Type InputType { get; set; }

        public bool HasResult => ResultType != null;
        public Type ResultType { get; set; }
    }
}
