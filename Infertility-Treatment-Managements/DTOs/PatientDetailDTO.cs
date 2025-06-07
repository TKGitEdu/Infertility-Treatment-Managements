using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Management.DTOs
{
    public class PatientDetailDTO
    {
        public int PatientDetailId { get; set; }
        public int? PatientId { get; set; }
        public string TreatmentStatus { get; set; }

        // Basic patient information
        public PatientBasicDTO Patient { get; set; }

        // List of simplified treatment processes
        public ICollection<TreatmentProcessBasicDTO> TreatmentProcesses { get; set; } = new List<TreatmentProcessBasicDTO>();
    }

    public class PatientDetailCreateDTO
    {
        public int PatientId { get; set; }
        public string TreatmentStatus { get; set; }
    }

    public class PatientDetailUpdateDTO
    {
        public int PatientDetailId { get; set; }
        public int PatientId { get; set; }
        public string TreatmentStatus { get; set; }
    }

    // Simplified TreatmentProcess DTO for PatientDetail response
    public class TreatmentProcessBasicDTO
    {
        public int TreatmentProcessId { get; set; }
        public string Method { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}