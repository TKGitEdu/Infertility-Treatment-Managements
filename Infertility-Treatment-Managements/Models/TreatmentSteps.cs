using System.ComponentModel.DataAnnotations;

namespace Infertility_Treatment_Managements.Models
{
    // Lưu trữ thông tin các bước trong quá trình điều trị
    public class TreatmentStep
    {
        public string TreatmentStepId { get; set; } // khóa chính

        // Khóa ngoại liên kết đến TreatmentPlan
        public string TreatmentPlanId { get; set; }

        // Thứ tự của bước (gợi ý thêm)
        public int StepOrder { get; set; }

        public string StepName { get; set; }

        public string Description { get; set; }

        // Navigation property
        public virtual TreatmentPlan TreatmentPlan { get; set; }
    }
}
