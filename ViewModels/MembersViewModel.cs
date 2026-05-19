using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
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

            Task.Run(() => LoadDataAsync());
        }

        protected override void OnSearchQueryUpdated(string value)
        {
            ApplyFilter();
        }

        private async Task LoadDataAsync()
        {
            var membersFromDb = await _memberRepository.GetAllMembersAsync();

            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
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

                foreach (var m in membersFromDb)
                {
                    var itemVm = new MemberItemViewModel(
                        avatar: m.MemberAvatar ?? Array.Empty<byte>(),
                        firstName: m.FirstName,
                        lastName: m.LastName,
                        role: m.AuthorityRole?.Name ?? "Brak roli",
                        indexNumber: m.IndexNumber ?? "00000000",
                        email: m.Account?.Email ?? "Brak@email.pl",
                        collegeDepartment: m.Department?.Name ?? "Brak wydziału",
                        major: m.Major ?? "Brak kierunku",
                        joinDate: m.JoinDate.ToString("dd.MM.yyyy"),
                        isActive: m.IsActive,
                        isSystemAddTile: false
                    );

                    _allMembers.Add(itemVm);
                }

                ApplyFilter();
            });
        }

        private void ApplyFilter()
        {
            Members.Clear();

            var query = SearchQuery?.ToLower() ?? "";

            foreach (var item in _allMembers)
            {
                if (item.IsSystemAddTile || 
                    item.FullName.ToLower().Contains(query) ||
                    item.Role.ToLower().Contains(query) ||
                    item.Major.ToLower().Contains(query))
                {
                    Members.Add(item);
                }

            }
        }
    }
}
