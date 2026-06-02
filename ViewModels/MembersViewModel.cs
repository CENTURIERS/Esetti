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
        private string _newPhoneNumber = string.Empty;

        [ObservableProperty] 
        private string _newIndexNumber = string.Empty;

        [ObservableProperty] 
        private byte[]? _newAvatar;

        [ObservableProperty]
        private string _newDescription = string.Empty;

        [ObservableProperty]
        private string _newMajor = string.Empty;

        public ObservableCollection<AuthorityRole> AvailableRoles { get; } = new();

        public ObservableCollection<Models.University.CollegeDepartment> AvailableDepartments { get; } = new();

        [ObservableProperty]
        private Models.University.CollegeDepartment? _selectedDepartment;

        [ObservableProperty]
        private Models.University.CollegeDepartment? _editSelectedDepartment;

        [ObservableProperty]
        private AuthorityRole? _selectedRole;

        [ObservableProperty] private bool _isAddFirstNameInvalid;
        [ObservableProperty] private bool _isAddLastNameInvalid;
        [ObservableProperty] private bool _isAddEmailInvalid;
        [ObservableProperty] private bool _isAddIndexNumberInvalid;
        [ObservableProperty] private bool _isAddFormValid = true;

        [ObservableProperty] private bool _isEditFirstNameInvalid;
        [ObservableProperty] private bool _isEditLastNameInvalid;
        [ObservableProperty] private bool _isEditEmailInvalid;
        [ObservableProperty] private bool _isEditIndexNumberInvalid;
        [ObservableProperty] private bool _isEditFormValid = true;

        private void ValidateAddForm()
        {
            IsAddFirstNameInvalid = string.IsNullOrWhiteSpace(NewFirstName);
            IsAddLastNameInvalid = string.IsNullOrWhiteSpace(NewLastName);
            IsAddEmailInvalid = string.IsNullOrWhiteSpace(NewEmail) || !IsValidEmail(NewEmail);
            IsAddIndexNumberInvalid = string.IsNullOrWhiteSpace(NewIndexNumber) || !NewIndexNumber.All(char.IsDigit) || NewIndexNumber.Length < 4;

            IsAddFormValid = !IsAddFirstNameInvalid && !IsAddLastNameInvalid && !IsAddEmailInvalid && !IsAddIndexNumberInvalid;
        }

        private void ValidateEditForm()
        {
            IsEditFirstNameInvalid = string.IsNullOrWhiteSpace(EditFirstName);
            IsEditLastNameInvalid = string.IsNullOrWhiteSpace(EditLastName);
            IsEditEmailInvalid = string.IsNullOrWhiteSpace(EditEmail) || !IsValidEmail(EditEmail);
            IsEditIndexNumberInvalid = string.IsNullOrWhiteSpace(EditIndexNumber) || !EditIndexNumber.All(char.IsDigit) || EditIndexNumber.Length < 4;

            IsEditFormValid = !IsEditFirstNameInvalid && !IsEditLastNameInvalid && !IsEditEmailInvalid && !IsEditIndexNumberInvalid;
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(NewFirstName) ||
                e.PropertyName == nameof(NewLastName) ||
                e.PropertyName == nameof(NewEmail) ||
                e.PropertyName == nameof(NewIndexNumber))
            {
                ValidateAddForm();
            }
            if (e.PropertyName == nameof(EditFirstName) ||
                e.PropertyName == nameof(EditLastName) ||
                e.PropertyName == nameof(EditEmail) ||
                e.PropertyName == nameof(EditIndexNumber))
            {
                ValidateEditForm();
            }
        }

        [ObservableProperty]
        private bool _isListEditPopupVisible;

        [ObservableProperty]
        private string _editFirstName = string.Empty;

        [ObservableProperty]
        private string _editLastName = string.Empty;

        [ObservableProperty]
        private string _editEmail = string.Empty;

        [ObservableProperty]
        private string _editPhoneNumber = string.Empty;

        [ObservableProperty]
        private string _editIndexNumber = string.Empty;

        [ObservableProperty]
        private byte[]? _editAvatar;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private string _editMajor = string.Empty;

        [ObservableProperty]
        private AuthorityRole? _editSelectedRole;

        [ObservableProperty]
        private string _addValidationError = string.Empty;

        [ObservableProperty]
        private string _editValidationError = string.Empty;

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
            EditPhoneNumber = member.PhoneNumber;
            EditIndexNumber = member.IndexNumber;
            EditDescription = member.Description;
            EditMajor = member.Major;
            EditAvatar = null;
            EditSelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == member.Role) ?? AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            EditSelectedDepartment = AvailableDepartments.FirstOrDefault(d => d.Name == member.CollegeDepartment);

            ValidateEditForm();
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
            EditValidationError = string.Empty;

            if (_memberBeingEdited == null)
                return;

            if (string.IsNullOrWhiteSpace(EditFirstName))
            {
                EditValidationError = "Imię jest wymagane.";
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
                EditValidationError = "Numer indeksu musi składać się wyłącznie z cyfr.";
                return;
            }

            int clubId = 1;

            var editMemberDb = new Models.Users.Member
            {
                MemberId = _memberBeingEdited.MemberId,
                FirstName = EditFirstName,
                LastName = EditLastName,
                IndexNumber = EditIndexNumber,
                Major = EditMajor,
                Description = EditDescription,
                MemberAvatar = EditAvatar,
                PhoneNumber = EditPhoneNumber,
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
            NewPhoneNumber = string.Empty;
            NewIndexNumber = string.Empty;
            NewDescription = string.Empty;
            NewMajor = string.Empty;
            NewAvatar = null;
            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Członek");
            SelectedDepartment = AvailableDepartments.FirstOrDefault();

            IsAddFirstNameInvalid = false;
            IsAddLastNameInvalid = false;
            IsAddEmailInvalid = false;
            IsAddIndexNumberInvalid = false;
            IsAddFormValid = false; // Empty fields mean form is invalid initially

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

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // email is optional
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        [RelayCommand]
        private async Task ConfirmAddAsync()
        {
            AddValidationError = string.Empty;

            if (string.IsNullOrWhiteSpace(NewFirstName))
            {
                AddValidationError = "Imię jest wymagane.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewLastName))
            {
                AddValidationError = "Nazwisko jest wymagane.";
                return;
            }

            if (!IsValidEmail(NewEmail))
            {
                AddValidationError = "Niepoprawny format adresu e-mail.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(NewIndexNumber) && !NewIndexNumber.All(char.IsDigit))
            {
                AddValidationError = "Numer indeksu musi składać się wyłącznie z cyfr.";
                return;
            }

            var newMemberDb = new Models.Users.Member
            {
                FirstName = NewFirstName,
                LastName = NewLastName,
                IndexNumber = NewIndexNumber,
                Major = NewMajor,
                Description = NewDescription,
                MemberAvatar = NewAvatar,
                PhoneNumber = NewPhoneNumber,
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

        protected override void OnSearchQueryUpdated(string value)
        {
            CurrentPage = 1;
            ApplyFilter();
        }

        private void OnMemberItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        }

        private void ClearMemberSubscriptions()
        {
            foreach (var vm in _allMembers)
            {
                vm.PropertyChanged -= OnMemberItemPropertyChanged;
            }
        }

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
                    ClearMemberSubscriptions();
                    _allMembers.Clear();
                    _allMembers.Add(new MemberItemViewModel(0, Array.Empty<byte>(), "", "", "", "", "", "", "", "", "", true, "", true));

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
                                phoneNumber: m.PhoneNumber ?? string.Empty,
                                collegeDepartment: dept,
                                major: m.Major ?? string.Empty,
                                joinDate: m.JoinDate.ToString("dd.MM.yyyy"),
                                isActive: m.IsActive,
                                description: m.Description ?? string.Empty,
                                isSystemAddTile: false
                            );
                            vm.PropertyChanged += OnMemberItemPropertyChanged;
                            _allMembers.Add(vm);
                        }
                    }
                    ApplyFilter();
                });
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        private int _totalPages = 1;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        [RelayCommand]
        private void NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                ApplyFilter();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            Members.Clear();
            var query = SearchQuery?.ToLower() ?? "";
            
            var filteredActualMembers = _allMembers
                .Where(item => !item.IsSystemAddTile)
                .Where(item => 
                    item.FullName.ToLower().Contains(query) || 
                    item.Role.ToLower().Contains(query) ||
                    item.IndexNumber.ToLower().Contains(query) ||
                    item.Email.ToLower().Contains(query) ||
                    item.CollegeDepartment.ToLower().Contains(query) ||
                    item.Major.ToLower().Contains(query))
                .ToList();

            int pageSize = 9;
            
            if (CurrentPage == 1)
            {
                int totalCount = filteredActualMembers.Count;
                TotalPages = (int)Math.Ceiling((double)(totalCount - 8) / pageSize) + 1;
                if (TotalPages < 1) TotalPages = 1;

                var addTile = _allMembers.FirstOrDefault(item => item.IsSystemAddTile);
                if (addTile != null)
                {
                    Members.Add(addTile);
                }

                var pageMembers = filteredActualMembers.Take(8);
                foreach (var item in pageMembers)
                {
                    Members.Add(item);
                }
            }
            else
            {
                int totalCount = filteredActualMembers.Count;
                TotalPages = (int)Math.Ceiling((double)(totalCount - 8) / pageSize) + 1;
                if (TotalPages < 1) TotalPages = 1;

                int skip = 8 + (CurrentPage - 2) * pageSize;
                var pageMembers = filteredActualMembers.Skip(skip).Take(pageSize);
                foreach (var item in pageMembers)
                {
                    Members.Add(item);
                }
            }
            
            UpdateSelectionState();
        }
    }
}

