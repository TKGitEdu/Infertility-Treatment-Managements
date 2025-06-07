using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }
        public int? BookingId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }

        // Related entity
        public BookingBasicDTO Booking { get; set; }
    }

    public class PaymentBasicDTO
    {
        public int PaymentId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }

    public class PaymentCreateDTO
    {
        public int? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }

    public class PaymentUpdateDTO
    {
        public int PaymentId { get; set; }
        public int? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }
}