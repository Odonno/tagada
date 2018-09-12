using System.Collections.Generic;
using System.Linq;

namespace Shared.Example
{
    public static class Functions
    {
        public static int GetContactsQueryCount = 0;

        public static List<string> QueriesOrCommands = new List<string>();

        public static List<Contact> Contacts = new List<Contact>
        {
            new Contact
            {
                Id = 1,
                Name = "Peter Parker"
            },
            new Contact
            {
                Id = 2,
                Name = "Tony Stark"
            }
        };

        public static List<Contact> GetContacts(GetContactsQuery query)
        {
            return Contacts;
        }

        public static Contact GetContactById(GetContactByIdQuery query)
        {
            return Contacts.FirstOrDefault(c => c.Id == query.Id);
        }

        public static Contact CreateContact(CreateContactCommand command)
        {
            var newContact = new Contact
            {
                Id = Contacts.Count + 1,
                Name = command.Name
            };

            Contacts.Add(newContact);

            return newContact;
        }
    }
}
