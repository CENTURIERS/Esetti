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
using Models.Users;
using Models.ClubBase;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Chrome;

namespace Esseti.ViewModels
{
    public class ActivityProfileItem
    {
        public int ActivityId { get; init; }
        public string Name { get; init; } = "";
        public string PersonInCharge { get; init; } = "";
        public string Date { get; init; } = "";
    }

    public class ProjectCardItem
    {
        public int ProjectId { get; init; }
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

        [ObservableProperty] private string _editFirstName = "";
        [ObservableProperty] private string _editLastName = "";
        [ObservableProperty] private string _editEmail = "";
        [ObservableProperty] private string _editPhoneNumber = "";
        [ObservableProperty] private string _editIndexNumber = "";
        [ObservableProperty] private string _editMajor = "";
        [ObservableProperty] private string _editDescription = "";
        [ObservableProperty] private string _editRole = "";
        [ObservableProperty] private string _editValidationError = "";

        [ObservableProperty] private bool _isEditFirstNameInvalid;
        [ObservableProperty] private bool _isEditLastNameInvalid;
        [ObservableProperty] private bool _isEditEmailInvalid;
        [ObservableProperty] private bool _isEditIndexNumberInvalid;
        [ObservableProperty] private bool _isEditPhoneNumberInvalid;
        [ObservableProperty] private bool _isEditMajorInvalid;
        [ObservableProperty] private bool _isEditFormValid = true;

        private void ValidateEditForm()
        {
            IsEditFirstNameInvalid = string.IsNullOrWhiteSpace(EditFirstName);
            IsEditLastNameInvalid = string.IsNullOrWhiteSpace(EditLastName);
            IsEditEmailInvalid = !string.IsNullOrWhiteSpace(EditEmail) && !IsValidEmail(EditEmail);
            IsEditIndexNumberInvalid = !string.IsNullOrWhiteSpace(EditIndexNumber) && !EditIndexNumber.All(char.IsDigit);
            IsEditPhoneNumberInvalid = !string.IsNullOrWhiteSpace(EditPhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(EditPhoneNumber, @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$");
            IsEditMajorInvalid = string.IsNullOrWhiteSpace(EditMajor);

            IsEditFormValid = !IsEditFirstNameInvalid && !IsEditLastNameInvalid && !IsEditEmailInvalid && !IsEditIndexNumberInvalid && !IsEditPhoneNumberInvalid && !IsEditMajorInvalid;
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(EditFirstName) ||
                e.PropertyName == nameof(EditLastName) ||
                e.PropertyName == nameof(EditEmail) ||
                e.PropertyName == nameof(EditIndexNumber) ||
                e.PropertyName == nameof(EditPhoneNumber) ||
                e.PropertyName == nameof(EditMajor))
            {
                ValidateEditForm();
            }
        }

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

        private readonly bool _openEditImmediately;

        public MemberProfileViewModel(int memberId, INavigationService navigationService, IMemberRepository memberRepository, bool openEditImmediately = false)
        {
            _memberId = memberId;
            _navigationService = navigationService;
            _memberRepository = memberRepository;
            _openEditImmediately = openEditImmediately;
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task ChangeAvatarAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                    Title = "Wybierz nowe zdjÄ™cie profilowe",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { FilePickerFileTypes.ImageAll } 
                });

                if (files.Count > 0)
                {
                    using (var stream = await files[0].OpenReadAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        byte[] avatarData = memoryStream.ToArray();

                        await _memberRepository.UpdateMemberAvatarAsync(_memberId, avatarData);

                        using var ms = new MemoryStream(avatarData);
                        Avatar = new Bitmap(ms);
                    }
                }
            }

        }

        private async Task LoadAsync()
        {
            try
            {
                var member = await _memberRepository.GetMemberByIdAsync(_memberId);
                if (member == null)
                {
                    Dispatcher.UIThread.Post(() => { IsLoading = false; Description = "Nie znaleziono uĹĽytkownika w bazie."; });
                    return;
                }

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
                    DepartmentName = dept?.Name ?? "Brak wydziaĹ‚u";
                    DepartmentAddress = dept != null
                        ? $"{dept.AddressLine}, {dept.PostalCode} {dept.City}".Trim(' ', ',')
                        : "";
                    CollegeNip = college?.NIP != null ? $"NIP: {college.NIP}" : "";

                    Activities.Clear();
                    foreach (var a in (member.Activities ?? new()).OrderByDescending(a => a.Date))
                    {
                        Activities.Add(new ActivityProfileItem
                        {
                            ActivityId = a.ActivityId,
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
                            ProjectId = p.ProjectId,
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
                    if (_openEditImmediately)
                    {
                        OpenEdit();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BĹ‚Ä…d Ĺ‚adowania profilu: {ex}");
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(App.Services.GetRequiredService<MembersViewModel>());
        }

        [RelayCommand]
        private void OpenEdit()
        {
            EditFirstName = FullName.Split(' ').FirstOrDefault() ?? "";
            EditLastName = FullName.Split(' ').LastOrDefault() ?? "";
            EditEmail = Email;
            EditPhoneNumber = PhoneNumber;
            EditIndexNumber = IndexNumber;
            EditMajor = Major;
            EditDescription = Description;
            EditRole = Role;

            ValidateEditForm();
            IsEditPopupVisible = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditPopupVisible = false;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            EditValidationError = string.Empty;

            if (string.IsNullOrWhiteSpace(EditFirstName))
            {
                EditValidationError = "ImiÄ™ jest wymagane.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditLastName))
            {
                EditValidationError = "Nazwisko jest wymagane.";
                return;
            }

            if (!IsValidEmail(EditEmail))
            {
                EditValidationError = "Niepoprawny format adresu e-mail.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(EditIndexNumber) && !EditIndexNumber.All(char.IsDigit))
            {
                EditValidationError = "Numer indeksu musi skĹ‚adaÄ‡ siÄ™ wyĹ‚Ä…cznie z cyfr.";
                return;
            }

            var updatedMember = new Models.Users.Member 
            {
                MemberId = _memberId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                PhoneNumber = EditPhoneNumber,
                IndexNumber = EditIndexNumber,
                Major = EditMajor,
                Description = EditDescription,
                Account = !string.IsNullOrWhiteSpace(EditEmail) ? new UserAccount { Email = EditEmail } : null,
                AuthorityRole = !string.IsNullOrWhiteSpace(EditRole) ? new AuthorityRole { Name = EditRole } : null
            };

            var remainingProjectIds = Projects.Select(p => p.ProjectId).ToList();
            var remainingActivityIds = Activities.Select(a => a.ActivityId).ToList();


            await _memberRepository.UpdateMemberAsync(updatedMember, remainingProjectIds, remainingActivityIds);

            // PrzeĹ‚aduj profil po zapisaniu
            await LoadAsync();

            IsEditPopupVisible = false;
        }

        [RelayCommand]
        private void RemoveFromProject(ProjectCardItem project)
        {
            if (project != null)
            {
                Projects.Remove(project);
                HasProjects = Projects.Any();
            }
        }

        [RelayCommand]
        private void RemoveFromActivity(ActivityProfileItem activity)
        {
            if (activity != null)
            {
                Activities.Remove(activity);
                HasActivities = Activities.Any();
            }
        }

        [RelayCommand]
        private void DeleteThisMember()
        {
            IsEditPopupVisible = false;
            IsPopupVisible = true;
        }

        protected override async Task ExecuteConfirmDeleteAsync()
        {
            await _memberRepository.DeleteSingleMemberAsync(_memberId);
            _navigationService.NavigateTo(App.Services.GetRequiredService<MembersViewModel>());
        }
    }
}

