using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthSystem.Controllers
{
    public class CommentController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var comment = new Comment
                {
                    PostId = postId,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UserId = user.Id
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "BlogPost", new { id = postId });
            }
            // This will hit if the model state is invalid and no comment will be posted
            return RedirectToAction("Details", "BlogPost", new { id = postId });
        }
    }
}
