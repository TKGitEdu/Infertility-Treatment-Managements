using System;

namespace Infertility_Treatment_Managements.Models
{
    public partial class Feedback
    {
        public string FeedbackId { get; set; }
        public string? PatientId { get; set; }
        public string? UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? BlogPostId { get; set; }
        public string? ServiceId { get; set; }
        public string FeedbackType { get; set; } // General, Service, Blog, Treatment
        public string Status { get; set; } // New, Read, Responded, Archived
        public DateTime CreateDate { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? RespondedById { get; set; }
        public bool IsPublic { get; set; }

        // Navigation properties
        public virtual Patient Patient { get; set; }
        public virtual User User { get; set; }
        public virtual User RespondedBy { get; set; }
        public virtual BlogPost BlogPost { get; set; }
        public virtual Service Service { get; set; }
    }
}