namespace Shared.Example
{
    public class AddNumbersQuery
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    }

    public class GetContactsQuery
    {
    }

    public class GetContactByIdQuery
    {
        public int Id { get; set; }
    }

    public class SearchContactsQuery
    {
        public string Value { get; set; }
    }

    public class CreateContactCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class UpdateContactCommand
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DeleteContactCommand
    {
        public int Id { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
    }
}
