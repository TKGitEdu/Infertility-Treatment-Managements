using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.Models
{
    public partial class ContentPage
    {
        public string ContentPageId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; } // URL-friendly version of the title
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string PageType { get; set; } // Home, About, Service, FAQ, etc.
        public string Status { get; set; } // Draft, Published, Archived
        public int DisplayOrder { get; set; } // For ordering pages in navigation
        public DateTime CreateDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string? CreatedById { get; set; }
        public string? LastModifiedById { get; set; }

        // Navigation properties
        public virtual User CreatedBy { get; set; }
        public virtual User LastModifiedBy { get; set; }
    }
}