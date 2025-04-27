using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Sector
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You cannot leave the sector name blank.")]
        [Display(Name = "Sector Name")]
        [StringLength(255, ErrorMessage = "Sector name cannot be more than 255 characters long.")]
        public string SectorName { get; set; } = "";

        [Display(Name = "Sector Description")]
        public string? SectorDescription { get; set; }

        public ICollection<MemberSector> MemberSectors { get; set; } = new List<MemberSector>();

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency


    }
}
