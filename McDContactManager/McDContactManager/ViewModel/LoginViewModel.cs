using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using McDContactManager.Model;

namespace McDContactManager.ViewModel;

public class LoginViewModel : INotifyPropertyChanged
{
    public ICommand LoginCommand { get; }

    public string Email
    {
        get =>  _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    private string _email = "";
    private string _password = "";

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(ExecuteLogin);
    }

    private void ExecuteLogin()
    {
        // TODO: OAuth

        MessageBox.Show($"Bejelentkezés: {Email}, {Password}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}