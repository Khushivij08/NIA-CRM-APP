namespace NIA_CRM.Models
{
    public class MemberContact
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int ContactId { get; set; }

        public Member? Member { get; set; }
        public Contact? Contact { get; set; }
    }
}
