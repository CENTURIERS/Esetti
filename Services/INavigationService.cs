using Esseti.ViewModels;

namespace Esseti.Services
{
    public interface INavigationService
    {
        void NavigateTo(ViewModelBase viewModel);
    }
}

