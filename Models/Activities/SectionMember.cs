using Models.Enums;
using Models.Users;

namespace Models.Activities
{
    /// <summary>
    /// Klasa pośrednicząca łącząca studenta (Member) z konkretną sekcją koła (Section).
    /// </summary>
    public class SectionMember
    {
        /// <summary>
        /// ID sekcji - klucz obcy.
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Referencja do sekcji koła naukowego (relacja).
        /// </summary>
        public Section? Section { get; set; }

        /// <summary>
        /// ID członka koła - klucz obcy.
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// Referencja do członka koła (relacja).
        /// </summary>
        public Member? Member { get; set; }

        /// <summary>
        /// Rola, jaką ten członek pełni w danej sekcji (np. Lider, Członek).
        /// </summary>
        public SectionRole Role { get; set; } = SectionRole.Member;
    }
}

