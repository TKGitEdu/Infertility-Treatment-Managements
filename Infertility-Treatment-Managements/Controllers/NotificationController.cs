using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IEmailService _emailService;

        public NotificationController(InfertilityTreatmentManagementContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetAllNotifications()
        {
            var notifications = await _context.Notifications
                .Select(n => new NotificationDTO
                {
                    NotificationId = n.NotificationId,
                    PatientId = n.PatientId,
                    DoctorId = n.DoctorId,
                    BookingId = n.BookingId,
                    TreatmentProcessId = n.TreatmentProcessId,
                    Type = n.Type,
                    Message = n.Message,
                    Time = n.Time
                })
                .ToListAsync();

            return Ok(notifications);
        }


    }
}
