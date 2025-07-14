using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class PaymentDTO
    {
        public string PaymentId { get; set; }
        public string? BookingId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public bool? Confirmed { get; set; }

        // Related entity
        public BookingBasicDTO Booking { get; set; }
    }

    public class PaymentBasicDTO
    {
        public string PaymentId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public bool? Confirmed { get; set; } // Trạng thái đã xác nhận lần 2 bởi admin thực hiện, thanh toán hay chưa
    }

    public class PaymentCreateDTO
    {
        public string? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public bool? Confirmed { get; set; } // Trạng thái đã xác nhận lần 2 bởi admin thực hiện, thanh toán hay chưa
    }

    public class PaymentUpdateDTO
    {
        public string PaymentId { get; set; }
        public string? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }
}