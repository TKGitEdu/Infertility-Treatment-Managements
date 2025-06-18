using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.Models
{
    public partial class BlogPost
    {
        public string BlogPostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public string? AuthorId { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } // Draft, Published, Archived
        public DateTime PublishDate { get; set; }
        public DateTime? LastModified { get; set; }
        public int ViewCount { get; set; }
        public string? Tags { get; set; }

        // Navigation properties
        public virtual User Author { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}