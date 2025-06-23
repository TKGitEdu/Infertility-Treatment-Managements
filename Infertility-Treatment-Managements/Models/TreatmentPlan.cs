using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.Models
{
    public partial class TreatmentPlan
    {
        public string TreatmentPlanId { get; set; }
        public string? DoctorId { get; set; }
        public string? ServiceId { get; set; } // New property for ServiceID
        public string Method { get; set; }
        public string? PatientDetailId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }

        public virtual Doctor Doctor { get; set; }
        public virtual Service Service { get; set; } // New navigation property
        public virtual PatientDetail PatientDetail { get; set; }
        public virtual ICollection<TreatmentProcess> TreatmentProcesses { get; set; } = new List<TreatmentProcess>();
    }
}