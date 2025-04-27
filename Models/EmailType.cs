using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum EmailType
    {
        Production,
        Welcome,
        Promotion,
        Reminder,
        [Display(Name= "Event Invitation")]
        EventInvitation,
        [Display(Name = "Product Updates")]
        ProductUpdates,
        Cancellation,
        Other
    }
}
