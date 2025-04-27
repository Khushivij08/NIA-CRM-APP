using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class MEvent : Auditable
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You cannot leave the member name blank.")]
        [Display(Name = "Event Name")]
        [StringLength(255, ErrorMessage = "Member name cannot be more than 255 characters long.")]
        public string EventName { get; set; } = "";

        [Display(Name = "Event Description")]
        public string? EventDescription { get; set; }

        [Display(Name = "Event Location")]
        [StringLength(255, ErrorMessage = "Event location cannot be more than 255 characters long.")]
        public string? EventLocation { get; set; }

        [Required(ErrorMessage = "Event Date is required.")]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        public DateTime EventDate { get; set; } = DateTime.Now;

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
        public ICollection<MemberEvent> MemberEvents { get; set; } = new List<MemberEvent>();
    }
}
