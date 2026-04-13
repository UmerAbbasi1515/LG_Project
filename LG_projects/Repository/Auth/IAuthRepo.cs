using LG_projects.Common.BaseResponse;
using LG_projects.ResponseModel.Auth;

namespace LG_projects.Repository.Auth
{
    public interface IAuthRepo
    {
        Task<ResponseResult<List<CountryVm>>> GetCountriesRepo();
        Task<ResponseResult<List<LanguageVm>>> GetLanguagesRepo();
        Task<ResponseResult<OTPCodeWithPasswordSetModel>> ValidateUserRepo(string mobile);
        Task<ResponseResult<CommonMessageResponseModel>> VerifyUserOTPRepo(string mobile , string OTPCode, string otp,string otpVerifyStatus); 
        Task<ResponseResult<CommonMessageResponseModel>> SetUserPassword(string mobile, string password);
        Task<ResponseResult<UserWithToken>> VerifyUserPassword(string mobile, string password);
    }
}
