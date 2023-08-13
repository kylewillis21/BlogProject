using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace AuthSystem.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(UserManager<ApplicationUser> userManager, AuthDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<UserProfileController> logger)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid file.");
                return RedirectToAction("Index");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if(!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(string.Empty, "Invlaid file format.");
                return RedirectToAction("Index");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePicturePath = fileName;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

        // Profile Page
        public IActionResult Profile(string id)
        {
            _logger.LogInformation("User id {userId} was passed", id);
            if (id == null)
            {
                return NotFound();
            }

            var user = _userManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Found user {User}", user.Nickname);


            return View(user);
        }

        // Edit Page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model, IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

            ModelState.Remove("ProfilePicturePath");
            ModelState.Remove("profilePicture");

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.UserName = model.Email;
                user.NormalizedUserName = model.Email.ToUpper(); 

                // Handling Profile picture upload
                if (profilePicture != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();
                    
                    if (allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogInformation("Valid picture format");
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            _logger.LogInformation("Directory does not exist creating one now on {uploadsFolder}", uploadsFolder);
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(stream);
                        }

                        user.ProfilePicturePath = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid file format.");
                        return View(model);
                    }
                }


                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    object routeValues = new { id = user.Id };
                    return RedirectToAction("Profile", "UserProfile", routeValues);
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        var errorMessage = error.ErrorMessage;
                        _logger.LogInformation(errorMessage);
                    }
                }
            }
            _logger.LogInformation("Unsuccessful Update of Information");
            return View("EditProfile", model);
        }
    }
}
