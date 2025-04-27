using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Member : Auditable
    {
        public string TimeSinceJoined
        {
            get
            {
                DateTime today = DateTime.Today;
                int years = today.Year - JoinDate.Year
                    - ((today.Month < JoinDate.Month ||
                        (today.Month == JoinDate.Month && today.Day < JoinDate.Day)) ? 1 : 0);

                return years.ToString() + " year(s) ago";
            }
        }


        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the member name blank.")]
        [Display(Name = "Member Name")]
        [StringLength(255, ErrorMessage = "Member name cannot be more than 255 characters long.")]
        public string MemberName { get; set; } = "";

        [Required(ErrorMessage = "You cannot leave the size blank.")]
        [Display(Name = "Size")]
        public int MemberSize { get; set; }

        public string? WebsiteUrl { get; set; }

        [Required(ErrorMessage = "Join Date is required.")]
        [Display(Name = "Join Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "Member Note")]
        public string? MemberNote { get; set; }

       

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; }
        public MemberLogo? MemberLogo { get; set; }
        public MemberThumbnail? MemberThumbnail { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency

        public int AddressID { get; set; }
        public Address? Address { get; set; }
        public ICollection<MemberMembershipType> MemberMembershipTypes { get; set; } = new List<MemberMembershipType>();
        public ICollection<IndustryNAICSCode> IndustryNAICSCodes { get; set; } = new List<IndustryNAICSCode>();
        public ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();
        public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
        public ICollection<MemberContact> MemberContacts { get; set; } = new List<MemberContact>();
        public ICollection<MemberSector> MemberSectors { get; set; } = new List<MemberSector>();
        public ICollection<MemberTag> MemberTags { get; set; } = new List<MemberTag>();
        public ICollection<MemberEvent> MemberEvents { get; set; } = new List<MemberEvent>();
    }
}