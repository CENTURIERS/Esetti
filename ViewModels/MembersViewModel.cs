using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
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

        public MembersViewModel(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
            _ = LoadDataAsync();
        }

        protected override void OnSearchQueryUpdated(string value)
        {
            ApplyFilter();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var membersFromDb = await _memberRepository.GetAllMembersAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        _allMembers.Clear();

                        _allMembers.Add(new MemberItemViewModel(
                            avatar: Array.Empty<byte>(),
                            firstName: string.Empty,
                            lastName: string.Empty,
                            role: string.Empty,
                            indexNumber: string.Empty,
                            email: string.Empty,
                            collegeDepartment: string.Empty,
                            major: string.Empty,
                            joinDate: string.Empty,
                            isActive: true,
                            isSystemAddTile: true
                        ));

                        if (membersFromDb != null)
                        {
                            foreach (var m in membersFromDb)
                            {
                                var departmentName = m.MemberClubs.FirstOrDefault()?.Club?.Department?.Name ?? "Brak wydziału";

                                var itemVm = new MemberItemViewModel(
                                    avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                                    firstName: m.FirstName ?? "Brak",
                                    lastName: m.LastName ?? "imienia",
                                    role: m.AuthorityRole?.Name ?? "Brak roli",
                                    indexNumber: m.IndexNumber ?? "00000000",
                                    email: m.Account?.Email ?? "Brak@email.pl",
                                    collegeDepartment: departmentName,
                                    major: m.Major ?? "Brak kierunku",
                                    joinDate: m.JoinDate != default ? m.JoinDate.ToString("dd.MM.yyyy") : "Brak daty",
                                    isActive: m.IsActive,
                                    isSystemAddTile: false
                                );

                                _allMembers.Add(itemVm);
                            }
                        }

                        ApplyFilter();
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
            }
        }

        private void ApplyFilter()
        {
            Members.Clear();
            var query = SearchQuery?.ToLower() ?? "";

            foreach (var item in _allMembers)
            {
                if (item.IsSystemAddTile ||
                    (!string.IsNullOrEmpty(item.FullName) && item.FullName.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(item.Role) && item.Role.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(item.Major) && item.Major.ToLower().Contains(query)))
                {
                    Members.Add(item);
                }
            }
        }
    }
}