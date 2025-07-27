using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using McDContactManager.data;
using McDContactManager.Model;

namespace McDContactManager.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ICommand OpenLoginCommand { get; }
    public ICommand OpenUploadCommand { get; }
    public ICommand LoadContactsCommand { get; }
    public RelayCommand MarkPublishedCommand { get; }
    public RelayCommand MarkHiredCommand { get; }
    public RelayCommand MarkNotPublishedCommand { get; }
    public RelayCommand MarkNotHiredCommand { get; }
    
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
    
    private string _nameFilter = "";
    private string _phoneFilter = "";
    private string _emailFilter = "";
    private DateTime? _dateFrom;
    private DateTime? _dateTo = DateTime.Today;
    public MainWindowViewModel()
    {
        OpenLoginCommand = new RelayCommand(OpenLoginWindow);
        OpenUploadCommand = new RelayCommand(OpenUploadWindow);
        LoadContactsCommand = new RelayCommand(() => LoadContactsFromDatabase());
        
        
        MarkPublishedCommand = new RelayCommand(ExecuteMarkPublished, CanExecuteMarkPublished);
        MarkHiredCommand = new RelayCommand(ExecuteMarkHired, CanExecuteMarkHired);
        MarkNotPublishedCommand = new RelayCommand(ExecuteMarkNotPublished, CanExecuteMarkNotPublished);
        MarkNotHiredCommand = new RelayCommand(ExecuteMarkNotHired, CanExecuteMarkNotHired);
            
        SelectedContacts.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (Contact c in e.NewItems)
                {
                    c.PropertyChanged += SelectedContact_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Contact c in e.OldItems)
                {
                    c.PropertyChanged -= SelectedContact_PropertyChanged;
                }
            }

            RaiseAllCanExecuteChanged();
        };
        
        LoadContactsFromDatabase(silent: true);
    }
    
    private void SelectedContact_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Contact.Published) || e.PropertyName == nameof(Contact.Hired))
        {
            RaiseAllCanExecuteChanged();
        }
    }
    
    private void RaiseAllCanExecuteChanged()
    {
        MarkPublishedCommand.RaiseCanExecuteChanged();
        MarkNotPublishedCommand.RaiseCanExecuteChanged();
        MarkHiredCommand.RaiseCanExecuteChanged();
        MarkNotHiredCommand.RaiseCanExecuteChanged();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void OpenUploadWindow()
    {
        // var window = new View.UploadWindow();
        // var result = window.ShowDialog();
        //
        // if (result == true)
        // {
        //     LoadContactsFromDatabase();
        // }
        
        var vm = new UploadViewModel();
        var window = new View.UploadWindow
        {
            DataContext = vm
        };

        window.Closed += (_, _) =>
        {
            if (vm.UploadSuccessful)
            {
                LoadContactsFromDatabase();
            }
        };

        window.ShowDialog();
    }

    private void OpenLoginWindow()
    {
        var window = new View.LoginWindow();
        window.ShowDialog();
    }
    
    private void LoadContactsFromDatabase(bool silent = false)
    {
        try
        {
            if (!File.Exists("contacts.db"))
            {
                if (!silent)
                {
                    MessageBox.Show("Adatbázis nem található.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

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
        catch (Exception ex)
        {
            if (!silent)
                MessageBox.Show($"Hiba történt az adatbázis betöltésekor:\n{ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
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
        if (SelectedContacts.Count == 0) return false;
        
        var canExecute = false;
        foreach (var contact in SelectedContacts)
        {
            if (!contact.Published)
            {
                canExecute = true;
            }
        }
        
        return canExecute;
    }

    private bool CanExecuteMarkHired()
    {
        if (SelectedContacts.Count == 0) return false;
        
        var canExecute = false;
        foreach (var contact in SelectedContacts)
        {
            if (!contact.Hired)
            {
                canExecute = true;
            }
        }
        
        return canExecute;
    }

    private bool CanExecuteMarkNotPublished()
    {
        if (SelectedContacts.Count == 0) return false;

        var canExecute = false;
        foreach (var contact in SelectedContacts)
        {
            if (contact.Published)
            {
                canExecute = true;
            }
        }
        
        return canExecute;
    }
    
    private bool CanExecuteMarkNotHired()
    {
        if (SelectedContacts.Count == 0) return false;
        
        var canExecute = false;
        foreach (var contact in SelectedContacts)
        {
            if (contact.Hired)
            {
                canExecute = true;
            }
        }
        
        return canExecute;
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
    
    private void ExecuteMarkHired()
    {
        var selected = SelectedContacts.ToList();

        if (selected.Count == 0) return;

        using var db = new DatabaseContext();

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Hired = true;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts)
        {
            if (selected.Any(s => s.Id == contact.Id))
            {
                contact.Hired = true; // ez triggereli a UI-t
            }
        }
    }
    
    private void ExecuteMarkNotPublished()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count == 0) return;

        using var db = new DatabaseContext();

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Published = false;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts)
        {
            if (selected.Any(s => s.Id == contact.Id))
            {
                contact.Published = false;
            }
        }
    }
    
    private void ExecuteMarkNotHired()
    {
        var selected = SelectedContacts.ToList();

        if (selected.Count == 0) return;

        using var db = new DatabaseContext();

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Hired = false;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts)
        {
            if (selected.Any(s => s.Id == contact.Id))
            {
                contact.Hired = false; // ez triggereli a UI-t
            }
        }
    }
}