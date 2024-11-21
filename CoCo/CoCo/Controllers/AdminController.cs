using CoCo.Data;
using CoCo.Models;
using CoCo.ViewModel.UserViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CoCo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CocoDbContext _context;
        public AdminController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, CocoDbContext context)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> UserDashboard()
        {
            var users = _userManager.Users.ToList();
            var userRoleViewModels = new List<AdminViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    var userRoleViewModel = new AdminViewModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault() // Assuming the user only has one role
                    };
                    userRoleViewModels.Add(userRoleViewModel);
                }
            }
            return View(userRoleViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            var comment = _context.comment.Where(n => n.userid == userid).ToList();
            var news = _context.news.Where(n => n.UserId == userid).ToList();
            if (comment != null && comment.Any())
            {
                _context.comment.RemoveRange(comment);
            }
            if (news != null && news.Any())
            {
                _context.news.RemoveRange(news);
            }
            var result = await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserDashboard", "Admin");
        }
    }
}
