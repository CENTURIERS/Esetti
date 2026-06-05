using Models.Enums;
using Models.Users;
using Models.ClubBase;

namespace Models.ClubBase
{
    /// <summary>
    /// Klasa pośrednicząca w relacji wiele do wielu łącząca studenta (Member) z kołem naukowym (ClubInfo).
    /// </summary>
    public class MemberClub
    {
        /// <summary>
        /// ID koła naukowego - klucz obcy.
        /// </summary>
        public int ClubId { get; set; }

        /// <summary>
        /// Referencja do koła naukowego.
        /// </summary>
        public ClubInfo? Club { get; set; }

        /// <summary>
        /// ID członka koła - klucz obcy.
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// Referencja do członka koła.
        /// </summary>
        public Member? Member { get; set; }

        /// <summary>
        /// Rola pełniona przez tego członka w konkretnym kole naukowym.
        /// </summary>
        public ClubRole ClubRole { get; set; } = ClubRole.Member;
    }
}

