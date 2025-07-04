﻿using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class SlotDTO
    {
        public string SlotId { get; set; }
        public string SlotName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        // Related entities
        public ICollection<BookingBasicDTO> Bookings { get; set; } = new List<BookingBasicDTO>();
    }

    public class SlotBasicDTO
    {
        public string SlotId { get; set; }
        public string SlotName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class SlotCreateDTO
    {
        public string SlotName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class SlotUpdateDTO
    {
        public string SlotId { get; set; }
        public string SlotName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}