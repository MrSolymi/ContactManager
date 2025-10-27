using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McDContactManager.Model;

public class Contact : INotifyPropertyChanged
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public DateTime DateCreated { get; private set; }
    public DateTime AssignedDate { get; private set; }

    private bool? _published;
    public bool? Published
    {
        get => _published;
        set
        {
            _published = value;
            OnPropertyChanged(nameof(Published));
            OnPropertyChanged(nameof(PublishedDisplay));
        }
    }

    private bool? _hired;
    public bool? Hired
    {
        get => _hired;
        set
        {
            if (value == _hired) return;
            _hired = value;
            OnPropertyChanged(nameof(Hired));
            OnPropertyChanged(nameof(HiredDisplay));
        }
    }

    // Csak megjelenítésre a DataGridben:
    [NotMapped]
    public string PublishedDisplay => Published is null ? "" : (Published.Value ? "Igen" : "Nem");

    [NotMapped]
    public string HiredDisplay => Hired is null ? "" : (Hired.Value ? "Igen" : "Nem");
    public Contact(string name, string phone, string email, DateTime assignedDate)
    {
        Name = name;
        Phone = phone;
        Email = email;
        DateCreated = DateTime.Today;
        AssignedDate = assignedDate;
        Published = null;
        Hired = null;
    }

    private Contact()
    {
    }

    public override string ToString()
    {
        return $"Contact: Name: {Name} Phone: {Phone} Email: {Email}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public override bool Equals(object? obj)
    {
        if (obj is not Contact other)
            return false;

        // Telefonszám vagy email alapján összehasonlítás
        return Phone == other.Phone || Email == other.Email;
    }

    public override int GetHashCode()
    {
        if (string.IsNullOrEmpty(Phone) && string.IsNullOrEmpty(Email))
            return 0;

        var phoneHash = Phone?.GetHashCode() ?? 0;
        var emailHash = Email?.GetHashCode() ?? 0;

        return phoneHash ^ emailHash;
    }

}