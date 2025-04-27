using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class IndustryNAICSCode
    {
        [Key]
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int NAICSCodeId { get; set; }

        public Member? Member { get; set; }
        public NAICSCode? NAICSCode { get; set; }
    }
}
