using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public enum Province
    {
        Alberta,
        BritishColumbia,
        Manitoba,
        [Display(Name = "New Brunswick")]
        NewBrunswick,
        [Display(Name = "Newfoundland and Labrador")]
        NewfoundlandandLabrador,
        [Display(Name = "Nova Scotia")]
        NovaScotia,
        Ontario,
        [Display(Name = "Prince Edward Island")]
        PrinceEdwardIsland,
        Quebec,
        Saskatchewan,
        Yukon,
        Nunavut,
        [Display(Name = "Northwest Territories")]
        NorthwestTerritories
    }
}
