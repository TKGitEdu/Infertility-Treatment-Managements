using System.ComponentModel.DataAnnotations;

namespace Infertility_Treatment_Managements.Models
{
    public class TreatmentMedication
    {
        [Key]
        public string MedicationId { get; set; }

        public string? TreatmentPlanId { get; set; }

        [Required]
        public string DrugType { get; set; }

        [Required]
        public string DrugName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        // Navigation
        public virtual TreatmentPlan TreatmentPlan { get; set; }
    }
}
