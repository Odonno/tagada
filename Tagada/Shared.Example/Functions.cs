﻿using System;
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

        public static Func<GetContactsQuery, List<Contact>> GetContacts = _ => Contacts;

        public static Func<GetContactByIdQuery, Contact> GetContactById = 
            query => Contacts.FirstOrDefault(c => c.Id == query.Id);

        public static Func<CreateContactCommand, Contact> CreateContact = command =>
        {
            var newContact = new Contact
            {
                Id = Contacts.Count + 1,
                Name = command.Name
            };

            Contacts.Add(newContact);

            return newContact;
        };

        public static Func<UpdateContactCommand, Contact> UpdateContact = command =>
        {
            var existingContact = Contacts.FirstOrDefault(c => c.Id == command.Id);
            if (existingContact != null)
            {
                existingContact.Name = command.Name;
                return existingContact;
            }

            return null;
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
