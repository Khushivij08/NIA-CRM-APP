using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Opportunity : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Opportunity Name")]
        [Required(ErrorMessage = "You cannot leave the opportunity name blank.")]
        [StringLength(255, ErrorMessage = "Opportunity name cannot be more than 255 characters long.")]
        public string OpportunityName { get; set; } = "";

        [Display(Name = "Action Item/Next Steps")]
        [Required(ErrorMessage = "You cannot leave the opportunity Action Item/Next Steps blank.")]
        public string OpportunityAction { get; set; } = "";

        [StringLength(255, ErrorMessage = "POC cannot be more than 255 characters long.")]
        public string? POC { get; set; }

        [StringLength(255, ErrorMessage = "Account cannot be more than 255 characters long.")]
        public string? Account { get; set; }

        public string? Interaction { get; set; }

        [Display(Name = "Last Contact")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? LastContact { get; set; }

        [Required(ErrorMessage = "You must select the opportunity status.")]
        [Display(Name = "Opportunity Status")]
        public OpportunityStatus OpportunityStatus { get; set; }

        [Required(ErrorMessage = "You must select the opportunity priority.")]
        [Display(Name = "Opportunity Priority")]
        public OpportunityPriority OpportunityPriority { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
    }
}