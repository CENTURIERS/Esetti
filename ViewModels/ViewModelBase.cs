using CommunityToolkit.Mvvm.ComponentModel;

namespace Esseti.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        public virtual string PageTitle => "Aplikacja Esseti";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        protected bool _isUpdatingSelection;

        [ObservableProperty]
        private bool _isAllSelected;

        [ObservableProperty]
        private bool _isAnySelected;

        [ObservableProperty]
        private int _selectedCount;

        partial void OnIsAllSelectedChanged(bool value)
        {
            OnIsAllSelectedChangedVirtual(value);
        }

        protected virtual void OnIsAllSelectedChangedVirtual(bool value)
        {
        }

        partial void OnSearchQueryChanged(string value)
        {
            OnSearchQueryUpdated(value);
        }

        protected virtual void OnSearchQueryUpdated(string value)
        {

        }
    }
}
