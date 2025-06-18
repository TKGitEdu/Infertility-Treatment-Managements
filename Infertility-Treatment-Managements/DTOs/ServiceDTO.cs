using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class ServiceDTO
    {
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string Category { get; set; } // Thêm trường Category

        // Related entity
        public BookingBasicDTO Booking { get; set; }
    }

    public class ServiceBasicDTO
    {
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string Category { get; set; } // Thêm trường Category
    }

    public class ServiceCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string Category { get; set; } // Thêm trường Category
    }

    public class ServiceUpdateDTO
    {
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string Category { get; set; } // Thêm trường Category
    }
}