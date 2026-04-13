using System.ComponentModel.DataAnnotations;

namespace LG_projects.RequestModel.Auth
{
    public class UserRequestMobileModel
    {
        [Required(ErrorMessage = "Parameter :mobile is required")]
        [RegularExpression(@"^\+\d{12}$", ErrorMessage = "Invalid mobile number")]
        public string? mobile { get; set; }
    }
    public class UserRequestOTPModel
    {
        [Required(ErrorMessage = "Parameter : mobile is required")]
        [RegularExpression(@"^\+\d{12}$", ErrorMessage = "Invalid mobile number")]
        public string? mobile { get; set; }

        [Required(ErrorMessage = "Parameter : otpCode is required")]
        public string? otpCode { get; set; }

        [Required(ErrorMessage = "Parameter : otp is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid OTP")]
        public string? otp { get; set; }

        [Required(ErrorMessage = "Parameter : otpVerifyStatus is required")]
        [RegularExpression("^[01]$", ErrorMessage = "Parameter : OTP Verify Status must be 0 or 1")]
        public string? otpVerifyStatus { get; set; }
    }
    public class UserRequestIDModel
    {
        [Required(ErrorMessage = "Parameter :userID is required")]
        public string? userID { get; set; }
    }
    public class UserPasswordRequestModel
    {
        [Required(ErrorMessage = "Parameter :mobile is required")]
        public string? mobile { get; set; }
        [Required(ErrorMessage = "Parameter :password is required")]
        public string? password { get; set; }
    }
    public class UpdateUserProfileRequestModel
    {
        [Required(ErrorMessage = "Parameter : UserId is required")]
        public string? UserId { get; set; }
        [Required(ErrorMessage = "Parameter : NameEn is required")]
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }

        [Required(ErrorMessage = "Parameter : Email is required")]
        [EmailAddress(ErrorMessage = "Parameter : Invalid Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Parameter : Phone is required")]
        [RegularExpression(@"^\+\d{12}$", ErrorMessage = "Invalid mobile number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Parameter : Address is required")]
        public string? Address { get; set; }

        public string? AddressUr { get; set; }
    }
}
