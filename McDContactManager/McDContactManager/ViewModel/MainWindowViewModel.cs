using System.Collections.ObjectModel;
using System.Windows.Input;
using McDContactManager.data;
using McDContactManager.Model;

namespace McDContactManager.ViewModel;

public class MainWindowViewModel
{
    public ICommand OpenUploadCommand { get; }
    public ICommand LoadContactsCommand { get; }
    
    public ObservableCollection<Contact> Contacts { get; } = new();

    public MainWindowViewModel()
    {
        OpenUploadCommand = new RelayCommand(OpenUploadWindow);
        LoadContactsCommand = new RelayCommand(LoadContactsFromDatabase);
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

        Contacts.Clear();
        foreach (var contact in contactsFromDb)
        {
            Contacts.Add(contact);
        }
    }
}