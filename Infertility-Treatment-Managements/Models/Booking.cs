﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.Models;

public partial class Booking
{
    public string BookingId { get; set; }

    public string? PatientId { get; set; }

    public string? ServiceId { get; set; }

    public string? PaymentId { get; set; }

    public string? DoctorId { get; set; }

    public string? SlotId { get; set; }

    public DateTime DateBooking { get; set; }

    public string Description { get; set; }

    public DateTime? CreateAt { get; set; }
    public string Status { get; set; } // hoặc StatusId nếu bạn dùng bảng Status riêng

    public string Note { get; set; }

    public virtual Doctor Doctor { get; set; }

    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();

    public virtual Patient Patient { get; set; }

    public virtual Payment Payment { get; set; }

    public virtual Service Service { get; set; }

    public virtual Slot Slot { get; set; }
}