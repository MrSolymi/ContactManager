using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Azure.Core;
using McDContactManager.data;
using McDContactManager.Model;
using McDContactManager.Service;

namespace McDContactManager.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public RelayCommand LoginCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand CopySelectedEmailsCommand { get; }
    public RelayCommand MarkPublishedCommand { get; }
    public RelayCommand MarkHiredCommand { get; }
    public RelayCommand MarkNotPublishedCommand { get; }
    public RelayCommand MarkNotHiredCommand { get; }
    
    public ObservableCollection<Contact> AllContacts { get; } = new();
    public ObservableCollection<Contact> FilteredContacts { get; } = new();
    public ObservableCollection<Contact> SelectedContacts { get; } = new();

    
    private string _nameFilter = "";
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
    
    private string _phoneFilter = "";
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

    private string _emailFilter = "";
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
    
    private DateTime? _dateFrom;
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
    
    private DateTime? _dateTo = DateTime.Today;
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

    private string _senderEmail = "";
    public string? SenderEmail
    {
        get => _senderEmail;
        set
        {
            _senderEmail = value ?? "";
            OnPropertyChanged(nameof(SenderEmail));
            RefreshCommand.RaiseCanExecuteChanged();
        }
    }
    
    private bool _onlyUnreviewed;
    public bool OnlyUnreviewed
    {
        get => _onlyUnreviewed;
        set
        {
            if (_onlyUnreviewed == value) return;
            _onlyUnreviewed = value;
            OnPropertyChanged(nameof(OnlyUnreviewed));
            ApplyFilters();
        }
    }

    private ICollectionView _view;
    
    public MainWindowViewModel()
    {
        RefreshCommand = new RelayCommand(async () => await FetchEmailsAsync(), CanFetchEmails);
        LoginCommand = new RelayCommand(async () => await ExecuteLoginCommand(), CanExecuteLoginCommand);

        CopySelectedEmailsCommand = new RelayCommand(ExecuteCopyEmails, CanExecuteCopyEmails);
        
        MarkPublishedCommand = new RelayCommand(ExecuteMarkPublished, CanExecuteMarkPublished);
        MarkHiredCommand = new RelayCommand(ExecuteMarkHired, CanExecuteMarkHired);
        MarkNotPublishedCommand = new RelayCommand(ExecuteMarkNotPublished, CanExecuteMarkNotPublished);
        MarkNotHiredCommand = new RelayCommand(ExecuteMarkNotHired, CanExecuteMarkNotHired);
            
        SelectedContacts.CollectionChanged += (_, e) =>
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
        
        _view = CollectionViewSource.GetDefaultView(FilteredContacts);
        _view.Filter = MatchesFilters;
        
        if (_view is ICollectionViewLiveShaping live)
        {
            live.IsLiveFiltering = true;
            live.LiveFilteringProperties.Add(nameof(Contact.Published));
            live.LiveFilteringProperties.Add(nameof(Contact.Hired));
        }
    }
    
    private bool MatchesFilters(object obj)
    {
        if (obj is not Contact c) return false;

        if (!string.IsNullOrWhiteSpace(NameFilter) &&
            !c.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(PhoneFilter) &&
            !c.Phone.Contains(PhoneFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(EmailFilter) &&
            !c.Email.Contains(EmailFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        if (DateFrom.HasValue && c.DateCreated < DateFrom.Value) return false;
        if (DateTo.HasValue && c.DateCreated > DateTo.Value) return false;

        if (OnlyUnreviewed && !(c.Published == null || c.Hired == null)) return false;

        return true;
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
        
        CopySelectedEmailsCommand.RaiseCanExecuteChanged();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool CanFetchEmails()
    {
        return !string.IsNullOrEmpty(_senderEmail);
    }
    
    private async Task FetchEmailsAsync()
    {
        if (!Application.Current.Properties.Contains("Credential"))
        {
            MessageBox.Show("Nincs bejelentkezve.");
            return;
        }

        var credential = Application.Current.Properties["Credential"] as TokenCredential;
        if (credential == null)
        {
            MessageBox.Show("Credential null.");
            return;
        }

        var graph = new GraphService(credential);
        
        var emailBodies = await graph.GetEmailTextsFromSenderAsync(_senderEmail, top: 200);
        
        var parsedContacts = new List<Contact>();

        foreach (var html in emailBodies)
        {
            var (name, phone, email) = EmailParser.Parse(html);

            if (!string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(phone) &&
                !string.IsNullOrWhiteSpace(email))
            {
                var probNewContact = new Contact(name, phone, email);
                
                if (!parsedContacts.Contains(probNewContact))
                {
                    parsedContacts.Add(new Contact(name, phone, email));
                }
            }
        }

        SaveContactsToDatabase(parsedContacts, out var newContactsCount);

        MessageBox.Show($"Sikeresen beolvasva {newContactsCount} új kontakt.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void SaveContactsToDatabase(List<Contact> contacts, out int newContactsCount)
    {
        newContactsCount = 0;
        
        using var db = new DatabaseContext("contacts.db");
        using var dummyDb = new DatabaseContext("dummyContacts.db");
        db.Database.EnsureCreated();

        foreach (var contact in contacts)
        {
            var alreadyExists = db.Contacts.Any(c => c.Phone == contact.Phone) || db.Contacts.Any(c => c.Email == contact.Email);

            if (alreadyExists)
            {
                continue;
            }
            
            newContactsCount++;
            
            db.Contacts.Add(contact);
            AllContacts.Add(contact);
            FilteredContacts.Add(contact);
        }
        
        db.SaveChanges();
    }

    private bool CanExecuteLoginCommand()
    {
        return !Application.Current.Properties.Contains("Credential");
    }
    
    private async Task ExecuteLoginCommand()
    {
        try
        {
            TokenCredential? credential = AuthService.Credential;


            if (credential != null)
            {
                var token = await credential.GetTokenAsync(
                    new TokenRequestContext(["https://graph.microsoft.com/.default"]),
                    CancellationToken.None
                );

                if (!string.IsNullOrWhiteSpace(token.Token))
                {
                    Application.Current.Properties["Credential"] = credential;
                
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bejelentkezés sikertelen: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

            using var db = new DatabaseContext("contacts.db");
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
        _view.Refresh();
    }

    private bool CanExecuteCopyEmails()
    {
        return SelectedContacts.Count != 0;
    }
    
    private bool CanExecuteMarkPublished()
    {
        var s = BulkStateHelper.GetBulkState(SelectedContacts, c => c.Published);
        return s == BulkState.AllFalse || s == BulkState.AllNull;
    }

    private bool CanExecuteMarkHired()
    {
        var s = BulkStateHelper.GetBulkState(SelectedContacts, c => c.Hired);
        return s == BulkState.AllFalse || s == BulkState.AllNull;
    }

    private bool CanExecuteMarkNotPublished()
    {
        var s = BulkStateHelper.GetBulkState(SelectedContacts, c => c.Published);
        return s == BulkState.AllTrue || s == BulkState.AllNull;
    }
    
    private bool CanExecuteMarkNotHired()
    {
        var s = BulkStateHelper.GetBulkState(SelectedContacts, c => c.Hired);
        return s == BulkState.AllTrue || s == BulkState.AllNull;
    }

    private void ExecuteCopyEmails()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count == 0) return;

        //Console.WriteLine($"Selected contacts: {string.Join(", ", selected.Select(c => c.Email))}");
        
        var selectedContactsEmails = string.Join("\n", selected.Select(c => c.Email));
        
        Clipboard.SetText(selectedContactsEmails);

        Console.WriteLine("Copied to clipboard: " + selectedContactsEmails);
    }
    
    private void ExecuteMarkPublished()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count == 0) return;

        using var db = new DatabaseContext("contacts.db");

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Published = true;
            }
        }

        db.SaveChanges();
        
        foreach (var contact in FilteredContacts.Where(f => selected.Any(s => s.Id == f.Id)))
            contact.Published = true;
    }
    
    private void ExecuteMarkHired()
    {
        var selected = SelectedContacts.ToList();

        if (selected.Count == 0) return;

        using var db = new DatabaseContext("contacts.db");

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Hired = true;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts.Where(f => selected.Any(s => s.Id == f.Id)))
            contact.Hired = true;
    }
    
    private void ExecuteMarkNotPublished()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count == 0) return;

        using var db = new DatabaseContext("contacts.db");

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Published = false;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts.Where(f => selected.Any(s => s.Id == f.Id)))
            contact.Published = false;
    }
    
    private void ExecuteMarkNotHired()
    {
        var selected = SelectedContacts.ToList();

        if (selected.Count == 0) return;

        using var db = new DatabaseContext("contacts.db");

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.Hired = false;
            }
        }

        db.SaveChanges();

        foreach (var contact in FilteredContacts.Where(f => selected.Any(s => s.Id == f.Id)))
            contact.Hired = false;
    }
}