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

    public class CreateContactCommand
    {
        public string Name { get; set; }
    }

    public class UpdateContactCommand
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DeleteContactCommand
    {
        public int Id { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
