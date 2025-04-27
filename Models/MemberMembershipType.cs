using System.Reflection;

namespace NIA_CRM.Models
{
    public class MemberMembershipType
    {
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public int MembershipTypeId { get; set; }

        public MembershipType? MembershipType { get; set; }
    }
}