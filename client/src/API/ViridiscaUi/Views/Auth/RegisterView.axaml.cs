using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Auth;

namespace ViridiscaUi.Views.Auth
{
    public partial class RegisterView : ReactiveUserControl<RegisterViewModel>
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 