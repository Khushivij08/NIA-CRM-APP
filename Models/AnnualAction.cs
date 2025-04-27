using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class AnnualAction : Auditable
    {
        public int ID { get; set; }

        [Display(Name="Action Name")]
        [Required(ErrorMessage = "You cannot leave the annual action name blank.")]
        [StringLength(255, ErrorMessage = "Annual action name cannot be more than 255 characters long.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "You cannot leave the annual action note blank.")]
        [StringLength(255, ErrorMessage = "Annual action note cannot be more than 255 characters long.")]
        public string Note { get; set; } = "";

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        [StringLength(255, ErrorMessage = "Assignee cannot be more than 255 characters long.")]
        public string? Asignee { get; set; }

        [Display(Name = "Annual Status")]
        [Required(ErrorMessage = "You must select the annual action status.")]
        public AnnualStatus AnnualStatus { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
    }
}
