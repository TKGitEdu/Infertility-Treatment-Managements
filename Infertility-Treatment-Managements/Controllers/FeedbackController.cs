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
    public class FeedbackController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public FeedbackController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Feedback
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetFeedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.RespondedBy)
                .Include(f => f.BlogPost)
                .Include(f => f.Service)
                .OrderByDescending(f => f.CreateDate)
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound("No feedbacks found");
            }

            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO
            {
                FeedbackId = f.FeedbackId,
                PatientId = f.PatientId,
                UserId = f.UserId,
                Title = f.Title,
                Content = f.Content,
                BlogPostId = f.BlogPostId,
                ServiceId = f.ServiceId,
                FeedbackType = f.FeedbackType,
                Status = f.Status,
                CreateDate = f.CreateDate,
                AdminResponse = f.AdminResponse,
                ResponseDate = f.ResponseDate,
                RespondedById = f.RespondedById,
                IsPublic = f.IsPublic,
                Patient = f.Patient != null ? new PatientBasicDTO
                {
                    PatientId = f.Patient.PatientId,
                    Name = f.Patient.Name,
                    Email = f.Patient.Email,
                    Phone = f.Patient.Phone
                } : null,
                User = f.User != null ? new UserBasicDTO
                {
                    UserId = f.User.UserId,
                    Username = f.User.Username,
                    FullName = f.User.FullName
                } : null,
                RespondedBy = f.RespondedBy != null ? new UserBasicDTO
                {
                    UserId = f.RespondedBy.UserId,
                    Username = f.RespondedBy.Username,
                    FullName = f.RespondedBy.FullName
                } : null,
                BlogPost = f.BlogPost != null ? new BlogPostBasicDTO
                {
                    BlogPostId = f.BlogPost.BlogPostId,
                    Title = f.BlogPost.Title,
                    Summary = f.BlogPost.Summary,
                    Category = f.BlogPost.Category,
                    PublishDate = f.BlogPost.PublishDate
                } : null,
                Service = f.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = f.Service.ServiceId,
                    Name = f.Service.Name,
                    Price = f.Service.Price
                } : null
            }).ToList();

            return Ok(feedbackDTOs);
        }

        // GET: api/Feedback/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackDTO>> GetFeedback(string id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.RespondedBy)
                .Include(f => f.BlogPost)
                .Include(f => f.Service)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found");
            }

            var feedbackDTO = new FeedbackDTO
            {
                FeedbackId = feedback.FeedbackId,
                PatientId = feedback.PatientId,
                UserId = feedback.UserId,
                Title = feedback.Title,
                Content = feedback.Content,
                BlogPostId = feedback.BlogPostId,
                ServiceId = feedback.ServiceId,
                FeedbackType = feedback.FeedbackType,
                Status = feedback.Status,
                CreateDate = feedback.CreateDate,
                AdminResponse = feedback.AdminResponse,
                ResponseDate = feedback.ResponseDate,
                RespondedById = feedback.RespondedById,
                IsPublic = feedback.IsPublic,
                Patient = feedback.Patient != null ? new PatientBasicDTO
                {
                    PatientId = feedback.Patient.PatientId,
                    Name = feedback.Patient.Name,
                    Email = feedback.Patient.Email,
                    Phone = feedback.Patient.Phone
                } : null,
                User = feedback.User != null ? new UserBasicDTO
                {
                    UserId = feedback.User.UserId,
                    Username = feedback.User.Username,
                    FullName = feedback.User.FullName
                } : null,
                RespondedBy = feedback.RespondedBy != null ? new UserBasicDTO
                {
                    UserId = feedback.RespondedBy.UserId,
                    Username = feedback.RespondedBy.Username,
                    FullName = feedback.RespondedBy.FullName
                } : null,
                BlogPost = feedback.BlogPost != null ? new BlogPostBasicDTO
                {
                    BlogPostId = feedback.BlogPost.BlogPostId,
                    Title = feedback.BlogPost.Title,
                    Summary = feedback.BlogPost.Summary,
                    Category = feedback.BlogPost.Category,
                    PublishDate = feedback.BlogPost.PublishDate
                } : null,
                Service = feedback.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = feedback.Service.ServiceId,
                    Name = feedback.Service.Name,
                    Price = feedback.Service.Price
                } : null
            };

            return Ok(feedbackDTO);
        }

        // GET: api/Feedback/Public
        [HttpGet("Public")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetPublicFeedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.BlogPost)
                .Include(f => f.Service)
                .Where(f => f.IsPublic && f.Status == "Responded")
                .OrderByDescending(f => f.CreateDate)
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound("No public feedbacks found");
            }

            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO
            {
                FeedbackId = f.FeedbackId,
                PatientId = f.PatientId,
                UserId = f.UserId,
                Title = f.Title,
                Content = f.Content,
                BlogPostId = f.BlogPostId,
                ServiceId = f.ServiceId,
                FeedbackType = f.FeedbackType,
                Status = f.Status,
                CreateDate = f.CreateDate,
                AdminResponse = f.AdminResponse,
                ResponseDate = f.ResponseDate,
                RespondedById = f.RespondedById,
                IsPublic = f.IsPublic,
                Patient = f.Patient != null ? new PatientBasicDTO
                {
                    PatientId = f.Patient.PatientId,
                    Name = f.Patient.Name
                } : null,
                User = f.User != null ? new UserBasicDTO
                {
                    UserId = f.User.UserId,
                    FullName = f.User.FullName
                } : null,
                BlogPost = f.BlogPost != null ? new BlogPostBasicDTO
                {
                    BlogPostId = f.BlogPost.BlogPostId,
                    Title = f.BlogPost.Title
                } : null,
                Service = f.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = f.Service.ServiceId,
                    Name = f.Service.Name
                } : null
            }).ToList();

            return Ok(feedbackDTOs);
        }

        // GET: api/Feedback/Service/{serviceId}
        [HttpGet("Service/{serviceId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetFeedbacksByService(string serviceId)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.RespondedBy)
                .Where(f => f.ServiceId == serviceId)
                .OrderByDescending(f => f.CreateDate)
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound($"No feedbacks found for service with ID {serviceId}");
            }

            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO
            {
                FeedbackId = f.FeedbackId,
                PatientId = f.PatientId,
                UserId = f.UserId,
                Title = f.Title,
                Content = f.Content,
                BlogPostId = f.BlogPostId,
                ServiceId = f.ServiceId,
                FeedbackType = f.FeedbackType,
                Status = f.Status,
                CreateDate = f.CreateDate,
                AdminResponse = f.AdminResponse,
                ResponseDate = f.ResponseDate,
                RespondedById = f.RespondedById,
                IsPublic = f.IsPublic,
                Patient = f.Patient != null ? new PatientBasicDTO
                {
                    PatientId = f.Patient.PatientId,
                    Name = f.Patient.Name,
                    Email = f.Patient.Email,
                    Phone = f.Patient.Phone
                } : null,
                User = f.User != null ? new UserBasicDTO
                {
                    UserId = f.User.UserId,
                    Username = f.User.Username,
                    FullName = f.User.FullName
                } : null,
                RespondedBy = f.RespondedBy != null ? new UserBasicDTO
                {
                    UserId = f.RespondedBy.UserId,
                    Username = f.RespondedBy.Username,
                    FullName = f.RespondedBy.FullName
                } : null
            }).ToList();

            return Ok(feedbackDTOs);
        }

        // GET: api/Feedback/BlogPost/{blogPostId}
        [HttpGet("BlogPost/{blogPostId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetFeedbacksByBlogPost(string blogPostId)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.RespondedBy)
                .Where(f => f.BlogPostId == blogPostId && (f.IsPublic || f.Status == "Responded"))
                .OrderByDescending(f => f.CreateDate)
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound($"No public feedbacks found for blog post with ID {blogPostId}");
            }

            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO
            {
                FeedbackId = f.FeedbackId,
                PatientId = f.PatientId,
                UserId = f.UserId,
                Title = f.Title,
                Content = f.Content,
                BlogPostId = f.BlogPostId,
                ServiceId = f.ServiceId,
                FeedbackType = f.FeedbackType,
                Status = f.Status,
                CreateDate = f.CreateDate,
                AdminResponse = f.AdminResponse,
                ResponseDate = f.ResponseDate,
                RespondedById = f.RespondedById,
                IsPublic = f.IsPublic,
                Patient = f.Patient != null ? new PatientBasicDTO
                {
                    PatientId = f.Patient.PatientId,
                    Name = f.Patient.Name
                } : null,
                User = f.User != null ? new UserBasicDTO
                {
                    UserId = f.User.UserId,
                    FullName = f.User.FullName
                } : null,
                RespondedBy = f.RespondedBy != null ? new UserBasicDTO
                {
                    UserId = f.RespondedBy.UserId,
                    FullName = f.RespondedBy.FullName
                } : null
            }).ToList();

            return Ok(feedbackDTOs);
        }

        // POST: api/Feedback
        [HttpPost]
        public async Task<ActionResult<FeedbackDTO>> CreateFeedback(FeedbackCreateDTO feedbackDTO)
        {
            // Validate foreign keys if provided
            if (!string.IsNullOrEmpty(feedbackDTO.PatientId))
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == feedbackDTO.PatientId);
                if (!patientExists)
                {
                    return BadRequest($"Patient with ID {feedbackDTO.PatientId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(feedbackDTO.UserId))
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == feedbackDTO.UserId);
                if (!userExists)
                {
                    return BadRequest($"User with ID {feedbackDTO.UserId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(feedbackDTO.BlogPostId))
            {
                var blogPostExists = await _context.BlogPosts.AnyAsync(bp => bp.BlogPostId == feedbackDTO.BlogPostId);
                if (!blogPostExists)
                {
                    return BadRequest($"Blog post with ID {feedbackDTO.BlogPostId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(feedbackDTO.ServiceId))
            {
                var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == feedbackDTO.ServiceId);
                if (!serviceExists)
                {
                    return BadRequest($"Service with ID {feedbackDTO.ServiceId} does not exist");
                }
            }

            var feedback = new Feedback
            {
                FeedbackId = "FB" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = feedbackDTO.PatientId,
                UserId = feedbackDTO.UserId,
                Title = feedbackDTO.Title,
                Content = feedbackDTO.Content,
                BlogPostId = feedbackDTO.BlogPostId,
                ServiceId = feedbackDTO.ServiceId,
                FeedbackType = feedbackDTO.FeedbackType,
                Status = "New",
                CreateDate = DateTime.UtcNow,
                AdminResponse = null,
                ResponseDate = null,
                RespondedById = null,
                IsPublic = feedbackDTO.IsPublic
            };

            _context.Feedbacks.Add(feedback);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the feedback: {ex.Message}");
            }

            var createdFeedbackDTO = new FeedbackDTO
            {
                FeedbackId = feedback.FeedbackId,
                PatientId = feedback.PatientId,
                UserId = feedback.UserId,
                Title = feedback.Title,
                Content = feedback.Content,
                BlogPostId = feedback.BlogPostId,
                ServiceId = feedback.ServiceId,
                FeedbackType = feedback.FeedbackType,
                Status = feedback.Status,
                CreateDate = feedback.CreateDate,
                AdminResponse = feedback.AdminResponse,
                ResponseDate = feedback.ResponseDate,
                RespondedById = feedback.RespondedById,
                IsPublic = feedback.IsPublic
            };

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.FeedbackId }, createdFeedbackDTO);
        }

        // PUT: api/Feedback/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(string id, FeedbackUpdateDTO feedbackDTO)
        {
            if (id != feedbackDTO.FeedbackId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found");
            }

            // Update properties
            feedback.Title = feedbackDTO.Title;
            feedback.Content = feedbackDTO.Content;
            feedback.Status = feedbackDTO.Status;
            feedback.AdminResponse = feedbackDTO.AdminResponse;
            feedback.IsPublic = feedbackDTO.IsPublic;

            // If adding a response, update respondent and date
            if (!string.IsNullOrEmpty(feedbackDTO.AdminResponse) &&
                (string.IsNullOrEmpty(feedback.AdminResponse) || feedback.AdminResponse != feedbackDTO.AdminResponse))
            {
                if (string.IsNullOrEmpty(feedbackDTO.RespondedById))
                {
                    return BadRequest("RespondedById is required when providing an AdminResponse");
                }

                var responderExists = await _context.Users.AnyAsync(u => u.UserId == feedbackDTO.RespondedById);
                if (!responderExists)
                {
                    return BadRequest($"User with ID {feedbackDTO.RespondedById} does not exist");
                }

                feedback.RespondedById = feedbackDTO.RespondedById;
                feedback.ResponseDate = DateTime.UtcNow;
                feedback.Status = "Responded";
            }

            _context.Entry(feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
                {
                    return NotFound($"Feedback with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Feedback/5/Status
        [HttpPatch("{id}/Status")]
        public async Task<IActionResult> UpdateFeedbackStatus(string id, [FromBody] string status)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found");
            }

            // Validate status
            if (status != "New" && status != "Read" && status != "Responded" && status != "Archived")
            {
                return BadRequest("Status must be 'New', 'Read', 'Responded', or 'Archived'");
            }

            feedback.Status = status;
            _context.Entry(feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
                {
                    return NotFound($"Feedback with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Feedback/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found");
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Feedback/Unresponded
        [HttpGet("Unresponded")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetUnrespondedFeedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Include(f => f.User)
                .Include(f => f.BlogPost)
                .Include(f => f.Service)
                .Where(f => f.Status == "New" || f.Status == "Read")
                .OrderBy(f => f.CreateDate)
                .ToListAsync();

            if (feedbacks == null || !feedbacks.Any())
            {
                return NotFound("No unresponded feedbacks found");
            }

            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO
            {
                FeedbackId = f.FeedbackId,
                PatientId = f.PatientId,
                UserId = f.UserId,
                Title = f.Title,
                Content = f.Content,
                BlogPostId = f.BlogPostId,
                ServiceId = f.ServiceId,
                FeedbackType = f.FeedbackType,
                Status = f.Status,
                CreateDate = f.CreateDate,
                IsPublic = f.IsPublic,
                Patient = f.Patient != null ? new PatientBasicDTO
                {
                    PatientId = f.Patient.PatientId,
                    Name = f.Patient.Name,
                    Email = f.Patient.Email,
                    Phone = f.Patient.Phone
                } : null,
                User = f.User != null ? new UserBasicDTO
                {
                    UserId = f.User.UserId,
                    Username = f.User.Username,
                    FullName = f.User.FullName
                } : null,
                BlogPost = f.BlogPost != null ? new BlogPostBasicDTO
                {
                    BlogPostId = f.BlogPost.BlogPostId,
                    Title = f.BlogPost.Title
                } : null,
                Service = f.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = f.Service.ServiceId,
                    Name = f.Service.Name
                } : null
            }).ToList();

            return Ok(feedbackDTOs);
        }
        private bool FeedbackExists(string id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }
    }
}