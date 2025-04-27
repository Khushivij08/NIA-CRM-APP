using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class EmailModal
    {
        //[Required(ErrorMessage = "Email address is required.")]
        //[EmailAddress(ErrorMessage = "Invalid email address format.")]
        //public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters.")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Body is required.")]
        [DataType(DataType.MultilineText)]
        public string? Body { get; set; }
    }
}
