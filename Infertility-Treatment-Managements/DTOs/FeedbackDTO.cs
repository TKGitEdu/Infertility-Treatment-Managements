using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class FeedbackDTO
    {
        public string FeedbackId { get; set; }
        public string PatientId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string BlogPostId { get; set; }
        public string ServiceId { get; set; }
        public string FeedbackType { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string AdminResponse { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string RespondedById { get; set; }
        public bool IsPublic { get; set; }

        // Related entities information
        public PatientBasicDTO Patient { get; set; }
        public UserBasicDTO User { get; set; }
        public UserBasicDTO RespondedBy { get; set; }
        public BlogPostBasicDTO BlogPost { get; set; }
        public ServiceBasicDTO Service { get; set; }
    }

    public class FeedbackCreateDTO
    {
        public string PatientId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string BlogPostId { get; set; }
        public string ServiceId { get; set; }
        public string FeedbackType { get; set; }
        public bool IsPublic { get; set; }
    }

    public class FeedbackUpdateDTO
    {
        public string FeedbackId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string AdminResponse { get; set; }
        public string RespondedById { get; set; }
        public bool IsPublic { get; set; }
    }

    public class FeedbackBasicDTO
    {
        public string FeedbackId { get; set; }
        public string Title { get; set; }
        public string FeedbackType { get; set; }
        public DateTime CreateDate { get; set; }
        public string Status { get; set; }
    }
}