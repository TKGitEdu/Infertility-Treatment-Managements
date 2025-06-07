using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    // Main DTO for retrieving treatment process data
    public class TreatmentProcessDTO
    {
        public int TreatmentProcessId { get; set; }
        public string Method { get; set; }
        public int? PatientDetailId { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }

        // Include basic patient detail information
        public PatientDetailBasicDTO PatientDetail { get; set; }
    }

    // DTO for creating a new treatment process
    public class TreatmentProcessCreateDTO
    {
        public string Method { get; set; }
        public int PatientDetailId { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
    }

    // DTO for updating an existing treatment process
    public class TreatmentProcessUpdateDTO
    {
        public int TreatmentProcessId { get; set; }
        public string Method { get; set; }
        public int PatientDetailId { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
    }

    // Simplified PatientDetail DTO for treatment process response
    public class PatientDetailBasicDTO
    {
        public int PatientDetailId { get; set; }
        public int? PatientId { get; set; }
        public string TreatmentStatus { get; set; }
        public PatientBasicDTO Patient { get; set; }
    }
}