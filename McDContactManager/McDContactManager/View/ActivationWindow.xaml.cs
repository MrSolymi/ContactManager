using System.Windows;
using McDContactManager.ViewModel;

namespace McDContactManager.View;

public partial class ActivationWindow : Window
{
    public ActivationWindow()
    {
        InitializeComponent();
        
        Loaded += (_, __) =>
        {
            if (DataContext is ActivationWindowViewModel vm)
            {
                vm.ActivationSucceeded += (_, __2) =>
                {
                    DialogResult = true;
                    Close();
                };
            }
        };
    }
}