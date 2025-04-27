using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NIA_CRM.Models
{
    public class ProductionEmail:Auditable
    {
        [Key] // Marks the Id property as the primary key
        public int Id { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [MaxLength(255, ErrorMessage = "Template name cannot be longer than 255 characters")]
        [DisplayName("Template Name")]
        public string TemplateName { get; set; } = "";

        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(255, ErrorMessage = "Subject cannot be longer than 255 characters")]
        public string? Subject { get; set; } = "";

        [Required(ErrorMessage = "Body content is required")]
        [DisplayName("Body Content")]
        [DataType(DataType.MultilineText)]
        public string? Body { get; set; } // Body content of the email

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency

        [Required(ErrorMessage = "Email type is required")]
        [DisplayName("Email Type")]
        public EmailType EmailType { get; set; }

        //[Required]
        //[DisplayName("Is Active")]
        //public bool IsActive { get; set; } // Whether the template is active or not
    }
}
