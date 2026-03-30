using LG_projects.Common.BaseResponse;
using LG_projects.ResponseModel.Auth;

namespace LG_projects.Repository.Auth
{
    public interface IAuthRepo
    {
        Task<ResponseResult<CountryVm>> GetCountriesRepo();
        Task<ResponseResult<LanguageVm>> GetLanguagesRepo();
        Task<ResponseResult<OTPCode>> ValidateUserRepo(string mobile);

        Task<ResponseResult<UserWithToken>> VerifyUserOTPRepo(string mobile , string OTPCode, string otp,string otpVerifyStatus);
    }
}
