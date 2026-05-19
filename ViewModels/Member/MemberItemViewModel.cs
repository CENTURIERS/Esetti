using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace Esseti.ViewModels.Member
{
    public partial class MemberItemViewModel : ViewModelBase
    {
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
        private bool _isSystemAddTile;

        private static readonly Bitmap DefaultAvatar = new Bitmap(AssetLoader.Open(new Uri("avares://Esseti/Assets/user-default.png")));

        public string FullName => $"{FirstName} {LastName}";

        public string FullFromDate => $"Od {JoinDate} r.";


        public MemberItemViewModel(byte[] avatar, string firstName, string lastName, string role, string indexNumber, string email, string collegeDepartment, string major, string joinDate, bool isActive, bool isSystemAddTile = false)
        {
            if (avatar != null && avatar.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(avatar))
                    {
                        Avatar = new Bitmap(ms);
                    }
                } catch
                {
                    Avatar = DefaultAvatar;
                }
            }
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            IndexNumber = indexNumber;
            Email = email;
            CollegeDepartment = collegeDepartment;
            Major = major;
            JoinDate = joinDate;
            IsActive = isActive;
            IsSystemAddTile = isSystemAddTile;
        }

    }
}
