using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Management.DTOs
{
    public class ExaminationDTO
    {
        public int ExaminationId { get; set; }
        public int? BookingId { get; set; }
        public DateTime? ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime? CreateAt { get; set; }

        // Related entity
        public BookingBasicDTO Booking { get; set; }
    }

    public class ExaminationBasicDTO
    {
        public int ExaminationId { get; set; }
        public DateTime? ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
    }

    public class ExaminationCreateDTO
    {
        public int BookingId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }

    public class ExaminationUpdateDTO
    {
        public int ExaminationId { get; set; }
        public int BookingId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }
}