using CoCo.Data;
using CoCo.Models;
using CoCo.ViewModel.NewsViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CoCo.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CocoDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(RoleManager<IdentityRole> roleManager, EmailService emailService, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, CocoDbContext context)
        {
            _emailService = emailService;
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ShowAllNews()
        {
            var news = await _context.news.ToListAsync();
            return View(news);
        }
        [HttpGet]
        public async Task<IActionResult> AddNews()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("UserLogin", "User");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddNews(NewsModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            var addnews = new NewsModel
            {
                NewsTitle = model.NewsTitle,
                NewsBody = model.NewsBody,
                Description = model.Description,
                UserId = user.Id
            };
            await _context.news.AddAsync(addnews);
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowAllNews", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> EditNews(int newsid)
        {
            var newlist = await _context.news.FirstOrDefaultAsync(n => n.NewsId == newsid);
            return View(newlist);
        }
        [HttpPost]
        public async Task<IActionResult> EditNews(NewsModel model)
        {
            var editnew = await _context.news.FirstOrDefaultAsync(n => n.NewsId == model.NewsId);
            editnew.NewsTitle = model.NewsTitle;
            editnew.Description = model.Description;
            editnew.NewsBody = model.NewsBody;
            _context.news.Update(editnew);
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowAllNews", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteNews(int newsid)
        {
            var newlist = await _context.news.FirstOrDefaultAsync(n => n.NewsId == newsid);
            _context.news.Remove(newlist);
            await _context.SaveChangesAsync();
            return RedirectToAction("ShowAllNews", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> SingleNews(int newsid)
        {
            var onenew = await _context.news.FirstOrDefaultAsync(n => n.NewsId == newsid);
            var comment = await _context.comment.Where(com => com.newsid == onenew.NewsId).ToListAsync();
            var newmodel = new NewsViewModel
            {
                news = onenew,
                Comments = comment
            };
            return View(newmodel);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(NewsViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("UserLogin", "User");
            }
            var onenew = await _context.news.FirstOrDefaultAsync(n => n.NewsId == model.news.NewsId);
            var comment = new CommentModel
            {
                CommentBody = model.cbody,
                newsid = onenew.NewsId,
                userid = (await _userManager.GetUserAsync(User))?.Id,
                username = (await _userManager.GetUserAsync(User))?.UserName,
            };
            _context.comment.Add(comment);
            _context.SaveChanges();
            return RedirectToAction("SingleNews", "Home", new { newsid = onenew.NewsId });
        }
    }
}