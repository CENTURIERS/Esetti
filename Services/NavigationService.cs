using Esseti.ViewModels;
using System;

namespace Esseti.Services
{
    public class NavigationService : INavigationService
    {
        public Action<ViewModelBase>? OnNavigate { get; set; }

        public void NavigateTo(ViewModelBase viewModel)
        {
            OnNavigate?.Invoke(viewModel);
        }
    }
}

