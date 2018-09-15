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

        [HttpGet("Search")]
        public IEnumerable<Contact> Search(string value)
        {
            QueriesOrCommands.Add(nameof(SearchContactsQuery));

            var query = new SearchContactsQuery { Value = value };
            return SearchContacts(query);
        }

        [HttpPost]
        public Contact Post([FromBody] CreateContactCommand command)
        {
            QueriesOrCommands.Add(nameof(CreateContactCommand));

            return CreateContact(command);
        }

        [HttpPut]
        public Contact Put([FromBody] UpdateContactCommand command)
        {
            QueriesOrCommands.Add(nameof(UpdateContactCommand));

            return UpdateContact(command);
        }

        [HttpDelete("{id}")]
        public bool Delete(int id)
        {
            QueriesOrCommands.Add(nameof(DeleteContactCommand));

            var command = new DeleteContactCommand { Id = id };
            return DeleteContact(command);
        }

        [HttpDelete("Search")]
        public bool DeleteBySearch(string value)
        {
            QueriesOrCommands.Add(nameof(DeleteContactBySearchCommand));

            var command = new DeleteContactBySearchCommand { Value = value };
            return DeleteContactBySearch(command);
        }
    }
}
