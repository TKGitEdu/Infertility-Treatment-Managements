namespace Infertility_Treatment_Managements.DTOs
{
    public class MedicalDTO
    {
        public string? MedicationId { get; set; }
        public string? TreatmentPlanId { get; set; }
        public string? DrugType { get; set; }
        public string? DrugName { get; set; }
        public string? Description { get; set; }
        // Additional properties can be added as needed
        // e.g., Dosage, Frequency, StartDate, EndDate, etc.
    }
}
