using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

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

        [ObservableProperty]
        private bool _isPopupVisible;

        [ObservableProperty]
        private bool _isAddPopupVisible;

        partial void OnIsPopupVisibleChanged(bool value)
        {
            if (!value) OnPopupClosed();
        }

        protected virtual void OnPopupClosed() { }

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

        [RelayCommand]
        protected void RequestDelete()
        {
            if (IsAnySelected)
            {
                IsPopupVisible = true;
            }
        }

        [RelayCommand]
        protected void CancelDelete()
        {
            IsPopupVisible = false;
        }


        [RelayCommand]
        private async Task ConfirmDeleteAsync()
        {
            IsPopupVisible = false;

            await ExecuteConfirmDeleteAsync();
        }

        protected virtual async Task ExecuteConfirmDeleteAsync()
        {
            await Task.CompletedTask; 
        }
    }
}
