using Microsoft.AspNetCore.Identity;

namespace CoCo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool? ForgotPassword { get; set; } = false;
    }
}
