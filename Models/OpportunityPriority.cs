using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum OpportunityPriority
    {
        [Display(Name = "Extremely High")]
        ExtremelyHigh,
        High,
        Medium,
        Low
    }
}
