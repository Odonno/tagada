using Microsoft.AspNetCore.Mvc;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class CountController : Controller
    {
        [HttpGet]
        public int Get()
        {
            QueriesOrCommands.Add("/count");
            return GetContactsQueryCount;
        }
    }
}
