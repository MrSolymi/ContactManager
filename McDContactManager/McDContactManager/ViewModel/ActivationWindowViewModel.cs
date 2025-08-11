using System.ComponentModel;
using McDContactManager.Common;
using McDContactManager.Model;
using McDContactManager.Security;

namespace McDContactManager.ViewModel;

public class ActivationWindowViewModel : INotifyPropertyChanged
{
    public RelayCommand ActivationCommand { get; }
    
    private string _activationKey = "";
    public string ActivationKey
    {
        get => _activationKey;
        set
        {
            _activationKey = value;
            OnPropertyChanged(nameof(ActivationKey));
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
        var input = _activationKey.Trim();
        
        if (KeyValidator.IsValid(input))
        {
            var config = ConfigManager.Load() ?? new AppConfig();
            config.ClientId = input;
            
            ConfigManager.Save(config);
            
            ActivationSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // TODO: hibaüzenet a UI-n (MessageBox / bindingolt error szöveg)
        }
    }
    
    private bool CanExecuteActivationCommand() => !string.IsNullOrWhiteSpace(ActivationKey);
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}