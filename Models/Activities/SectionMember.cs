using Models.Enums;
using Models.Users;

namespace Models.Activities
{
    public class SectionMember
    {
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public SectionRole Role { get; set; } = SectionRole.Member;
    }
}

