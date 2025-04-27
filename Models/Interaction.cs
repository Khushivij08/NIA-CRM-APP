using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Interaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Member ID is required.")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Contact ID is required.")]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Interaction Date is required.")]
        public DateTime InteractionDate { get; set; }

        public int? OpportunityId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters.")]
        public string? InteractionNotes { get; set; }

        [Required(ErrorMessage = "Creation date is required.")]
        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "Update date is required.")]
        public DateTime? UpdatedAt { get; set; }

        public Member Member { get; set; }
        public Contact Contact { get; set; }
        public Opportunity Opportunity { get; set; }
    }
}
