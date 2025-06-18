using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public RatingController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Rating
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RatingDTO>>> GetRatings()
        {
            var ratings = await _context.Ratings
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Service)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.RatingDate)
                .ToListAsync();

            if (ratings == null || !ratings.Any())
            {
                return NotFound("No ratings found");
            }

            var ratingDTOs = ratings.Select(r => new RatingDTO
            {
                RatingId = r.RatingId,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                ServiceId = r.ServiceId,
                BookingId = r.BookingId,
                Score = r.Score,
                Comment = r.Comment,
                RatingType = r.RatingType,
                RatingDate = r.RatingDate,
                Status = r.Status,
                IsAnonymous = r.IsAnonymous,
                Patient = !r.IsAnonymous && r.Patient != null ? new PatientBasicDTO
                {
                    PatientId = r.Patient.PatientId,
                    Name = r.Patient.Name,
                    Email = r.Patient.Email,
                    Phone = r.Patient.Phone
                } : null,
                Doctor = r.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = r.Doctor.DoctorId,
                    DoctorName = r.Doctor.DoctorName,
                    Specialization = r.Doctor.Specialization
                } : null,
                Service = r.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = r.Service.ServiceId,
                    Name = r.Service.Name,
                    Price = r.Service.Price
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description
                } : null
            }).ToList();

            return Ok(ratingDTOs);
        }

        // GET: api/Rating/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RatingDTO>> GetRating(string id)
        {
            var rating = await _context.Ratings
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Service)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found");
            }

            var ratingDTO = new RatingDTO
            {
                RatingId = rating.RatingId,
                PatientId = rating.PatientId,
                DoctorId = rating.DoctorId,
                ServiceId = rating.ServiceId,
                BookingId = rating.BookingId,
                Score = rating.Score,
                Comment = rating.Comment,
                RatingType = rating.RatingType,
                RatingDate = rating.RatingDate,
                Status = rating.Status,
                IsAnonymous = rating.IsAnonymous,
                Patient = !rating.IsAnonymous && rating.Patient != null ? new PatientBasicDTO
                {
                    PatientId = rating.Patient.PatientId,
                    Name = rating.Patient.Name,
                    Email = rating.Patient.Email,
                    Phone = rating.Patient.Phone
                } : null,
                Doctor = rating.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = rating.Doctor.DoctorId,
                    DoctorName = rating.Doctor.DoctorName,
                    Specialization = rating.Doctor.Specialization
                } : null,
                Service = rating.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = rating.Service.ServiceId,
                    Name = rating.Service.Name,
                    Price = rating.Service.Price
                } : null,
                Booking = rating.Booking != null ? new BookingBasicDTO
                {
                    BookingId = rating.Booking.BookingId,
                    DateBooking = rating.Booking.DateBooking,
                    Description = rating.Booking.Description
                } : null
            };

            return Ok(ratingDTO);
        }

        // GET: api/Rating/Doctor/{doctorId}
        [HttpGet("Doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<RatingDTO>>> GetRatingsByDoctor(string doctorId)
        {
            var ratings = await _context.Ratings
                .Include(r => r.Patient)
                .Include(r => r.Booking)
                .Where(r => r.DoctorId == doctorId && r.Status == "Approved")
                .OrderByDescending(r => r.RatingDate)
                .ToListAsync();

            if (ratings == null || !ratings.Any())
            {
                return NotFound($"No approved ratings found for doctor with ID {doctorId}");
            }

            var ratingDTOs = ratings.Select(r => new RatingDTO
            {
                RatingId = r.RatingId,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                ServiceId = r.ServiceId,
                BookingId = r.BookingId,
                Score = r.Score,
                Comment = r.Comment,
                RatingType = r.RatingType,
                RatingDate = r.RatingDate,
                Status = r.Status,
                IsAnonymous = r.IsAnonymous,
                Patient = !r.IsAnonymous && r.Patient != null ? new PatientBasicDTO
                {
                    PatientId = r.Patient.PatientId,
                    Name = r.Patient.Name,
                    Email = r.Patient.Email,
                    Phone = r.Patient.Phone
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description
                } : null
            }).ToList();

            return Ok(ratingDTOs);
        }

        // GET: api/Rating/Service/{serviceId}
        [HttpGet("Service/{serviceId}")]
        public async Task<ActionResult<IEnumerable<RatingDTO>>> GetRatingsByService(string serviceId)
        {
            var ratings = await _context.Ratings
                .Include(r => r.Patient)
                .Include(r => r.Booking)
                .Where(r => r.ServiceId == serviceId && r.Status == "Approved")
                .OrderByDescending(r => r.RatingDate)
                .ToListAsync();

            if (ratings == null || !ratings.Any())
            {
                return NotFound($"No approved ratings found for service with ID {serviceId}");
            }

            var ratingDTOs = ratings.Select(r => new RatingDTO
            {
                RatingId = r.RatingId,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                ServiceId = r.ServiceId,
                BookingId = r.BookingId,
                Score = r.Score,
                Comment = r.Comment,
                RatingType = r.RatingType,
                RatingDate = r.RatingDate,
                Status = r.Status,
                IsAnonymous = r.IsAnonymous,
                Patient = !r.IsAnonymous && r.Patient != null ? new PatientBasicDTO
                {
                    PatientId = r.Patient.PatientId,
                    Name = r.Patient.Name,
                    Email = r.Patient.Email,
                    Phone = r.Patient.Phone
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description
                } : null
            }).ToList();

            return Ok(ratingDTOs);
        }

        // POST: api/Rating
        [HttpPost]
        public async Task<ActionResult<RatingDTO>> CreateRating(RatingCreateDTO ratingDTO)
        {
            // Validate foreign keys if provided
            if (!string.IsNullOrEmpty(ratingDTO.PatientId))
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == ratingDTO.PatientId);
                if (!patientExists)
                {
                    return BadRequest($"Patient with ID {ratingDTO.PatientId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(ratingDTO.DoctorId))
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == ratingDTO.DoctorId);
                if (!doctorExists)
                {
                    return BadRequest($"Doctor with ID {ratingDTO.DoctorId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(ratingDTO.ServiceId))
            {
                var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == ratingDTO.ServiceId);
                if (!serviceExists)
                {
                    return BadRequest($"Service with ID {ratingDTO.ServiceId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(ratingDTO.BookingId))
            {
                var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == ratingDTO.BookingId);
                if (!bookingExists)
                {
                    return BadRequest($"Booking with ID {ratingDTO.BookingId} does not exist");
                }
            }

            // Validate score range
            if (ratingDTO.Score < 1 || ratingDTO.Score > 5)
            {
                return BadRequest("Score must be between 1 and 5");
            }

            var rating = new Rating
            {
                RatingId = "RT" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = ratingDTO.PatientId,
                DoctorId = ratingDTO.DoctorId,
                ServiceId = ratingDTO.ServiceId,
                BookingId = ratingDTO.BookingId,
                Score = ratingDTO.Score,
                Comment = ratingDTO.Comment,
                RatingType = ratingDTO.RatingType,
                RatingDate = DateTime.Now,
                Status = "Pending", // All ratings start as pending and need approval
                IsAnonymous = ratingDTO.IsAnonymous
            };

            _context.Ratings.Add(rating);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the rating: {ex.Message}");
            }

            var createdRatingDTO = new RatingDTO
            {
                RatingId = rating.RatingId,
                PatientId = rating.PatientId,
                DoctorId = rating.DoctorId,
                ServiceId = rating.ServiceId,
                BookingId = rating.BookingId,
                Score = rating.Score,
                Comment = rating.Comment,
                RatingType = rating.RatingType,
                RatingDate = rating.RatingDate,
                Status = rating.Status,
                IsAnonymous = rating.IsAnonymous
            };

            return CreatedAtAction(nameof(GetRating), new { id = rating.RatingId }, createdRatingDTO);
        }

        // PUT: api/Rating/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRating(string id, RatingUpdateDTO ratingDTO)
        {
            if (id != ratingDTO.RatingId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found");
            }

            // Validate score range
            if (ratingDTO.Score < 1 || ratingDTO.Score > 5)
            {
                return BadRequest("Score must be between 1 and 5");
            }

            // Update properties
            rating.Score = ratingDTO.Score;
            rating.Comment = ratingDTO.Comment;
            rating.Status = ratingDTO.Status;

            _context.Entry(rating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingExists(id))
                {
                    return NotFound($"Rating with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Rating/5/Status
        [HttpPatch("{id}/Status")]
        public async Task<IActionResult> UpdateRatingStatus(string id, [FromBody] string status)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found");
            }

            // Validate status
            if (status != "Approved" && status != "Rejected" && status != "Pending")
            {
                return BadRequest("Status must be 'Approved', 'Rejected', or 'Pending'");
            }

            rating.Status = status;
            _context.Entry(rating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingExists(id))
                {
                    return NotFound($"Rating with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Rating/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(string id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound($"Rating with ID {id} not found");
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/Rating/Average/Doctor/{doctorId}
        [HttpGet("Average/Doctor/{doctorId}")]
        public async Task<ActionResult<object>> GetAverageRatingForDoctor(string doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {doctorId} not found");
            }

            var ratings = await _context.Ratings
                .Where(r => r.DoctorId == doctorId && r.Status == "Approved")
                .ToListAsync();

            if (ratings == null || !ratings.Any())
            {
                return Ok(new
                {
                    DoctorId = doctorId,
                    DoctorName = doctor.DoctorName,
                    AverageRating = 0,
                    TotalRatings = 0
                });
            }

            double averageRating = ratings.Average(r => r.Score);
            int totalRatings = ratings.Count;

            return Ok(new
            {
                DoctorId = doctorId,
                DoctorName = doctor.DoctorName,
                AverageRating = Math.Round(averageRating, 1),
                TotalRatings = totalRatings
            });
        }

        // GET: api/Rating/Average/Service/{serviceId}
        [HttpGet("Average/Service/{serviceId}")]
        public async Task<ActionResult<object>> GetAverageRatingForService(string serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
            if (service == null)
            {
                return NotFound($"Service with ID {serviceId} not found");
            }

            var ratings = await _context.Ratings
                .Where(r => r.ServiceId == serviceId && r.Status == "Approved")
                .ToListAsync();

            if (ratings == null || !ratings.Any())
            {
                return Ok(new
                {
                    ServiceId = serviceId,
                    ServiceName = service.Name,
                    AverageRating = 0,
                    TotalRatings = 0
                });
            }

            double averageRating = ratings.Average(r => r.Score);
            int totalRatings = ratings.Count;

            return Ok(new
            {
                ServiceId = serviceId,
                ServiceName = service.Name,
                AverageRating = Math.Round(averageRating, 1),
                TotalRatings = totalRatings
            });
        }
        private bool RatingExists(string id)
        {
            return _context.Ratings.Any(e => e.RatingId == id);
        }
    }
}