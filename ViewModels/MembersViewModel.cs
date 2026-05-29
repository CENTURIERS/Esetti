using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Esseti.ViewModels.Member;
using Microsoft.Extensions.DependencyInjection;
using Models.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Models.ClubBase;

namespace Esseti.ViewModels
{
    public partial class MembersViewModel : ViewModelBase
    {
        public override string PageTitle => "Lista członków";

        public override bool ShowActionHeader => true;

        public override string SearchPlaceholder => "Szukaj członków...";

        public ObservableCollection<MemberItemViewModel> Members { get; } = new();
        private readonly IMemberRepository _memberRepository;
        private readonly List<MemberItemViewModel> _allMembers = new();
        private MemberItemViewModel? _memberToDelete;

        [ObservableProperty] 
        private string _newFirstName = string.Empty;

        [ObservableProperty] 
        private string _newLastName = string.Empty;

        [ObservableProperty] 
        private string _newEmail = string.Empty;

        [ObservableProperty] 
        private string _newIndexNumber = string.Empty;

        [ObservableProperty]
        private byte[]? _newAvatar;

        public ObservableCollection<AuthorityRole> AvailableRoles { get; } = new();

        public ObservableCollection<Models.University.CollegeDepartment> AvailableDepartments { get; } = new();

        [ObservableProperty]
        private Models.University.CollegeDepartment? _selectedDepartment;

        [ObservableProperty]
        private Models.University.CollegeDepartment? _editSelectedDepartment;

        [ObservableProperty]
        private AuthorityRole? _selectedRole;

        [ObservableProperty]
        private bool _isListEditPopupVisible;

        [ObservableProperty]
        private string _editFirstName = string.Empty;

        [ObservableProperty]
        private string _editLastName = string.Empty;

        [ObservableProperty]
        private string _editEmail = string.Empty;

        [ObservableProperty]
        private string _editIndexNumber = string.Empty;

        [ObservableProperty]
        private byte[]? _editAvatar;

        [ObservableProperty]
        private AuthorityRole? _editSelectedRole;

        private MemberItemViewModel? _memberBeingEdited;

        private readonly INavigationService _navigationService;

        protected override void OnPopupClosed()
        {
            
        }

        protected override void OnIsAllSelectedChangedVirtual(bool value)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;

            try
            {
                foreach (var member in Members.Where(m => !m.IsSystemAddTile)) {
                    member.IsSelected = value;
                }
                UpdateSelectionState();
            } finally
            {
                _isUpdatingSelection = false;
            }
        }

        public MembersViewModel(IMemberRepository memberRepository, INavigationService navigationService)
        {
            _memberRepository = memberRepository;
            _navigationService = navigationService;
            _ = LoadDataAsync();
        }

        private void UpdateSelectionState()
        {
            var selected = Members.Where(m => !m.IsSystemAddTile && m.IsSelected).ToList();
            SelectedCount = selected.Count;
            IsAnySelected = SelectedCount > 0;
        }

        protected override async Task ExecuteConfirmDeleteAsync()
        {
            try
            {
                if (_memberToDelete != null)
                {
                    await _memberRepository.DeleteSingleMemberAsync(_memberToDelete.MemberId);

                    Members.Remove(_memberToDelete);
                    _allMembers.Remove(_memberToDelete);

                    _memberToDelete = null;
                }
                else
                {
                    var selectedVMs = Members.Where(m => m.IsSelected && !m.IsSystemAddTile).ToList();
                    if (!selectedVMs.Any()) return;

                    var idsToDelete = selectedVMs.Select(m => m.MemberId).ToList();
                    await _memberRepository.DeleteMembersAsync(idsToDelete);

                    foreach (var vm in selectedVMs)
                    {
                        Members.Remove(vm);
                        _allMembers.Remove(vm);
                    }
                }

                IsAllSelected = false;
                UpdateSelectionState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd bazy: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenProfile(MemberItemViewModel member)
        {
            if (member == null || member.IsSystemAddTile) return;
            var profileVm = new MemberProfileViewModel(
                member.MemberId,
                _navigationService,
                App.Services.GetRequiredService<IMemberRepository>()
            );
            _navigationService.NavigateTo(profileVm);
        }


        [RelayCommand]
        private void DeleteSingleMember(MemberItemViewModel member)
        {
            if (member == null) return;

            _memberToDelete = member;
            IsPopupVisible = true;
        }

        [RelayCommand]
        private void EditMember(MemberItemViewModel member)
        {
            if (member == null || member.IsSystemAddTile) return;

            _memberBeingEdited = member;
            EditFirstName = member.FirstName;
            EditLastName = member.LastName;
            EditEmail = member.Email;
            EditIndexNumber = member.IndexNumber;
            EditAvatar = null;
            EditSelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == member.Role) ?? AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            EditSelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Name == member.CollegeDepartment);

            IsListEditPopupVisible = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsListEditPopupVisible = false;
        }

        [RelayCommand]
        private async Task OpenEditAvatarPickerAsync()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Wybierz zdjęcie",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                    });

                    if (files.Count >= 1)
                    {
                        using var stream = await files[0].OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        EditAvatar = ms.ToArray();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task ConfirmEditAsync()
        {
            if (_memberBeingEdited == null || string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName))
                return;

            int clubId = 1;

            var editMemberDb = new Models.Users.Member
            {
                MemberId = _memberBeingEdited.MemberId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                IndexNumber = EditIndexNumber,
                MemberAvatar = EditAvatar,
                AuthorityRole = EditSelectedRole,
                RoleId = EditSelectedRole?.RoleId ?? 0,
                Account = string.IsNullOrWhiteSpace(EditEmail) ? null : new UserAccount
                {
                    Email = EditEmail,
                    SystemRole = Models.Enums.SystemRole.User
                },
                MemberClubs = new List<MemberClub>
                {
                    new MemberClub { ClubId = clubId, MemberId = _memberBeingEdited.MemberId }
                }
            };

            await _memberRepository.UpdateMemberBasicInfoAsync(editMemberDb, EditSelectedDepartment?.CollegeDepartmentId);

            await LoadDataAsync();

            IsListEditPopupVisible = false;
        }

        [RelayCommand]
        private void AddMember()
        {
            NewFirstName = string.Empty;
            NewLastName = string.Empty;
            NewEmail = string.Empty;
            NewIndexNumber = string.Empty;
            NewAvatar = null;
            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            SelectedDepartment = AvailableDepartments.FirstOrDefault();

            IsAddPopupVisible = true;
        }

        [RelayCommand]
        private void CancelAdd()
        {
            IsAddPopupVisible = false;
        }

        [RelayCommand]
        private async Task OpenAvatarPickerAsync()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.MainWindow;
                if (window != null)
                {
                    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Wybierz zdjęcie",
                        AllowMultiple = false,
                        FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
                    });

                    if (files.Count >= 1)
                    {
                        using var stream = await files[0].OpenReadAsync();
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        NewAvatar = ms.ToArray();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task ConfirmAddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFirstName) || string.IsNullOrWhiteSpace(NewLastName))
                return;

            var newMemberDb = new Models.Users.Member
            {
                FirstName = NewFirstName,
                LastName = NewLastName,
                IndexNumber = NewIndexNumber,
                MemberAvatar = NewAvatar,
                IsActive = true,
                JoinDate = System.DateTime.Now,
                AuthorityRole = SelectedRole,
                RoleId = SelectedRole?.RoleId ?? 0,
                Account = string.IsNullOrWhiteSpace(NewEmail)? null: new UserAccount
                            {
                                Email = NewEmail,
                                SystemRole = Models.Enums.SystemRole.User
                            }
            };
            int clubId = 1;

            newMemberDb.MemberClubs = new List<MemberClub>
            {
                new MemberClub { ClubId = clubId }
            };

            await _memberRepository.AddMemberAsync(newMemberDb, SelectedDepartment?.CollegeDepartmentId);

            await LoadDataAsync();

            IsAddPopupVisible = false;
        }

        protected override void OnSearchQueryUpdated(string value) => ApplyFilter();

        private async Task LoadDataAsync()
        {
            try
            {
                var roles = await _memberRepository.GetAuthorityRolesAsync();
                var departments = await _memberRepository.GetCollegeDepartmentsAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    AvailableRoles.Clear();
                    foreach (var r in roles) AvailableRoles.Add(r);

                    AvailableDepartments.Clear();
                    foreach (var d in departments) AvailableDepartments.Add(d);
                });

                var membersFromDb = await _memberRepository.GetAllMembersAsync();
                Dispatcher.UIThread.Post(() =>
                {
                    _allMembers.Clear();
                    _allMembers.Add(new MemberItemViewModel(0, Array.Empty<byte>(), "", "", "", "", "", "", "", "", true, true));

                    if (membersFromDb != null)
                    {
                        foreach (var m in membersFromDb)
                        {
                            var dept = m.MemberClubs?.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";
                            var vm = new MemberItemViewModel(
                                memberId: m.MemberId,
                                avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                                firstName: m.FirstName ?? string.Empty,
                                lastName: m.LastName ?? string.Empty,
                                role: m.AuthorityRole?.Name ?? "Brak roli",
                                indexNumber: m.IndexNumber ?? string.Empty,
                                email: m.Account?.Email ?? string.Empty,
                                collegeDepartment: dept,
                                major: m.Major ?? string.Empty,
                                joinDate: m.JoinDate.ToString("dd.MM.yyyy"),
                                isActive: m.IsActive,
                                isSystemAddTile: false
                            );
                            vm.PropertyChanged += (s, e) =>
                            {
                                if (e.PropertyName == nameof(MemberItemViewModel.IsSelected))
                                {
                                    UpdateSelectionState();

                                    if (!_isUpdatingSelection)
                                    {
                                        _isUpdatingSelection = true;
                                        try
                                        {
                                            var selectableMembers = Members.Where(m => !m.IsSystemAddTile).ToList();
                                            if (selectableMembers.Any())
                                            {
                                                IsAllSelected = selectableMembers.All(m => m.IsSelected);
                                            }
                                        }
                                        finally
                                        {
                                            _isUpdatingSelection = false;
                                        }
                                    }
                                }
                            };
                            _allMembers.Add(vm);
                        }
                    }
                    ApplyFilter();
                });
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void ApplyFilter()
        {
            Members.Clear();
            var query = SearchQuery?.ToLower() ?? "";
            foreach (var item in _allMembers)
            {
                if (item.IsSystemAddTile || item.FullName.ToLower().Contains(query) || item.Role.ToLower().Contains(query))
                    Members.Add(item);
            }
            UpdateSelectionState();
        }
    }
}