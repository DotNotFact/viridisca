using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Auth;

namespace ViridiscaUi.Views.Auth
{
    public partial class LoginView : ReactiveUserControl<LoginViewModel>
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 