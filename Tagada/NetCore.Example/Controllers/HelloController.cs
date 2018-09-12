using Microsoft.AspNetCore.Mvc;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        [HttpGet]
        public string Get()
        {
            QueriesOrCommands.Add("/hello");
            return "Hello world!";
        }
    }
}
