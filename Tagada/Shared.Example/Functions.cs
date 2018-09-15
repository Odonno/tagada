using System;
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
                FirstName = "Peter",
                LastName = "Parker"
            },
            new Contact
            {
                Id = 2,
                FirstName = "Tony",
                LastName = "Stark"
            }
        };

        public static Func<CalculateQuery, int> Calculate = query =>
        {
            if (query.Operator == "plus")
            {
                return query.Number1 + query.Number2;
            }
            if (query.Operator == "minus")
            {
                return query.Number1 - query.Number2;
            }
            if (query.Operator == "times")
            {
                return query.Number1 * query.Number2;
            }
            if (query.Operator == "divide")
            {
                return query.Number1 / query.Number2;
            }
            return 0;
        };

        public static Func<GetContactsQuery, IEnumerable<Contact>> GetContacts = _ => Contacts;

        public static Func<SearchContactsQuery, IEnumerable<Contact>> SearchContacts =
            query => Contacts.Where(c => c.FullName.ToLower().Contains(query.Value.ToLower()));

        public static Func<GetContactByIdQuery, Contact> GetContactById =
            query => Contacts.FirstOrDefault(c => c.Id == query.Id);
        
        public static Func<CreateContactCommand, Contact> CreateContact = command =>
        {
            var newContact = new Contact
            {
                Id = Contacts.Count + 1,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            Contacts.Add(newContact);

            return newContact;
        };

        public static Func<UpdateContactCommand, Contact> UpdateContact = command =>
        {
            var existingContact = Contacts.FirstOrDefault(c => c.Id == command.Id);
            if (existingContact != null)
            {
                existingContact.FirstName = command.FirstName;
                existingContact.LastName = command.LastName;
                return existingContact;
            }

            return null;
        };

        public static Func<DeleteContactBySearchCommand, bool> DeleteContactBySearch = command =>
        {
            var contactsToRemove = Contacts.Where(c => c.FullName.ToLower().Contains(command.Value.ToLower()));

            if (contactsToRemove.Any())
            {
                Contacts.RemoveAll(c => contactsToRemove.Any(c2 => c2.Id == c.Id));
                return true;
            }

            return false;
        };

        public static Func<DeleteContactCommand, bool> DeleteContact = command =>
        {
            var existingContact = Contacts.FirstOrDefault(c => c.Id == command.Id);
            if (existingContact != null)
            {
                Contacts.Remove(existingContact);
                return true;
            }

            return false;
        };
    }
}
