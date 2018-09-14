using Microsoft.AspNetCore.Mvc;
using Shared.Example;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class AddController : Controller
    {
        [HttpGet("{number1}/{number2}")]
        public int Get(int number1, int number2)
        {
            QueriesOrCommands.Add(nameof(AddNumbersQuery));

            var query = new AddNumbersQuery { Number1 = number1, Number2 = number2 };
            return query.Number1 + query.Number2;
        }
    }
}
