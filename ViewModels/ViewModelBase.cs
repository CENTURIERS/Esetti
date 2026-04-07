using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        public virtual string PageTitle => "Aplikacja Esseti";
    }
}
