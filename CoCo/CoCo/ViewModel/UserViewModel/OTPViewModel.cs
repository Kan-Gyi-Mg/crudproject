using System.ComponentModel.DataAnnotations;

namespace CoCo.ViewModel.UserViewModel
{
    public class OTPViewModel
    {
        [Required]
        public string OTP { get; set; }
    }
}
