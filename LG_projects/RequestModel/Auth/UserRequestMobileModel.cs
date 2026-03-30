namespace LG_projects.RequestModel.Auth
{
    public class UserRequestMobileModel
    {
        public string? mobile { get; set; }
    }
    public class UserRequestOTPModel
    {
        public string? mobile { get; set; }
        public string? otpCode { get; set; }
        public string? otp { get; set; }
        public string? otpVerifyStatus { get; set; }
    }
    public class UserRequestIDModel
    {
        public string? userID { get; set; }
    }
}
