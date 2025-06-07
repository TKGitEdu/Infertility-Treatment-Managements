using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }

        // Related entity
        public BookingBasicDTO Booking { get; set; }
    }

    public class ServiceBasicDTO
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }

    public class ServiceCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }

    public class ServiceUpdateDTO
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }
}