using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using McDContactManager.data;
using McDContactManager.Model;

namespace McDContactManager.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ICommand OpenUploadCommand { get; }
    public ICommand LoadContactsCommand { get; }
    
    public ObservableCollection<Contact> AllContacts { get; } = new();
    
    public ObservableCollection<Contact> FilteredContacts { get; } = new();
    
    public string NameFilter
    {
        get => _nameFilter;
        set
        {
            _nameFilter = value;
            OnPropertyChanged(nameof(NameFilter));
            ApplyFilters();
        }
    }
    
    private string _nameFilter;

    public MainWindowViewModel()
    {
        OpenUploadCommand = new RelayCommand(OpenUploadWindow);
        LoadContactsCommand = new RelayCommand(LoadContactsFromDatabase);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private static void OpenUploadWindow()
    {
        var window = new View.UploadWindow();
        window.ShowDialog();
    }
    
    private void LoadContactsFromDatabase()
    {
        using var db = new DatabaseContext();
        var contactsFromDb = db.Contacts.ToList();

        AllContacts.Clear();
        FilteredContacts.Clear();
        
        foreach (var contact in contactsFromDb)
        {
            AllContacts.Add(contact);
            FilteredContacts.Add(contact);
        }
    }
    
    private void ApplyFilters()
    {
        FilteredContacts.Clear();

        var query = AllContacts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(NameFilter))
        {
            query = query.Where(c => c.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var contact in query)
        {
            FilteredContacts.Add(contact);
        }
    }
}