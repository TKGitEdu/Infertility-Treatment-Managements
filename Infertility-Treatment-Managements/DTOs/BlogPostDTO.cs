using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class BlogPostDTO
    {
        public string BlogPostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? LastModified { get; set; }
        public int ViewCount { get; set; }
        public string Tags { get; set; }

        // Author information
        public UserBasicDTO Author { get; set; }

        // Feedback count
        public int FeedbackCount { get; set; }
    }

    public class BlogPostCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime? PublishDate { get; set; }
        public string Tags { get; set; }
    }

    public class BlogPostUpdateDTO
    {
        public string BlogPostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime? PublishDate { get; set; }
        public string Tags { get; set; }
    }

    public class BlogPostBasicDTO
    {
        public string BlogPostId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public DateTime PublishDate { get; set; }
        public string AuthorName { get; set; }
    }
}