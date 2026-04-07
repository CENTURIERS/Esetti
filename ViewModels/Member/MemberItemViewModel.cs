using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Esseti.ViewModels.Member
{
    public partial class MemberItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private byte[] _avatar = Array.Empty<byte>();

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _role = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _collegeDepartment = string.Empty;

        [ObservableProperty]
        private string _major = string.Empty;

        [ObservableProperty]
        private string _joinDate = string.Empty;

        [ObservableProperty]
        private bool _isActive = true;

        public string FullName => $"{FirstName} {LastName}";

        public MemberItemViewModel(byte[] avatar, string firstName, string lastName, string role, string email, string collegeDepartment, string major, string joinDate, bool isActive)
        {
            Avatar = avatar;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            Email = email;
            CollegeDepartment = collegeDepartment;
            Major = major;
            JoinDate = joinDate;
            IsActive = isActive;
        }

    }
}
