using Esseti.ViewModels;

namespace Esseti.Services
{
    /// <summary>
    /// Interfejs serwisu nawigacji. Odpowiada za routing stron i przełączanie widoków (ViewModeli).
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Zmienia aktualny widok na inny ViewModel.
        /// </summary>
        /// <param name="viewModel">ViewModel docelowy, na który chcemy się przełączyć.</param>
        void NavigateTo(ViewModelBase viewModel);
    }
}

