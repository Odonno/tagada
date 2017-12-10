using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tagada
{
    internal static class HttpExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        internal static Task WriteJsonAsync<T>(this HttpResponse response, T obj)
        {
            return Task.Run(() =>
            {
                response.ContentType = "application/json";

                using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        jsonWriter.CloseOutput = false;
                        jsonWriter.AutoCompleteOnClose = false;

                        Serializer.Serialize(jsonWriter, obj);
                    }
                }
            });
        }

        internal static async Task<T> ReadFromJsonAsync<T>(this HttpContext httpContext)
        {
            using (var streamReader = new StreamReader(httpContext.Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var obj = Serializer.Deserialize<T>(jsonTextReader);
                var results = new List<ValidationResult>();

                if (Validator.TryValidateObject(obj, new ValidationContext(obj), results))
                {
                    return obj;
                }

                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteJsonAsync(results);

                return default(T);
            }
        }
    }
}
