using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McDContactManager.Model;

public class Contact
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; private set; }

    public string Phone { get; private set; }
    
    public string Email { get; private set; }

    public Contact(string name, string phone, string email)
    {
        Name = name;
        Phone = phone;
        Email = email;
    }

    private Contact()
    {
    }

    public override string ToString()
    {
        return $"Contact: Name: {Name} Phone: {Phone} Email: {Email}";
    }
}