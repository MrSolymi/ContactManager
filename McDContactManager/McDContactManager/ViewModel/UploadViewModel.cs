using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using McDContactManager.Model;
using Microsoft.Win32;

namespace McDContactManager.ViewModel;

public class UploadViewModel
{
    public ICommand SelectFileCommand { get; }
    public ICommand CloseWindowCommand { get; }
    
    public ObservableCollection<Contact> Contacts { get; } = new();

    public UploadViewModel()
    {
        SelectFileCommand = new RelayCommand(SelectFile);
        CloseWindowCommand = new RelayCommand<Window>(w => w?.Close());
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
                var name = Regex.Match(split, @"Név:\s*\*?(.*?)\*?\s*(\r?\n|$)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                var phone = Regex.Match(split, @"Telefon:\s*\*?(.*?)\*?\s*(\r?\n|$)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                var email = Regex.Match(split, @"Email:\s*\*?(.*?)\*?", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

                Console.WriteLine(name);
                Console.WriteLine(phone);
                
                //TODO: itt hasal el az email regexnél
                
                Console.WriteLine(email);

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    Contacts.Add(new Contact(name, email, phone));
                }
            }
            
            MessageBox.Show($"Sikeresen beolvasva {Contacts.Count} kontakt.");
        }
    }
}