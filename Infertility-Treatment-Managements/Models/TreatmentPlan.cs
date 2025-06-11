using Infertility_Treatment_Managements.Models;

namespace Infertility_Treatment_Managements.Models
{
    public partial class TreatmentPlan
    {
        public int TreatmentPlanId { get; set; }
        public int? DoctorId { get; set; }
        public string Method { get; set; }
        public int? PatientDetailId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }

        public virtual Doctor Doctor { get; set; }
        public virtual PatientDetail PatientDetail { get; set; }
        public virtual ICollection<TreatmentProcess> TreatmentProcesses { get; set; } = new List<TreatmentProcess>();
    }
}
