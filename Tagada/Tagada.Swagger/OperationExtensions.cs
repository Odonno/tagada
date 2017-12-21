namespace Tagada.Swagger
{
    internal class OperationExtensions
    {
        internal static string GetOperationPartName(string n)
        {
            if (n.StartsWith("{") && n.EndsWith("}"))
            {
                return "By" + n.Substring(1, n.Length - 2).Capitalize();
            }
            return n.Capitalize();
        }
    }
}
