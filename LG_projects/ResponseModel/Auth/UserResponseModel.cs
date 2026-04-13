using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LG_projects.ResponseModel.Auth
{
    public class LanguageVm
    {
        public long Id { get; set; }
        public string? NameEn { get; set; }
        public bool Active { get; set; }
    }
    public class CountryVm
    {
        public int Id { get; set; }
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
        public string? DialingCode { get; set; }
        public string? Flag { get; set; }
        public bool Active { get; set; }
    }
    public class UserVm
    {
        public int id { get; set; }
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? addressUr { get; set; }
        public string? IsPassword { get; set; }
    }
    public class CommonMessageResponseModel
    {
        public string? message { get; set; }
    }
    
    public class OTPCodeWithPasswordSetModel
    {
     public string? otpCode { get; set; }
     public string? isPasswordSet { get; set; }
    }
    public class UserWithToken
    {
        public UserVm? User { get; set; }
        public string? Token { get; set; }
    }

}
