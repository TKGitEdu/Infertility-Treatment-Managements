using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class ContentPageDTO
    {
        public string ContentPageId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string PageType { get; set; }
        public string Status { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedById { get; set; }
        public string LastModifiedById { get; set; }

        // Creator information
        public UserBasicDTO CreatedBy { get; set; }
        public UserBasicDTO LastModifiedBy { get; set; }
    }

    public class ContentPageCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string PageType { get; set; }
        public string Status { get; set; }
        public int DisplayOrder { get; set; }
        public string CreatedById { get; set; }
    }

    public class ContentPageUpdateDTO
    {
        public string ContentPageId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string PageType { get; set; }
        public string Status { get; set; }
        public int DisplayOrder { get; set; }
        public string LastModifiedById { get; set; }
    }

    public class ContentPageBasicDTO
    {
        public string ContentPageId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string PageType { get; set; }
        public int DisplayOrder { get; set; }
    }
}