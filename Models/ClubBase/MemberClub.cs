using Models.Enums;
using Models.Users;
using Models.ClubBase;

namespace Models.ClubBase
{
    public class MemberClub
    {
        public int ClubId { get; set; }
        public ClubInfo? Club { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public ClubRole ClubRole { get; set; } = ClubRole.Member;
    }
}