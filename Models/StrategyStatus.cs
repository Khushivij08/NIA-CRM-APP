using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum StrategyStatus
    {
        [Display(Name = "To Do")]
        ToDo,
        [Display(Name = "In Progress")]
        InProgress,
        Cancelled,
        Done
    }
}
