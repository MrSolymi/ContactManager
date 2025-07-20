using System.Collections;
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
    public RelayCommand MarkPublishedCommand { get; }
    
    public ObservableCollection<Contact> AllContacts { get; } = new();
    public ObservableCollection<Contact> FilteredContacts { get; } = new();
    public ObservableCollection<Contact> SelectedContacts { get; } = new();

    
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
    
    public string PhoneFilter
    {
        get => _phoneFilter;
        set
        {
            _phoneFilter = value;
            OnPropertyChanged(nameof(PhoneFilter));
            ApplyFilters();
        }
    }

    public string EmailFilter
    {
        get => _emailFilter;
        set
        {
            _emailFilter = value;
            OnPropertyChanged(nameof(EmailFilter));
            ApplyFilters();
        }
    }
    
    public DateTime? DateFrom
    {
        get => _dateFrom;
        set
        {
            _dateFrom = value;
            OnPropertyChanged(nameof(DateFrom));
            ApplyFilters();
        }
    }
    
    public DateTime? DateTo
    {
        get => _dateTo;
        set
        {
            _dateTo = value;
            OnPropertyChanged(nameof(DateTo));
            ApplyFilters();
        }
    }
    
    private string _nameFilter;
    private string _phoneFilter;
    private string _emailFilter;
    private DateTime? _dateFrom;
    private DateTime? _dateTo = DateTime.Today;
    public MainWindowViewModel()
    {
        OpenUploadCommand = new RelayCommand(OpenUploadWindow);
        LoadContactsCommand = new RelayCommand(LoadContactsFromDatabase);
        
        
        MarkPublishedCommand = new RelayCommand(ExecuteMarkPublished, CanExecuteMarkPublished);
        SelectedContacts.CollectionChanged += (s, e) =>
        {
            MarkPublishedCommand.RaiseCanExecuteChanged();
        };
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
        //TODO: handle non existent db
        
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
        
        if (!string.IsNullOrWhiteSpace(PhoneFilter))
        {
            query = query.Where(c => c.Phone.Contains(PhoneFilter, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(EmailFilter))
        {
            query = query.Where(c => c.Email.Contains(EmailFilter, StringComparison.OrdinalIgnoreCase));
        }
        
        if (DateFrom.HasValue)
        {
            query = query.Where(c => c.DateCreated >= DateFrom.Value);
        }
        if (DateTo.HasValue)
        {
            query = query.Where(c => c.DateCreated <= DateTo.Value);
        }

        foreach (var contact in query)
        {
            FilteredContacts.Add(contact);
        }
    }
    
    private bool CanExecuteMarkPublished()
    {
        return SelectedContacts != null && SelectedContacts.Count > 0;
    }
    
    private void ExecuteMarkPublished()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count == 0) return;

        using var db = new DatabaseContext();

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Published = true;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts)
        {
            if (selected.Any(s => s.Id == contact.Id))
            {
                contact.Published = true;
            }
        }
    }
}