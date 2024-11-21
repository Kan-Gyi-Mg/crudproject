using CoCo.Data;
using CoCo.Models;
using CoCo.ViewModel.UserViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoCo.Controllers
{
    public class UserController : Controller
    {
        private readonly EmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CocoDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(RoleManager<IdentityRole> roleManager, EmailService emailService, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, CocoDbContext context)
        {
            _emailService = emailService;
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        //create user
        [HttpGet]
        public async Task<IActionResult> UserCreate() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCreate(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var roleresult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to assign role.");
                    return View(model);
                }
                var otp = GenerateOTP();
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("RegisterUserId", user.Id);

                await _emailService.SendEmailAsync(model.Email, "OTP Code", $"Your OTP code is {otp}");
                return RedirectToAction("VerifyOTP");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        //forgot password
        [HttpGet]
        public async Task<IActionResult> UserForgotPassword() => View();
        [HttpPost]
        public async Task<IActionResult> UserForgotPassword(String Email)
        {
            if (Email == null)
            {
                return View(Email);
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                return RedirectToAction("UserCreate", "User");
            }
            user.ForgotPassword = true;
            var otp = GenerateOTP();
            HttpContext.Session.SetString("ForgotOTP", otp);
            HttpContext.Session.SetString("ForgotUserId", user.Id);
            await _emailService.SendEmailAsync(Email, "OTP Code", $"Your OTP code is {otp}");
            return RedirectToAction("VerifyForgotOTP");
        }
        
        //login user
        [HttpGet]
        public async Task<IActionResult> UserLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser user = null;

            if (model.EmailOrUserName.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                user = await _userManager.FindByEmailAsync(model.EmailOrUserName);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.EmailOrUserName);
            }

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    Console.WriteLine("++++++++++++++++++++" + string.Join(", ", roles));

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (roles.Contains("User"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        //logout user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        //otp session
        [HttpGet]
        public IActionResult VerifyOTP()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(OTPViewModel model)
        {
            var storedOtp = HttpContext.Session.GetString("OTP");
            var userId = HttpContext.Session.GetString("RegisterUserId");

            if (model.OTP == storedOtp && !string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("UserLogin", "User");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid OTP.");
            return View(model);
        }
        //forgototp
        [HttpGet]
        public async Task<IActionResult> VerifyForgotOTP()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyForgotOTP(OTPViewModel model)
        {
            var storedOtp = HttpContext.Session.GetString("ForgotOTP");
            var userId = HttpContext.Session.GetString("ForgotUserId");

            if (model.OTP == storedOtp && !string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    return RedirectToAction("UserResetPassword", "User");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid OTP.");
            return View(model);
        }
        //Reset Password
        [HttpGet]
        public async Task<IActionResult> UserResetPassword(String UserId)
        {
            return View(UserId);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserResetPassword(UserResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.ForgotPassword == false)
            {

                var result = await _userManager.RemovePasswordAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                result = await _userManager.AddPasswordAsync(user, model.Password); // Set new password
                if (result.Succeeded)
                {
                    user.ForgotPassword = false;
                    _context.user.Update(user);
                    _context.SaveChanges();
                    return RedirectToAction("UserLogin", "User");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        //resendOtp function
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOTP(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }

            var otp = GenerateOTP();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("RegisterUserId", user.Id);

            await _emailService.SendEmailAsync(user.Email, "Your OTP Code", $"Your new OTP code is {otp}");

            return RedirectToAction("VerifyOTP");
        }
    }
}
