using Microsoft.AspNetCore.Mvc;
using Shared.Example;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class CalculateController : Controller
    {
        [HttpGet("{operator}")]
        public int Get(string @operator, int number1, int number2)
        {
            QueriesOrCommands.Add(nameof(CalculateQuery));

            var query = new CalculateQuery { Operator = @operator, Number1 = number1, Number2 = number2 };
            return Calculate(query);
        }
    }
}
