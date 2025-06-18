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
    public class BlogPostController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public BlogPostController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/BlogPost
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPostDTO>>> GetBlogPosts()
        {
            var blogPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Feedbacks)
                .ToListAsync();

            if (blogPosts == null || !blogPosts.Any())
            {
                return NotFound("No blog posts found");
            }

            var blogPostDTOs = blogPosts.Select(b => new BlogPostDTO
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Content = b.Content,
                Summary = b.Summary,
                ImageUrl = b.ImageUrl,
                AuthorId = b.AuthorId,
                Category = b.Category,
                Status = b.Status,
                PublishDate = b.PublishDate,
                LastModified = b.LastModified,
                ViewCount = b.ViewCount,
                Tags = b.Tags,
                Author = b.Author != null ? new UserBasicDTO
                {
                    UserId = b.Author.UserId,
                    Username = b.Author.Username,
                    FullName = b.Author.FullName
                } : null,
                FeedbackCount = b.Feedbacks?.Count ?? 0
            }).ToList();

            return Ok(blogPostDTOs);
        }

        // GET: api/BlogPost/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostDTO>> GetBlogPost(string id)
        {
            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Feedbacks)
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blogPost == null)
            {
                return NotFound($"Blog post with ID {id} not found");
            }

            // Increment view count
            blogPost.ViewCount++;
            _context.Entry(blogPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var blogPostDTO = new BlogPostDTO
            {
                BlogPostId = blogPost.BlogPostId,
                Title = blogPost.Title,
                Content = blogPost.Content,
                Summary = blogPost.Summary,
                ImageUrl = blogPost.ImageUrl,
                AuthorId = blogPost.AuthorId,
                Category = blogPost.Category,
                Status = blogPost.Status,
                PublishDate = blogPost.PublishDate,
                LastModified = blogPost.LastModified,
                ViewCount = blogPost.ViewCount,
                Tags = blogPost.Tags,
                Author = blogPost.Author != null ? new UserBasicDTO
                {
                    UserId = blogPost.Author.UserId,
                    Username = blogPost.Author.Username,
                    FullName = blogPost.Author.FullName
                } : null,
                FeedbackCount = blogPost.Feedbacks?.Count ?? 0
            };

            return Ok(blogPostDTO);
        }

        // GET: api/BlogPost/Category/{category}
        [HttpGet("Category/{category}")]
        public async Task<ActionResult<IEnumerable<BlogPostDTO>>> GetBlogPostsByCategory(string category)
        {
            var blogPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Feedbacks)
                .Where(b => b.Category == category && b.Status == "Published")
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();

            if (blogPosts == null || !blogPosts.Any())
            {
                return NotFound($"No published blog posts found in category {category}");
            }

            var blogPostDTOs = blogPosts.Select(b => new BlogPostDTO
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Content = b.Content,
                Summary = b.Summary,
                ImageUrl = b.ImageUrl,
                AuthorId = b.AuthorId,
                Category = b.Category,
                Status = b.Status,
                PublishDate = b.PublishDate,
                LastModified = b.LastModified,
                ViewCount = b.ViewCount,
                Tags = b.Tags,
                Author = b.Author != null ? new UserBasicDTO
                {
                    UserId = b.Author.UserId,
                    Username = b.Author.Username,
                    FullName = b.Author.FullName
                } : null,
                FeedbackCount = b.Feedbacks?.Count ?? 0
            }).ToList();

            return Ok(blogPostDTOs);
        }

        // POST: api/BlogPost
        [HttpPost]
        public async Task<ActionResult<BlogPostDTO>> CreateBlogPost(BlogPostCreateDTO blogPostDTO)
        {
            // Validate if author exists
            if (!string.IsNullOrEmpty(blogPostDTO.AuthorId))
            {
                var authorExists = await _context.Users.AnyAsync(u => u.UserId == blogPostDTO.AuthorId);
                if (!authorExists)
                {
                    return BadRequest($"Author with ID {blogPostDTO.AuthorId} does not exist");
                }
            }

            var blogPost = new BlogPost
            {
                BlogPostId = "BP" + Guid.NewGuid().ToString().Substring(0, 8),
                Title = blogPostDTO.Title,
                Content = blogPostDTO.Content,
                Summary = blogPostDTO.Summary,
                ImageUrl = blogPostDTO.ImageUrl,
                AuthorId = blogPostDTO.AuthorId,
                Category = blogPostDTO.Category,
                Status = blogPostDTO.Status ?? "Draft",
                PublishDate = blogPostDTO.PublishDate ?? DateTime.Now,
                LastModified = DateTime.Now,
                ViewCount = 0,
                Tags = blogPostDTO.Tags
            };

            _context.BlogPosts.Add(blogPost);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the blog post: {ex.Message}");
            }

            var createdBlogPostDTO = new BlogPostDTO
            {
                BlogPostId = blogPost.BlogPostId,
                Title = blogPost.Title,
                Content = blogPost.Content,
                Summary = blogPost.Summary,
                ImageUrl = blogPost.ImageUrl,
                AuthorId = blogPost.AuthorId,
                Category = blogPost.Category,
                Status = blogPost.Status,
                PublishDate = blogPost.PublishDate,
                LastModified = blogPost.LastModified,
                ViewCount = blogPost.ViewCount,
                Tags = blogPost.Tags
            };

            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.BlogPostId }, createdBlogPostDTO);
        }

        // PUT: api/BlogPost/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlogPost(string id, BlogPostUpdateDTO blogPostDTO)
        {
            if (id != blogPostDTO.BlogPostId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound($"Blog post with ID {id} not found");
            }

            blogPost.Title = blogPostDTO.Title;
            blogPost.Content = blogPostDTO.Content;
            blogPost.Summary = blogPostDTO.Summary;
            blogPost.ImageUrl = blogPostDTO.ImageUrl;
            blogPost.Category = blogPostDTO.Category;
            blogPost.Status = blogPostDTO.Status;
            blogPost.PublishDate = blogPostDTO.PublishDate ?? blogPost.PublishDate;
            blogPost.LastModified = DateTime.Now;
            blogPost.Tags = blogPostDTO.Tags;

            _context.Entry(blogPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(id))
                {
                    return NotFound($"Blog post with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/BlogPost/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(string id)
        {
            var blogPost = await _context.BlogPosts
                .Include(b => b.Feedbacks)
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blogPost == null)
            {
                return NotFound($"Blog post with ID {id} not found");
            }

            // Check if there are any feedbacks associated with this blog post
            if (blogPost.Feedbacks != null && blogPost.Feedbacks.Any())
            {
                return BadRequest("Cannot delete blog post with associated feedbacks. Either delete the feedbacks first or archive the blog post instead.");
            }

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/BlogPost/Tag/{tag}
        [HttpGet("Tag/{tag}")]
        public async Task<ActionResult<IEnumerable<BlogPostDTO>>> GetBlogPostsByTag(string tag)
        {
            var blogPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Feedbacks)
                .Where(b => b.Tags.Contains(tag) && b.Status == "Published")
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();

            if (blogPosts == null || !blogPosts.Any())
            {
                return NotFound($"No published blog posts found with tag '{tag}'");
            }

            var blogPostDTOs = blogPosts.Select(b => new BlogPostDTO
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Content = b.Content,
                Summary = b.Summary,
                ImageUrl = b.ImageUrl,
                AuthorId = b.AuthorId,
                Category = b.Category,
                Status = b.Status,
                PublishDate = b.PublishDate,
                LastModified = b.LastModified,
                ViewCount = b.ViewCount,
                Tags = b.Tags,
                Author = b.Author != null ? new UserBasicDTO
                {
                    UserId = b.Author.UserId,
                    Username = b.Author.Username,
                    FullName = b.Author.FullName
                } : null,
                FeedbackCount = b.Feedbacks?.Count ?? 0
            }).ToList();

            return Ok(blogPostDTOs);
        }

        private bool BlogPostExists(string id)
        {
            return _context.BlogPosts.Any(e => e.BlogPostId == id);
        }
    }
}