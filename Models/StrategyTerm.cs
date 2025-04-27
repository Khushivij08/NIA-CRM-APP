using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum StrategyTerm
    {
        [Display(Name = "Long-Term")]
        LongTerm,
        [Display(Name = "Short-Term")]
        ShortTerm,
        [Display(Name = "Medium-Term")]
        MediumTerm,
        Seasonal,
        Cyclical,
        [Display(Name = "Not Stated")]
        NotStated
    }
}
