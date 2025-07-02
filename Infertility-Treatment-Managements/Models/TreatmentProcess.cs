using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.Models
{
    public partial class TreatmentProcess
    {
        public string TreatmentProcessId { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientDetailId { get; set; }
        public string? TreatmentPlanId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }

        public virtual Doctor Doctor { get; set; } // Added navigation property
        public virtual PatientDetail PatientDetail { get; set; }
        public virtual TreatmentPlan TreatmentPlan { get; set; }
    }
}