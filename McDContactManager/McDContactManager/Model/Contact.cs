namespace McDContactManager.Model;

public class Contact
{
    private string Name { get; }

    private string Email { get; }

    private string Phone { get; }

    public Contact(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }
    
}