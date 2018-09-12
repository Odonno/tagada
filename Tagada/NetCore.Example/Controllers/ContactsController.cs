using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shared.Example;
using static Shared.Example.Functions;

namespace NetCore.Example.Controllers
{
    [Route("api/[controller]")]
    public class ContactsController : Controller
    {
        [HttpGet]
        public IEnumerable<Contact> Get()
        {
            QueriesOrCommands.Add(nameof(GetContactsQuery));

            var query = new GetContactsQuery();
            var result = GetContacts(query);

            GetContactsQueryCount++;

            return result;
        }

        [HttpGet("{id}")]
        public Contact Get(int id)
        {
            QueriesOrCommands.Add(nameof(GetContactByIdQuery));

            var query = new GetContactByIdQuery { Id = id };
            return GetContactById(query);
        }

        [HttpPost]
        public Contact Post([FromBody] CreateContactCommand command)
        {
            QueriesOrCommands.Add(nameof(CreateContactCommand));

            return CreateContact(command);
        }
    }
}
