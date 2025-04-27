using NIA_CRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NIA_CRM.Models
{
    public class Address
    {
        [Display(Name = "Address")]
        public string FormattedAddress
        {
            get
            {
                var parts = new List<string>
            {
                AddressLine1,
                AddressLine2,
                City,
                StateProvince.ToString(),
                PostalCode
            }.Where(p => !string.IsNullOrWhiteSpace(p));

                return string.Join(", ", parts);
            }
        }

        [Key]
        public int Id { get; set; } // Primary Key

        

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [MaxLength(255, ErrorMessage = "Address Line 1 cannot exceed 255 characters.")]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = "";

        [MaxLength(255, ErrorMessage = "Address Line 2 cannot exceed 255 characters.")]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        [Display(Name = "City")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "Province is required.")]
        [Display(Name = "Province")]
        public Province StateProvince { get; set; }

        [Required(ErrorMessage = "Postal Code is required.")]
        [MaxLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters.")]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$", ErrorMessage = "Invalid Postal Code. Must be in the format 'A1A 1A1'")]

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        // Navigation Property
        [ForeignKey(nameof(Member))]
        [Required(ErrorMessage = "Member is required.")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Display(Name = "Member")]
        public Member? Member { get; set; }



    }
}