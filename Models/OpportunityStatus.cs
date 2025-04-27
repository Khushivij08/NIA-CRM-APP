using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum OpportunityStatus
    {

       Negotiating,
        Qualification,
        [Display(Name = "Closed New Member")]
        ClosedNewMember,
        [Display(Name = "Closed Not Interested")]
        ClosedNotInterested
    }
}