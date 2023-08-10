using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthSystem.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<BlogPostController> _logger;

        public BlogPostController(AuthDbContext context, ILogger<BlogPostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Calls index which provides an entire list of all the posts and allows the user to click on one which
        /// can then bring the user to the detail screen
        /// </summary>
        
        // GET: BlogPost
        public IActionResult Index(string? id)
        {
            if (id == null)
            {
                var blogPosts = _context.Posts
                    .Include(p => p.User)
                    .ToList();
                return View(blogPosts);
            }
            else
            {
                var blogPosts = _context.Posts
                    .Include(p => p.User)
                    .Where(p => p.UserId == id)
                    .ToList();
                return View(blogPosts);
            }
            
        }

        public IActionResult MyIndex()
        {
            var blogPosts = _context.Posts.Where(p => p.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).ToList();
            return View(blogPosts);
        }

        // GET: BlogPost/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var blogPost = _context.Posts
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefault(p => p.PostId == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        /// <summary>
        /// These following functions work with Create.cshtml to allow users to post
        /// to the database. These will only be allowed if the user signed in to an
        /// authorized account
        /// </summary>
        
        // GET: BlogPost/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: BlogPost/Create
        [HttpPost]
        [Authorize]
        public IActionResult Create(BlogPost blogPost)
        {
            var tmp = blogPost;
            tmp.CreatedAt = DateTime.Now;
            tmp.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // This should never be null because the user has to be signed in to see it due to the [Authorize] tag

            _context.Posts.Add(tmp);
            _context.SaveChanges();
            _logger.LogInformation("Blog post created");
            return RedirectToAction(nameof(Index));

            //return View(tmp);

        }
        /// <summary>
        /// The following 2 functions are used to edit already existing posts. It first checks to make sure that the user trying to edit
        /// the post is the same user who created the post to ensure that nobody can edit a post just by knowing the url to the post.
        /// </summary>
        /// <param name="id"></param>
        // GET: BlogPost/Edit/id
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id)
        {
            // Error checking
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _context.Posts.Find(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            if(blogPost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(blogPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int id, [Bind("PostId,PostTitle,Content,CreatedAt,UserId")] BlogPost blogPost)
        {
            //Error Checking
            if (id != blogPost.PostId)
            {
                return NotFound();
            }

            var originalBlogPost = _context.Posts.Find(id);
            if (originalBlogPost == null)
            {
                return NotFound();
            }

            if (originalBlogPost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) // If this is true the user trying to edit the post is not the user who created the post
            {
                _logger.LogInformation(User.FindFirstValue(ClaimTypes.NameIdentifier));
                _logger.LogInformation(blogPost.UserId);
                return Forbid();
            }

            originalBlogPost.UpdatedAt = DateTime.Now;
            originalBlogPost.PostTitle = blogPost.PostTitle;
            originalBlogPost.Content = blogPost.Content;


            _context.SaveChanges();
            _logger.LogInformation("Post edited successfully");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var blogPost = await _context.Posts.FindAsync(id);
            // Error Checking
            if (blogPost == null)
            {
                return NotFound();
            }

            // Confirming th user wanting to delete the post is the creator of the post
            if (blogPost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(blogPost);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = await _context.Posts.FindAsync(id);
            // Error Checking
            if (blogPost == null)
            {
                return NotFound();
            }

            // Confirming the user of the post
            if (blogPost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            _context.Posts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




    }
}
