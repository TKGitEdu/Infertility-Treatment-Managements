using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class TreatmentPlanDTO
    {
        public int TreatmentPlanId { get; set; }
        public int? DoctorId { get; set; }
        public string Method { get; set; }
        public int? PatientDetailId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }

        // Navigation properties
        public DoctorBasicDTO Doctor { get; set; }
        public PatientDetailBasicDTO PatientDetail { get; set; }
        public ICollection<TreatmentProcessBasicDTO> TreatmentProcesses { get; set; } = new List<TreatmentProcessBasicDTO>();
    }

    public class TreatmentPlanBasicDTO
    {
        public int TreatmentPlanId { get; set; }
        public string Method { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }
    }

    public class TreatmentPlanCreateDTO
    {
        public int DoctorId { get; set; }
        public string Method { get; set; }
        public int PatientDetailId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }
    }

    public class TreatmentPlanUpdateDTO
    {
        public int TreatmentPlanId { get; set; }
        public int DoctorId { get; set; }
        public string Method { get; set; }
        public int PatientDetailId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; }
        public string TreatmentDescription { get; set; }
    }
}