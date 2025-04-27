using System.ComponentModel.DataAnnotations;

namespace NIA_CRM.Models
{
    public class Strategy : Auditable
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Strategy Name")]
        [Required(ErrorMessage = "Strategy name is required.")]
        public string StrategyName { get; set; } = "";

        [Display(Name = "Assignee")]
        public string? StrategyAssignee { get; set; }

        [Display(Name = "Strategy Note")]
        public string? StrategyNote { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Automatically set to current date

        [Required(ErrorMessage = "You must select the opportunity priority.")]
        [Display(Name = "Strategy Term")]
        public StrategyTerm StrategyTerm { get; set; }

        [Required(ErrorMessage = "You must select the opportunity priority.")]
        [Display(Name = "Strategy Status")]
        public StrategyStatus StrategyStatus { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency
    }
}
