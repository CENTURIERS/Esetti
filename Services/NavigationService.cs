using Esseti.ViewModels;
using System;

namespace Esseti.Services
{
    /// <summary>
    /// Klasa obsługująca przełączanie się między widokami w aplikacji (routing stron).
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        /// Zdarzenie (Callback) wywoływane w momencie zmiany widoku.
        /// Zazwyczaj podczepia się pod to MainWindowViewModel, żeby podmienić aktualny Content.
        /// </summary>
        public Action<ViewModelBase>? OnNavigate { get; set; }

        /// <summary>
        /// Przełącza aplikację na podany ViewModel, odpalając zdarzenie <see cref="OnNavigate"/>.
        /// </summary>
        /// <param name="viewModel">Nowy view model, na który przechodzimy.</param>
        public void NavigateTo(ViewModelBase viewModel)
        {
            OnNavigate?.Invoke(viewModel);
        }
    }
}

