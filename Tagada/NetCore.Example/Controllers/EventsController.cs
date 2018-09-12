using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var result = QueriesOrCommands;

            QueriesOrCommands.Add("/events");

            return result;
        }
    }
}
