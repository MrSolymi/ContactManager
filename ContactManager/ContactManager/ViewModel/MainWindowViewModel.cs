using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using ContactManager.Common;
using ContactManager.data;
using ContactManager.Model;
using ContactManager.Service;

namespace ContactManager.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public RelayCommand LoginCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand CopySelectedEmailsCommand { get; }
    public RelayCommand MarkPublishedCommand { get; }
    public RelayCommand MarkHiredCommand { get; }
    public RelayCommand MarkNotPublishedCommand { get; }
    public RelayCommand MarkNotHiredCommand { get; }
    public RelayCommand ToggleForeignCommand { get; }
    public RelayCommand DeleteContactsCommand { get; }
    
    public ObservableCollection<Contact> AllContacts { get; } = new();
    public ObservableCollection<Contact> FilteredContacts { get; } = new();
    public ObservableCollection<Contact> SelectedContacts { get; } = new();

    
    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        private set
        {
            if (_isLoggedIn == value) return;
            _isLoggedIn = value;
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    private string _loggedInUser = "";
    public string LoggedInUser
    {
        get => _loggedInUser;
        private set
        {
            if (_loggedInUser == value) return;
            _loggedInUser = value;
            OnPropertyChanged(nameof(LoggedInUser));
            OnPropertyChanged(nameof(StatusText));
        }
    }
    public string StatusText => IsLoggedIn ? $"Bejelentkezve: {LoggedInUser}" : "Nincs bejelentkezve";
    
    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }
    
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
    
    private bool _onlyForeign;
    public bool OnlyForeign
    {
        get => _onlyForeign;
        set
        {
            if (_onlyForeign == value) return;
            _onlyForeign = value;
            OnPropertyChanged(nameof(OnlyForeign));
            ApplyFilters();
        }
    }

    private ICollectionView _view;
    
    public MainWindowViewModel()
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            return;
        
        RefreshCommand = new RelayCommand(async () => await FetchEmailsAsync(), CanFetchEmails);
        LoginCommand = new RelayCommand(async () => await ExecuteLoginCommand(), CanExecuteLoginCommand);

        CopySelectedEmailsCommand = new RelayCommand(ExecuteCopyEmails, CanExecuteCopyEmails);
        
        MarkPublishedCommand = new RelayCommand(ExecuteMarkPublished, CanExecuteMarkPublished);
        MarkHiredCommand = new RelayCommand(ExecuteMarkHired, CanExecuteMarkHired);
        MarkNotPublishedCommand = new RelayCommand(ExecuteMarkNotPublished, CanExecuteMarkNotPublished);
        MarkNotHiredCommand = new RelayCommand(ExecuteMarkNotHired, CanExecuteMarkNotHired);
        
        ToggleForeignCommand = new RelayCommand(ExecuteToggleForeign, CanExecuteToggleForeign);
        DeleteContactsCommand = new RelayCommand(ExecuteDeleteContacts, CanExecuteDeleteContacts);
            
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
        
        var config = ConfigManager.Load();
        if (config != null && !string.IsNullOrEmpty(config.LastUsedSenderAddress))
        {
            SenderEmail = config.LastUsedSenderAddress;
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

        if (DateFrom.HasValue && c.AssignedDate.Date < DateFrom.Value.Date) return false;
        if (DateTo.HasValue && c.AssignedDate.Date > DateTo.Value.Date) return false;

        if (OnlyUnreviewed && !(c.Published == null || c.Hired == null)) return false;
        
        if (OnlyForeign && !c.IsForeign) return false;

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
        ToggleForeignCommand.RaiseCanExecuteChanged();
        DeleteContactsCommand.RaiseCanExecuteChanged();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool CanFetchEmails()
    {
        return AuthServiceGoogle.Gmail != null; // Gmail be van jelentkezve
    }

    private async Task FetchEmailsAsync()
    {
        if (AuthServiceGoogle.Gmail == null)
        {
            MessageBox.Show("Előbb jelentkezz be Gmailbe.");
            return;
        }

        IsRefreshing = true;

        try
        {
            var config = ConfigManager.Load();
            if (config != null)
            {
                config.LastUsedSenderAddress = _senderEmail;
                ConfigManager.Save(config);
            }

            var gmail = new GmailServiceWrapper(AuthServiceGoogle.Gmail);
            var items = await gmail.SearchEmailsFromAsync(SenderEmail, maxMessages: 200);
            var idsWithAtt = items.Where(m => m.HasAttachments).Select(m => m.Id).ToList();
            if (idsWithAtt.Count == 0)
            {
                MessageBox.Show("Nincs csatolmányos levél.");
                return;
            }

            var downloadDir = Path.Combine(AppInitializer.AppFolderPath, "Downloads");
            
            await gmail.DownloadAttachmentsAsync(idsWithAtt, downloadDir, onlyOutlookItems: true);

            var parsedContacts = EmlProcessorService.ProcessEmlFolder(downloadDir);

            var newCount = EmlProcessorService.SaveContactsToDatabase(parsedContacts, out var reapplications);

            LoadContactsFromDatabase(silent: true);
            ApplyFilters();

            var message = $"Feldolgozva {parsedContacts.Count} fájl, ebből {newCount} új kontakt mentve az adatbázisba.";

            if (reapplications.Count > 0)
            {
                message += "\n\nAz alábbi kontaktok már szerepeltek az adatbázisban eltérő jelentkezési dátummal:\n\n";
                foreach (var r in reapplications)
                {
                    message += $"- {r.Name}: korábbi dátum: {r.ExistingAssignedDate:yyyy. MM. dd}, új dátum: {r.NewAssignedDate:yyyy. MM. dd}\n";
                }
            }

            MessageBox.Show(message, "Import eredménye");
            
            ClearDownloadDirectory(downloadDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hiba: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    
    // private void SaveContactsToDatabase(List<Contact> contacts, out int newContactsCount)
    // {
    //     newContactsCount = 0;
    //     
    //     using var db = new DatabaseContext("contacts.db");
    //     using var dummyDb = new DatabaseContext("dummyContacts.db");
    //     db.Database.EnsureCreated();
    //
    //     foreach (var contact in contacts)
    //     {
    //         var alreadyExists = db.Contacts.Any(c => c.Phone == contact.Phone) || db.Contacts.Any(c => c.Email == contact.Email);
    //
    //         if (alreadyExists)
    //         {
    //             continue;
    //         }
    //         
    //         newContactsCount++;
    //         
    //         db.Contacts.Add(contact);
    //         AllContacts.Add(contact);
    //         FilteredContacts.Add(contact);
    //     }
    //     
    //     db.SaveChanges();
    // }

    private bool CanExecuteLoginCommand()
    {
        return AuthServiceGoogle.Gmail == null;
    }
    
    private async Task ExecuteLoginCommand()
    {
        try
        {
            var ok = await AuthServiceGoogle.EnsureSignedInAsync();
            if (!ok)
            {
                MessageBox.Show("Bejelentkezés sikertelen.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // profil lekérés (email cím)
            try
            {
                var profile = await AuthServiceGoogle.Gmail.Users.GetProfile("me").ExecuteAsync();
                LoggedInUser = profile?.EmailAddress ?? "bejelentkezve";
            }
            catch
            {
                LoggedInUser = "bejelentkezve";
            }

            // Gmail „me” – itt nincs külön „/me” mint Graph-nál, de jelöljük logged-in állapotot:
            IsLoggedIn = true;
            // LoggedInUser = "Gmail fiók bejelentkezve";

            LoginCommand.RaiseCanExecuteChanged();
            RefreshCommand.RaiseCanExecuteChanged();
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

    private bool CanExecuteToggleForeign()
    {
        return SelectedContacts.Count == 1;
    }

    private bool CanExecuteDeleteContacts()
    {
        return SelectedContacts.Count == 1;
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

    private void ExecuteToggleForeign()
    {
        var selected = SelectedContacts.ToList();
        
        if (selected.Count != 1) return;
        
        using var db = new DatabaseContext("contacts.db");
        
        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                tracked.IsForeign = !tracked.IsForeign;
            }
        }
        
        db.SaveChanges();
        
        foreach (var contact in FilteredContacts.Where(f => selected.Any(s => s.Id == f.Id)))
            contact.IsForeign = !contact.IsForeign;
        
        ApplyFilters();
    }
    
    private void ExecuteDeleteContacts()
    {
        var selected = SelectedContacts.ToList();
        if (selected.Count == 0)
            return;

        var result = MessageBox.Show(
            $"Biztosan törölni szeretnéd a kijelölt {selected.Count} kontaktot?",
            "Kontaktok törlése",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        using var db = new DatabaseContext("contacts.db");

        foreach (var contact in selected)
        {
            var tracked = db.Contacts.FirstOrDefault(c => c.Id == contact.Id);
            if (tracked != null)
            {
                db.Contacts.Remove(tracked);
            }
        }

        db.SaveChanges();

        foreach (var contact in selected)
        {
            AllContacts.Remove(contact);
            FilteredContacts.Remove(contact);
        }

        SelectedContacts.Clear();
        ApplyFilters();
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

    public static void ClearDownloadDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return;

        // Fájlok törlése
        foreach (var file in Directory.GetFiles(directoryPath))
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Nem sikerült törölni a fájlt: {file} - {ex.Message}");
            }
        }
    }
}