using Infertility_Treatment_Managements.DTOs;
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
    public class TreatmentPlanController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public TreatmentPlanController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

    }
}