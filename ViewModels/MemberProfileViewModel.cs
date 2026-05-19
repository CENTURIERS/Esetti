using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Esseti.ViewModels
{
    public class ActivityProfileItem
    {
        public string Name { get; init; } = "";
        public string PersonInCharge { get; init; } = "";
        public string Date { get; init; } = "";
    }

    public class ProjectCardItem
    {
        public string Name { get; init; } = "";
        public string LeaderName { get; init; } = "";
        public int ParticipantCount { get; init; }
        public string SectionNames { get; init; } = "";
        public string Description { get; init; } = "";
    }

    public partial class MemberProfileViewModel : ViewModelBase
    {
        public override string PageTitle => "Profil członka";

        private readonly INavigationService _navigationService;
        private readonly IMemberRepository _memberRepository;
        private readonly int _memberId;

        [ObservableProperty] private Bitmap? _avatar;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _role = "";
        [ObservableProperty] private string _indexNumber = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _major = "";
        [ObservableProperty] private string _joinDate = "";
        [ObservableProperty] private string _description = "";
        [ObservableProperty] private string _collegeName = "";
        [ObservableProperty] private string _departmentName = "";
        [ObservableProperty] private string _departmentAddress = "";
        [ObservableProperty] private string _collegeNip = "";
        [ObservableProperty] private bool _isLoading = true;
        [ObservableProperty] private bool _hasProjects;
        [ObservableProperty] private bool _hasActivities;

        public ObservableCollection<ActivityProfileItem> Activities { get; } = new();
        public ObservableCollection<ProjectCardItem> Projects { get; } = new();

        private static Bitmap? _defaultAvatar;
        private static Bitmap? SafeDefaultAvatar
        {
            get
            {
                if (_defaultAvatar != null) return _defaultAvatar;
                try { _defaultAvatar = new Bitmap(AssetLoader.Open(new Uri("avares://Esseti/Assets/user-default.png"))); }
                catch { }
                return _defaultAvatar;
            }
        }

        public MemberProfileViewModel(int memberId, INavigationService navigationService, IMemberRepository memberRepository)
        {
            _memberId = memberId;
            _navigationService = navigationService;
            _memberRepository = memberRepository;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var member = await _memberRepository.GetMemberByIdAsync(_memberId);
                if (member == null) return;

                Dispatcher.UIThread.Post(() =>
                {
                    if (member.MemberAvatar?.Length > 0)
                    {
                        try { using var ms = new MemoryStream(member.MemberAvatar); Avatar = new Bitmap(ms); }
                        catch { Avatar = SafeDefaultAvatar; }
                    }
                    else Avatar = SafeDefaultAvatar;

                    FullName = $"{member.FirstName} {member.LastName}".Trim();
                    Role = member.AuthorityRole?.Name ?? "Brak roli";
                    IndexNumber = member.IndexNumber ?? "";
                    Email = member.Account?.Email ?? "";
                    PhoneNumber = member.PhoneNumber ?? "";
                    Major = member.Major ?? "";
                    JoinDate = $"Od {member.JoinDate:dd.MM.yyyy} r.";
                    Description = string.IsNullOrWhiteSpace(member.Description) ? "Brak opisu." : member.Description;

                    var club = member.MemberClubs?.FirstOrDefault()?.Club;
                    var dept = club?.Department;
                    var college = dept?.College;

                    CollegeName = college?.Name ?? "Brak uczelni";
                    DepartmentName = dept?.Name ?? "Brak wydziału";
                    DepartmentAddress = dept != null
                        ? $"{dept.AddressLine}, {dept.PostalCode} {dept.City}".Trim(' ', ',')
                        : "";
                    CollegeNip = college?.NIP != null ? $"NIP: {college.NIP}" : "";

                    Activities.Clear();
                    foreach (var a in (member.Activities ?? new()).OrderByDescending(a => a.Date))
                    {
                        Activities.Add(new ActivityProfileItem
                        {
                            Name = a.Name,
                            PersonInCharge = a.PersonInChargeName ?? "Uczestnik",
                            Date = a.Date.ToString("dd.MM.yy") + " r."
                        });
                    }
                    HasActivities = Activities.Any();

                    Projects.Clear();
                    foreach (var p in member.Projects ?? new())
                    {
                        Projects.Add(new ProjectCardItem
                        {
                            Name = p.Name,
                            LeaderName = p.PersonInCharge != null
                                ? $"{p.PersonInCharge.FirstName} {p.PersonInCharge.LastName}".Trim()
                                : "Brak lidera",
                            ParticipantCount = p.Participants?.Count ?? 0,
                            SectionNames = p.Sections?.Any() == true
                                ? string.Join(", ", p.Sections.Select(s => s.Name))
                                : "Brak sekcji",
                            Description = p.Description ?? ""
                        });
                    }
                    HasProjects = Projects.Any();
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania profilu: {ex}");
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<MembersViewModel>());
        }
    }
}