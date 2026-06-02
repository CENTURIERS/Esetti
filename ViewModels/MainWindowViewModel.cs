using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.ViewModels.Components;
using Esseti.ViewModels;
using Esseti.Views;
using Esseti.Repositories;
using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Esseti.Services;

namespace Esseti.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public override string PageTitle => "";

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

        private bool _isViewChanged = false;

        public ObservableCollection<NavigationItemViewModel> MenuItems { get; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (navigationService is NavigationService nav)
                nav.OnNavigate = vm =>
                {
                    CurrentViewModel = vm;
                    if (vm is MemberProfileViewModel) SetActiveMenuItem(2);
                    if (!_isViewChanged) ChangeView();
                };

            CurrentViewModel = new StartViewModel();
            LoadLogo(HeaderLogoPath);

            MenuItems = new ObservableCollection<NavigationItemViewModel> { 
                new NavigationItemViewModel("M6 2a.5.5 0 0 1 .47.33L10 12.036l1.53-4.208A.5.5 0 0 1 12 7.5h3.5a.5.5 0 0 1 0 1h-3.15l-1.88 5.17a.5.5 0 0 1-.94 0L6 3.964 4.47 8.171A.5.5 0 0 1 4 8.5H.5a.5.5 0 0 1 0-1h3.15l1.88-5.17A.5.5 0 0 1 6 2", 
                "Aktywności", 
                ShowAllActivitiesCommand),

                new NavigationItemViewModel("m7.646 9.354-3.792 3.792a.5.5 0 0 0 .353.854h7.586a.5.5 0 0 0 .354-.854L8.354 9.354a.5.5 0 0 0-.708 0 M11.414 11H14.5a.5.5 0 0 0 .5-.5v-7a.5.5 0 0 0-.5-.5h-13a.5.5 0 0 0-.5.5v7a.5.5 0 0 0 .5.5h3.086l-1 1H1.5A1.5 1.5 0 0 1 0 10.5v-7A1.5 1.5 0 0 1 1.5 2h13A1.5 1.5 0 0 1 16 3.5v7a1.5 1.5 0 0 1-1.5 1.5h-2.086z",
                "Projekty",
                ShowAllProjectsCommand),

                new NavigationItemViewModel("M7 14s-1 0-1-1 1-4 5-4 5 3 5 4-1 1-1 1zm4-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6m-5.784 6A2.24 2.24 0 0 1 5 13c0-1.355.68-2.75 1.936-3.72A6.3 6.3 0 0 0 5 9c-4 0-5 3-5 4s1 1 1 1zM4.5 8a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5",
                "Członkowie",
                ShowAllMembersCommand),

                new NavigationItemViewModel("M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16 M8.93 6.588 L6.64 6.875 L6.558 7.255 L7.008 7.338 C7.302 7.408 7.36 7.514 7.296 7.807 L6.558 11.275 C6.364 12.172 6.663 12.594 7.366 12.594 C7.911 12.594 8.544 12.342 8.831 11.996 L8.919 11.58 C8.719 11.756 8.427 11.826 8.233 11.826 C7.958 11.826 7.858 11.633 7.929 11.293 Z M9 4.5 a1 1 0 1 1-2 0 1 1 0 0 1 2 0",
                "Informacje",
                ShowClubInfoCommand),

                new NavigationItemViewModel("M4 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2zm0 1h8a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1 M4.603 12.087a.8.8 0 0 1-.438-.42c-.195-.388-.13-.776.08-1.102.198-.307.526-.568.897-.787a7.7 7.7 0 0 1 1.482-.645 20 20 0 0 0 1.062-2.227 7.3 7.3 0 0 1-.43-1.295c-.086-.4-.119-.796-.046-1.136.075-.354.274-.672.65-.823.192-.077.4-.12.602-.077a.7.7 0 0 1 .477.365c.088.164.12.356.127.538.007.187-.012.395-.047.614-.084.51-.27 1.134-.52 1.794a11 11 0 0 0 .98 1.686 5.8 5.8 0 0 1 1.334.05c.364.065.734.195.96.465.12.144.193.32.2.518.007.192-.047.382-.138.563a1.04 1.04 0 0 1-.354.416.86.86 0 0 1-.51.138c-.331-.014-.654-.196-.933-.417a5.7 5.7 0 0 1-.911-.95 11.6 11.6 0 0 0-1.997.406 11.3 11.3 0 0 1-1.021 1.51c-.29.35-.608.655-.926.787a.8.8 0 0 1-.58.029m1.379-1.901q-.25.115-.459.238c-.328.194-.541.383-.647.547-.094.145-.096.25-.04.361q.016.032.026.044l.035-.012c.137-.056.355-.235.635-.572a8 8 0 0 0 .45-.606m1.64-1.33a13 13 0 0 1 1.01-.193 12 12 0 0 1-.51-.858 21 21 0 0 1-.5 1.05zm2.446.45q.226.244.435.41c.24.19.407.253.498.256a.1.1 0 0 0 .07-.015.3.3 0 0 0 .094-.125.44.44 0 0 0 .059-.2.1.1 0 0 0-.026-.063c-.052-.062-.2-.152-.518-.209a4 4 0 0 0-.612-.053zM8.078 5.8a7 7 0 0 0 .2-.828q.046-.282.038-.465a.6.6 0 0 0-.032-.198.5.5 0 0 0-.145.04c-.087.035-.158.106-.196.283-.04.192-.03.469.046.822q.036.167.09.346z",
                "Dokumenty",
                ShowDocumentsPageCommand),

            };
        }

        [ObservableProperty]
        private bool _isSettingsActive;

        private void SetActiveMenuItem(int index)
        {
            IsSettingsActive = (index == -1);
            for (int i = 0; i < MenuItems.Count; i++)
                MenuItems[i].IsActive = (i == index);
        }

        [RelayCommand]
        public void ShowAllActivities()
        {
            CurrentViewModel = App.Services.GetRequiredService<ActivitiesViewModel>();
            SetActiveMenuItem(0);

            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        [RelayCommand]
        public void ShowAllProjects()
        {
            CurrentViewModel = App.Services.GetRequiredService<ProjectsViewModel>();
            SetActiveMenuItem(1);
            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        [RelayCommand]
        public void ShowAllMembers()
        {
            CurrentViewModel = App.Services.GetRequiredService<MembersViewModel>();
            SetActiveMenuItem(2);
            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        [RelayCommand]
        public void ShowClubInfo()
        {
            CurrentViewModel = App.Services.GetRequiredService<ClubInfoViewModel>();
            SetActiveMenuItem(3);
            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        [RelayCommand]
        public void ShowDocumentsPage()
        {
            CurrentViewModel = App.Services.GetRequiredService<DocumentsViewModel>();
            SetActiveMenuItem(4);
            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        [RelayCommand]
        public void ShowSettingsPage()
        {
            CurrentViewModel = App.Services.GetRequiredService<SettingsViewModel>();
            SetActiveMenuItem(-1);

            if (!_isViewChanged)
            {
                ChangeView();
            }
        }

        private void ChangeView()
        {
            _isViewChanged = true;

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
                var oldLogo = HeaderLogo;
                HeaderLogo = new Bitmap(AssetLoader.Open(uri));
                oldLogo?.Dispose();
            } catch (Exception ex)
            {
                Console.WriteLine($"Nie udało się załadować logo: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            HeaderLogo?.Dispose();
            HeaderLogo = null;
            base.Dispose();
        }
    }
}


