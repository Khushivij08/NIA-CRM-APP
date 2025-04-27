using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class MembershipType
    {
        public int Id { get; set; }

        [Display(Name = "Membership Type Name")]
        [Required(ErrorMessage = "You cannot leave the membership type name blank.")]
        [StringLength(255, ErrorMessage = "Membership type name cannot be more than 255 characters long.")]
        public string TypeName { get; set; } = "";

        [Display(Name = "Membership Type Description")]
        public string? TypeDescr { get; set; }

        public ICollection<MemberMembershipType> MemberMembershipTypes { get; set; } = new List<MemberMembershipType>();

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
    }
}