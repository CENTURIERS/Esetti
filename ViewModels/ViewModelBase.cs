using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        public virtual string PageTitle => "Aplikacja Esseti";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        partial void OnSearchQueryChanged(string value)
        {
            OnSearchQueryUpdated(value);
        }

        protected virtual void OnSearchQueryUpdated(string value)
        {

        }
    }
}
