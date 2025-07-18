﻿using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class ExaminationDTO
    {
        public string ExaminationId { get; set; }
        public string? BookingId { get; set; }
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
        public string ExaminationId { get; set; }
        public DateTime? ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
    }

    public class ExaminationCreateDTO
    {
        public string BookingId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }

    public class ExaminationUpdateDTO
    {
        public string ExaminationId { get; set; }
        public string BookingId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationDescription { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }
}