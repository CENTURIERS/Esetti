using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Esseti.ViewModels.Components
{
    public partial class NavigationItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _icon;

        [ObservableProperty]
        private string? _label;

        public ICommand? Command { get; }

        public NavigationItemViewModel(string icon, string label, ICommand command)
        {
            Icon = icon;
            Label = label;
            Command = command;
        }
    }
}
