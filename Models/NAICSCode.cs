using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class NAICSCode
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        public string? Code { get; set; } // No need for nullable since it's now required

        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; } // No need for nullable since it's now required

        public ICollection<IndustryNAICSCode> IndustryNAICSCodes { get; set; } = new List<IndustryNAICSCode>();
    }
}
