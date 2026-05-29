using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Esseti.ViewModels
{
    public partial class ViewModelBase : ObservableObject, IDisposable
    {
        public virtual void Dispose()
        {
        }

        public virtual string PageTitle => "Aplikacja Esseti";

        public virtual bool ShowActionHeader => false;

        public virtual string SearchPlaceholder => "Szukaj...";

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

        [ObservableProperty]
        private bool _isEditPopupVisible;

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
            await ExecuteConfirmDeleteAsync();
            IsPopupVisible = false;
        }

        protected virtual async Task ExecuteConfirmDeleteAsync()
        {
            await Task.CompletedTask; 
        }

        protected bool TryParseDate(string dateStr, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
            {
                date = default;
                return false;
            }
            return DateTime.TryParseExact(dateStr.Trim(), 
                new[] { "dd.MM.yyyy", "yyyy-MM-dd", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy" }, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out date) 
                || DateTime.TryParse(dateStr.Trim(), out date);
        }
    }
}


