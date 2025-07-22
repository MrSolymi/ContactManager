using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using McDContactManager.data;
using McDContactManager.Model;
using Microsoft.Win32;

namespace McDContactManager.ViewModel;

public class UploadViewModel
{
    public bool UploadSuccessful { get; private set; }
    public ICommand SelectFileCommand { get; }
    public ICommand CloseWindowCommand { get; }

    private ObservableCollection<Contact> Contacts { get; } = new();

    public UploadViewModel()
    {
        SelectFileCommand = new RelayCommand(SelectFile);
        CloseWindowCommand = new RelayCommand<Window>(w =>
        {
            if (w != null)
            {
                if (UploadSuccessful)
                    w.DialogResult = true;
                else
                    w.Close();
            }
        });
    }

    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Szövegfájlok (*.txt)|*.txt",
            Title = "Szövegfájl kiválasztása"
        };
        
        if (dialog.ShowDialog() == true)
        {
            //MessageBox.Show("Kiválasztott fájl: " + dialog.FileName);
            
            var text = File.ReadAllText(dialog.FileName);
            var splits = Regex.Split(text, @"-{20,}");
            
            Contacts.Clear();
            
            foreach (var split in splits)
            {
                if (!split.Trim().StartsWith("Tárgy")) continue;
                
                var name = Regex.Match(split, @"Név:\s*\*?(.*?)\*?\s*(\r?\n|$)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                var phone = Regex.Match(split, @"Telefon:\s*\*?(.*?)\*?\s*(\r?\n|$)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                
                var emailLineMatch = Regex.Match(split, @"Email:\s*(.*)", RegexOptions.IgnoreCase);
                var rawEmailLine = emailLineMatch.Groups[1].Value.Trim();
                var rawSplits = rawEmailLine.Split('*');
                var email = rawSplits[0] + rawSplits[1].Split(' ')[0].Trim();

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(phone))
                {
                    Contacts.Add(new Contact(name, phone, email));
                }
            }

            int newContactsCount;
            SaveContactsToDatabase(out newContactsCount);
            
            MessageBox.Show($"Sikeresen beolvasva {newContactsCount} kontakt.");
            
            UploadSuccessful = true;
        }
    }

    private void SaveContactsToDatabase(out int newContactsCount)
    {
        newContactsCount = 0;
        
        using var db = new DatabaseContext();
        db.Database.EnsureCreated(); // ez csak akkor hoz létre adatbázist, ha még nincs — jó így

        foreach (var contact in Contacts)
        {
            var alreadyExists = db.Contacts.Any(c => c.Phone == contact.Phone);

            if (alreadyExists) continue;
            
            newContactsCount++;
            db.Contacts.Add(contact);
        }

        db.SaveChanges();
    }
}