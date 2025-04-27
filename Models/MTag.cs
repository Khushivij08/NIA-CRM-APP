using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class MTag
    {
            
        public int Id { get; set; }

        [Required(ErrorMessage = "You cannot leave the tag name blank.")]
        [Display(Name = "Tag Name")]
        [StringLength(255, ErrorMessage = "Tag name cannot be more than 255 characters long.")]
        public string MTagName { get; set; } = "";
        [Display(Name = "Tag Description")]
        public string? MTagDescription { get; set; }

        public ICollection<MemberTag> MemberTags { get; set; } = new List<MemberTag>();

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency

    }
}
