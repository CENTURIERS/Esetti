using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Views;
using System;

namespace Esseti.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentViewModel;

        [ObservableProperty]
        private string _sidebarColor = "#FFFFFF";

        [ObservableProperty]
        private string _mainPanelColor = "#0033A0";

        [ObservableProperty]
        private string _sidebarButtonTextColor = "#0033A0";

        [ObservableProperty]
        private string _sidebarButtonBackgroundColor = "#FFFFFF";

        [ObservableProperty]
        private string _sidebarButtonHoverBackgroundColor = "#0033A0";

        [ObservableProperty]
        private string _sidebarButtonHoverTextColor = "#FFFFFF";

        [ObservableProperty]
        private bool _isHeaderVisible = false;

        [ObservableProperty]
        private Bitmap? _headerLogo;

        [ObservableProperty]
        private string _headerLogoPath = "urNiebieskie.png";

        public MainWindowViewModel()
        {
            CurrentViewModel = new StartViewModel();

            LoadLogo(HeaderLogoPath);
        }

        [RelayCommand]
        public void ShowAllActivities()
        {
            CurrentViewModel = new ActivitiesViewModel();
            ChangeView();
        }

        [RelayCommand]
        public void ShowAllProjects()
        {
            CurrentViewModel = new ProjectsViewModel();
            ChangeView();
        }

        [RelayCommand]
        public void ShowAllMembers()
        {
            CurrentViewModel = new MemberViewModel();
            ChangeView();
        }

        [RelayCommand]
        public void ShowClubInfo()
        {
            CurrentViewModel = new ClubInfoViewModel();
            ChangeView();
        }

        [RelayCommand]
        public void ShowDocumentsPage()
        {
            CurrentViewModel = new DocumentsViewModel();
            ChangeView();
        }

        [RelayCommand]
        public void ShowSettingsPage()
        {
            CurrentViewModel = new SettingsViewModel();
            ChangeView();
        }

        private void ChangeView()
        {
            SidebarColor = "#0033A0";
            MainPanelColor = "#FFFFFF";

            SidebarButtonTextColor = "#FFFFFF";
            SidebarButtonBackgroundColor = "#0033A0";

            SidebarButtonHoverTextColor = "#0033A0";
            SidebarButtonHoverBackgroundColor = "#FFFFFF";

            HeaderLogoPath = "urBiale.png";

            LoadLogo(HeaderLogoPath);

            IsHeaderVisible = true;
        }



        private void LoadLogo(string path)
        {
            try
            {
                var uri = new Uri("avares://Esseti/Assets/UR/" + path);

                HeaderLogo = new Bitmap(AssetLoader.Open(uri));
            } catch (Exception ex)
            {
                Console.WriteLine($"Nie udało się załadować logo: {ex.Message}");
            }
        }
    }
}
