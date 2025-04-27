using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class DashboardLayout
    {
        public int Id { get; set; }

        // JSON string
        [Required]
        public string LayoutData { get; set; }

        // Optionally store the associated user's ID to support multiple users
        public string UserId { get; set; }
    }
}
