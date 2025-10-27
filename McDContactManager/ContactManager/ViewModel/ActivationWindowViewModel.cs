using System.ComponentModel;
using ContactManager.Common;
using ContactManager.Model;
using ContactManager.Security;

namespace ContactManager.ViewModel;

public class ActivationWindowViewModel : INotifyPropertyChanged
{
    public RelayCommand ActivationCommand { get; }
    
    private string _activationId = "";
    public string ActivationId
    {
        get => _activationId;
        set
        {
            _activationId = value;
            OnPropertyChanged(nameof(ActivationId));
            ActivationCommand.RaiseCanExecuteChanged();
        }
    }
    
    private string _activationSecret = "";

    public string ActivationSecret
    {
        get => _activationSecret;
        set
        {
            _activationSecret = value;
            OnPropertyChanged(nameof(ActivationSecret));
            ActivationCommand.RaiseCanExecuteChanged();
        }
    }
    
    public event EventHandler? ActivationSucceeded;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public ActivationWindowViewModel()
    {
        ActivationCommand = new RelayCommand(ExecuteActivationCommand, CanExecuteActivationCommand);
    }

    private void ExecuteActivationCommand()
    {
        var inputId = _activationId.Trim();
        var inputSecret = _activationSecret.Trim();
        
        if (KeyValidator.IsValid(inputId, inputSecret))
        {
            var config = ConfigManager.Load() ?? new AppConfig();
            config.ClientId = inputId;
            config.ClientSecret = inputSecret;
            
            ConfigManager.Save(config);
            
            ActivationSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // TODO: hibaüzenet a UI-n (MessageBox / bindingolt error szöveg)
        }
    }
    
    private bool CanExecuteActivationCommand()
    {
        return !string.IsNullOrWhiteSpace(ActivationId) && !string.IsNullOrWhiteSpace(ActivationSecret);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}