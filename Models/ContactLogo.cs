using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class ContactLogo
    {
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public byte[]? Content { get; set; }

        [StringLength(255)]
        public string? MimeType { get; set; }

        public int ContactID { get; set; }
        public Contact? Contact { get; set; }
    }
}
