using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esseti.Repositories.Interfaces;
using Esseti.ViewModels.Member;

namespace Esseti.ViewModels
{
    public partial class MembersViewModel : ViewModelBase
    {
        public override string PageTitle => "Lista członków";

        public ObservableCollection<MemberItemViewModel> Members { get; } = new();
        private readonly IMemberRepository _memberRepository;
        private readonly List<MemberItemViewModel> _allMembers = new();

        [ObservableProperty]
        private bool _isAllSelected;

        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var member in Members.Where(m => !m.IsSystemAddTile))
            {
                member.IsSelected = value;
            }
        }

        public MembersViewModel(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteSelectedAsync()
        {
            var toDelete = Members.Where(m => m.IsSelected && !m.IsSystemAddTile).ToList();
            if (!toDelete.Any()) return;

            foreach (var item in toDelete)
            {
                Members.Remove(item);
                _allMembers.Remove(item);
            }
            IsAllSelected = false;
        }

        [RelayCommand]
        private void OpenProfile(MemberItemViewModel member)
        {
            if (member == null) return;
            System.Diagnostics.Debug.WriteLine($"Otwieram profil: {member.FullName}");
        }

        [RelayCommand]
        private void EditMember(MemberItemViewModel member)
        {
            if (member == null) return;
            System.Diagnostics.Debug.WriteLine($"Edytuję członka: {member.FullName}");
        }

        [RelayCommand]
        private async Task DeleteMemberAsync(MemberItemViewModel member)
        {
            if (member == null) return;
            Members.Remove(member);
            _allMembers.Remove(member);
        }

        [RelayCommand]
        private void AddMember()
        {
            System.Diagnostics.Debug.WriteLine("Otwieram formularz dodawania nowego członka...");
        }

        protected override void OnSearchQueryUpdated(string value) => ApplyFilter();

        private async Task LoadDataAsync()
        {
            try
            {
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
                            _allMembers.Add(new MemberItemViewModel(
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
                            ));
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
        }
    }
}