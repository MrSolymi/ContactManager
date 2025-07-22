using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McDContactManager.Model;

public class Contact : INotifyPropertyChanged
{
    private bool _published;
    private bool _hired;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; private set; }

    public string Phone { get; private set; }

    public string Email { get; private set; }

    public DateTime DateCreated { get; private set; }

    public bool Published
    {
        get => _published;
        set
        {
            _published = value;
            OnPropertyChanged(nameof(Published));
        }
    }

    public bool Hired
    {
        get => _hired;
        set
        {
            if (value == _hired) return;
            _hired = value;
            OnPropertyChanged(nameof(Hired));
        }
    }

    public Contact(string name, string phone, string email)
    {
        Name = name;
        Phone = phone;
        Email = email;
        DateCreated = DateTime.Today;
        Published = false;
        Hired = false;
    }

    private Contact()
    {
    }

    public override string ToString()
    {
        return $"Contact: Name: {Name} Phone: {Phone} Email: {Email}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}