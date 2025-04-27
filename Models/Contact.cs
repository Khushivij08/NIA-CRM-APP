using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Contact : Auditable
    {
        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                if (Phone != null && Phone.Contains("("))
                {
                    return Phone;
                }
                return "(" + Phone?.Substring(0, 3) + ") " + Phone?.Substring(3, 3) + "-" + Phone?[6..];
            }
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle Name cannot be longer than 50 characters.")]
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = "";

        [StringLength(10, ErrorMessage = "Title cannot be longer than 10 characters.")]
        public string? Title { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot be longer than 100 characters.")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
        public string? Phone { get; set; }

        [Url(ErrorMessage = "Invalid LinkedIn URL.")]
        [StringLength(200, ErrorMessage = "LinkedIn URL cannot be longer than 200 characters.")]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "Contact Note")]
        public string? ContactNote { get; set; }

        [Display(Name = "VIP")]
        public bool IsVip { get; set; } = false;

        [Display(Name = "Archieved")]
        public bool IsArchieved { get; set; } = false;

        public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

        [Required(ErrorMessage = "Mamber is required")]
        public ICollection<MemberContact> MemberContacts { get; set; } = new List<MemberContact>();
        public ICollection<ContactCancellation> ContactCancellations { get; set; } = new List<ContactCancellation>();


        public ContactLogo? ContactLogo { get; set; }
        public ContactThumbnail? ContactThumbnail { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency

        [Display(Name = "Contact Name")]
        public string Summary
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }
    }
}
