using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class PatientDetailDTO
    {
        public string PatientDetailId { get; set; }
        public string? PatientId { get; set; }
        public string TreatmentStatus { get; set; }
        public string Name { get; set; }

        // Basic patient information
        public PatientBasicDTO Patient { get; set; }

        // List of simplified treatment processes
        public ICollection<TreatmentProcessBasicDTO> TreatmentProcesses { get; set; } = new List<TreatmentProcessBasicDTO>();
        
        // List of simplified treatment plans
        public ICollection<TreatmentPlanBasicDTO> TreatmentPlans { get; set; } = new List<TreatmentPlanBasicDTO>();
    }

    public class PatientDetailCreateDTO
    {
        public string PatientId { get; set; }
        public string TreatmentStatus { get; set; }
    }

    public class PatientDetailUpdateDTO
    {
        public string PatientDetailId { get; set; }
        public string PatientId { get; set; }
        public string TreatmentStatus { get; set; }
    }

    // Simplified TreatmentProcess DTO for PatientDetail response
    public class TreatmentProcessBasicDTO
    {
        public string TreatmentProcessId { get; set; }
        public string Method { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}