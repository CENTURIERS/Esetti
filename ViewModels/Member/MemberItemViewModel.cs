using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace Esseti.ViewModels.Member
{
    public partial class MemberItemViewModel : ViewModelBase
    {
        public int MemberId { get; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private Bitmap? _avatar;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _role = string.Empty;

        [ObservableProperty]
        private string _indexNumber = string.Empty;

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

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isSystemAddTile;

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string FullFromDate => $"Od {JoinDate} r.";

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

        public MemberItemViewModel(int memberId, byte[] avatar, string firstName, string lastName, string role, string indexNumber, string email, string collegeDepartment, string major, string joinDate, bool isActive, string description = "", bool isSystemAddTile = false)
        {
            MemberId = memberId;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            IndexNumber = indexNumber;
            Email = email;
            CollegeDepartment = collegeDepartment;
            Major = major;
            JoinDate = joinDate;
            IsActive = isActive;
            Description = description;
            IsSystemAddTile = isSystemAddTile;

            if (avatar != null && avatar.Length > 0)
            {
                try { using var ms = new MemoryStream(avatar); Avatar = new Bitmap(ms); }
                catch { Avatar = SafeDefaultAvatar; }
            }
            else Avatar = SafeDefaultAvatar;
        }

        public override void Dispose()
        {
            if (Avatar != null && Avatar != _defaultAvatar)
            {
                Avatar.Dispose();
                Avatar = null;
            }
            base.Dispose();
        }
    }
}

