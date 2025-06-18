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
    public class ContentPageController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public ContentPageController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/ContentPage
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContentPageDTO>>> GetContentPages()
        {
            var contentPages = await _context.ContentPages
                .Include(c => c.CreatedBy)
                .Include(c => c.LastModifiedBy)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            if (contentPages == null || !contentPages.Any())
            {
                return NotFound("No content pages found");
            }

            var contentPageDTOs = contentPages.Select(c => new ContentPageDTO
            {
                ContentPageId = c.ContentPageId,
                Title = c.Title,
                Content = c.Content,
                Slug = c.Slug,
                MetaDescription = c.MetaDescription,
                MetaKeywords = c.MetaKeywords,
                PageType = c.PageType,
                Status = c.Status,
                DisplayOrder = c.DisplayOrder,
                CreateDate = c.CreateDate,
                LastModified = c.LastModified,
                CreatedById = c.CreatedById,
                LastModifiedById = c.LastModifiedById,
                CreatedBy = c.CreatedBy != null ? new UserBasicDTO
                {
                    UserId = c.CreatedBy.UserId,
                    Username = c.CreatedBy.Username,
                    FullName = c.CreatedBy.FullName
                } : null,
                LastModifiedBy = c.LastModifiedBy != null ? new UserBasicDTO
                {
                    UserId = c.LastModifiedBy.UserId,
                    Username = c.LastModifiedBy.Username,
                    FullName = c.LastModifiedBy.FullName
                } : null
            }).ToList();

            return Ok(contentPageDTOs);
        }

        // GET: api/ContentPage/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContentPageDTO>> GetContentPage(string id)
        {
            var contentPage = await _context.ContentPages
                .Include(c => c.CreatedBy)
                .Include(c => c.LastModifiedBy)
                .FirstOrDefaultAsync(c => c.ContentPageId == id);

            if (contentPage == null)
            {
                return NotFound($"Content page with ID {id} not found");
            }

            var contentPageDTO = new ContentPageDTO
            {
                ContentPageId = contentPage.ContentPageId,
                Title = contentPage.Title,
                Content = contentPage.Content,
                Slug = contentPage.Slug,
                MetaDescription = contentPage.MetaDescription,
                MetaKeywords = contentPage.MetaKeywords,
                PageType = contentPage.PageType,
                Status = contentPage.Status,
                DisplayOrder = contentPage.DisplayOrder,
                CreateDate = contentPage.CreateDate,
                LastModified = contentPage.LastModified,
                CreatedById = contentPage.CreatedById,
                LastModifiedById = contentPage.LastModifiedById,
                CreatedBy = contentPage.CreatedBy != null ? new UserBasicDTO
                {
                    UserId = contentPage.CreatedBy.UserId,
                    Username = contentPage.CreatedBy.Username,
                    FullName = contentPage.CreatedBy.FullName
                } : null,
                LastModifiedBy = contentPage.LastModifiedBy != null ? new UserBasicDTO
                {
                    UserId = contentPage.LastModifiedBy.UserId,
                    Username = contentPage.LastModifiedBy.Username,
                    FullName = contentPage.LastModifiedBy.FullName
                } : null
            };

            return Ok(contentPageDTO);
        }

        // GET: api/ContentPage/Slug/{slug}
        [HttpGet("Slug/{slug}")]
        public async Task<ActionResult<ContentPageDTO>> GetContentPageBySlug(string slug)
        {
            var contentPage = await _context.ContentPages
                .Include(c => c.CreatedBy)
                .Include(c => c.LastModifiedBy)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.Status == "Published");

            if (contentPage == null)
            {
                return NotFound($"Published content page with slug '{slug}' not found");
            }

            var contentPageDTO = new ContentPageDTO
            {
                ContentPageId = contentPage.ContentPageId,
                Title = contentPage.Title,
                Content = contentPage.Content,
                Slug = contentPage.Slug,
                MetaDescription = contentPage.MetaDescription,
                MetaKeywords = contentPage.MetaKeywords,
                PageType = contentPage.PageType,
                Status = contentPage.Status,
                DisplayOrder = contentPage.DisplayOrder,
                CreateDate = contentPage.CreateDate,
                LastModified = contentPage.LastModified,
                CreatedById = contentPage.CreatedById,
                LastModifiedById = contentPage.LastModifiedById,
                CreatedBy = contentPage.CreatedBy != null ? new UserBasicDTO
                {
                    UserId = contentPage.CreatedBy.UserId,
                    Username = contentPage.CreatedBy.Username,
                    FullName = contentPage.CreatedBy.FullName
                } : null,
                LastModifiedBy = contentPage.LastModifiedBy != null ? new UserBasicDTO
                {
                    UserId = contentPage.LastModifiedBy.UserId,
                    Username = contentPage.LastModifiedBy.Username,
                    FullName = contentPage.LastModifiedBy.FullName
                } : null
            };

            return Ok(contentPageDTO);
        }

        // GET: api/ContentPage/Type/{pageType}
        [HttpGet("Type/{pageType}")]
        public async Task<ActionResult<IEnumerable<ContentPageDTO>>> GetContentPagesByType(string pageType)
        {
            var contentPages = await _context.ContentPages
                .Include(c => c.CreatedBy)
                .Include(c => c.LastModifiedBy)
                .Where(c => c.PageType == pageType && c.Status == "Published")
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            if (contentPages == null || !contentPages.Any())
            {
                return NotFound($"No published content pages found with page type '{pageType}'");
            }

            var contentPageDTOs = contentPages.Select(c => new ContentPageDTO
            {
                ContentPageId = c.ContentPageId,
                Title = c.Title,
                Content = c.Content,
                Slug = c.Slug,
                MetaDescription = c.MetaDescription,
                MetaKeywords = c.MetaKeywords,
                PageType = c.PageType,
                Status = c.Status,
                DisplayOrder = c.DisplayOrder,
                CreateDate = c.CreateDate,
                LastModified = c.LastModified,
                CreatedById = c.CreatedById,
                LastModifiedById = c.LastModifiedById,
                CreatedBy = c.CreatedBy != null ? new UserBasicDTO
                {
                    UserId = c.CreatedBy.UserId,
                    Username = c.CreatedBy.Username,
                    FullName = c.CreatedBy.FullName
                } : null,
                LastModifiedBy = c.LastModifiedBy != null ? new UserBasicDTO
                {
                    UserId = c.LastModifiedBy.UserId,
                    Username = c.LastModifiedBy.Username,
                    FullName = c.LastModifiedBy.FullName
                } : null
            }).ToList();

            return Ok(contentPageDTOs);
        }

        // POST: api/ContentPage
        [HttpPost]
        public async Task<ActionResult<ContentPageDTO>> CreateContentPage(ContentPageCreateDTO contentPageDTO)
        {
            // Validate if creator exists
            if (!string.IsNullOrEmpty(contentPageDTO.CreatedById))
            {
                var creatorExists = await _context.Users.AnyAsync(u => u.UserId == contentPageDTO.CreatedById);
                if (!creatorExists)
                {
                    return BadRequest($"Creator with ID {contentPageDTO.CreatedById} does not exist");
                }
            }

            // Check if slug already exists
            var slugExists = await _context.ContentPages.AnyAsync(c => c.Slug == contentPageDTO.Slug);
            if (slugExists)
            {
                return BadRequest($"Content page with slug '{contentPageDTO.Slug}' already exists");
            }

            var contentPage = new ContentPage
            {
                ContentPageId = "CP" + Guid.NewGuid().ToString().Substring(0, 8),
                Title = contentPageDTO.Title,
                Content = contentPageDTO.Content,
                Slug = contentPageDTO.Slug,
                MetaDescription = contentPageDTO.MetaDescription,
                MetaKeywords = contentPageDTO.MetaKeywords,
                PageType = contentPageDTO.PageType,
                Status = contentPageDTO.Status ?? "Draft",
                DisplayOrder = contentPageDTO.DisplayOrder,
                CreateDate = DateTime.Now,
                LastModified = DateTime.Now,
                CreatedById = contentPageDTO.CreatedById,
                LastModifiedById = contentPageDTO.CreatedById // Initially the same as the creator
            };

            _context.ContentPages.Add(contentPage);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the content page: {ex.Message}");
            }

            var createdContentPageDTO = new ContentPageDTO
            {
                ContentPageId = contentPage.ContentPageId,
                Title = contentPage.Title,
                Content = contentPage.Content,
                Slug = contentPage.Slug,
                MetaDescription = contentPage.MetaDescription,
                MetaKeywords = contentPage.MetaKeywords,
                PageType = contentPage.PageType,
                Status = contentPage.Status,
                DisplayOrder = contentPage.DisplayOrder,
                CreateDate = contentPage.CreateDate,
                LastModified = contentPage.LastModified,
                CreatedById = contentPage.CreatedById,
                LastModifiedById = contentPage.LastModifiedById
            };

            return CreatedAtAction(nameof(GetContentPage), new { id = contentPage.ContentPageId }, createdContentPageDTO);
        }

        // PUT: api/ContentPage/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContentPage(string id, ContentPageUpdateDTO contentPageDTO)
        {
            if (id != contentPageDTO.ContentPageId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var contentPage = await _context.ContentPages.FindAsync(id);
            if (contentPage == null)
            {
                return NotFound($"Content page with ID {id} not found");
            }

            // Validate if modifier exists
            if (!string.IsNullOrEmpty(contentPageDTO.LastModifiedById))
            {
                var modifierExists = await _context.Users.AnyAsync(u => u.UserId == contentPageDTO.LastModifiedById);
                if (!modifierExists)
                {
                    return BadRequest($"Modifier with ID {contentPageDTO.LastModifiedById} does not exist");
                }
            }

            // Check if slug already exists (excluding this content page)
            var slugExists = await _context.ContentPages.AnyAsync(c => c.Slug == contentPageDTO.Slug && c.ContentPageId != id);
            if (slugExists)
            {
                return BadRequest($"Another content page with slug '{contentPageDTO.Slug}' already exists");
            }

            contentPage.Title = contentPageDTO.Title;
            contentPage.Content = contentPageDTO.Content;
            contentPage.Slug = contentPageDTO.Slug;
            contentPage.MetaDescription = contentPageDTO.MetaDescription;
            contentPage.MetaKeywords = contentPageDTO.MetaKeywords;
            contentPage.PageType = contentPageDTO.PageType;
            contentPage.Status = contentPageDTO.Status;
            contentPage.DisplayOrder = contentPageDTO.DisplayOrder;
            contentPage.LastModified = DateTime.Now;
            contentPage.LastModifiedById = contentPageDTO.LastModifiedById;

            _context.Entry(contentPage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContentPageExists(id))
                {
                    return NotFound($"Content page with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ContentPage/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContentPage(string id)
        {
            var contentPage = await _context.ContentPages.FindAsync(id);
            if (contentPage == null)
            {
                return NotFound($"Content page with ID {id} not found");
            }

            _context.ContentPages.Remove(contentPage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContentPageExists(string id)
        {
            return _context.ContentPages.Any(e => e.ContentPageId == id);
        }
    }
}