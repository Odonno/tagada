namespace Shared.Example
{
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

    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
