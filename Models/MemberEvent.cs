namespace NIA_CRM.Models
{
    public class MemberEvent
    {
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public int MEventID { get; set; }

        public MEvent? MEvent { get; set; }

    }
}
