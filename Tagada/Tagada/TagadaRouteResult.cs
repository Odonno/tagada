namespace Tagada
{
    public class TagadaRouteResult
    {
        public string HttpVerb { get; set; }
        public string Path { get; set; }
        public object Input { get; set; }
        public object Result { get; set; }
    }
}
