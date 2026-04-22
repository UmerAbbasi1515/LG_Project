using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Common.ListConvertor;
using LG_projects.DAL;
using LG_projects.Repository.Auth;
using LG_projects.RequestModel.Auth;
using LG_projects.ResponseModel.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using System.Net;

namespace LG_projects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo authRepo;

        public AuthController(IAuthRepo _authRepo)
        {
            authRepo = _authRepo;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("GetLanguages")]
        public async Task<ResponseResult<List<LanguageVm>>> GetLanguages()
        {

            ResponseResult<List<LanguageVm>> responseResult = new ResponseResult<List<LanguageVm>>();

            try
            {
                
                responseResult = await authRepo.GetLanguagesRepo();
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<List<LanguageVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetCountries")]
        public async Task<ResponseResult<List<CountryVm>>> GetCountries()
        {

            ResponseResult<List<CountryVm>> responseResult = new ResponseResult<List<CountryVm>>();

            try
            {

                responseResult = await authRepo.GetCountriesRepo();
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<List<CountryVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ValidateUser")]
        public async Task<ResponseResult<OTPCodeWithPasswordSetModel>> ValidateUser([FromBody] UserRequestMobileModel param)
        {
            ResponseResult<OTPCodeWithPasswordSetModel> responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>();
            UserVm getUser = new UserVm();
            
            try
            {
                string mobile = param.mobile ?? "";
                responseResult = await authRepo.ValidateUserRepo(mobile.ToString());
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("VerifyUserOTP")]
        public async Task<ResponseResult<CommonMessageResponseModel>> VerifyUserOTP([FromBody] UserRequestOTPModel param)
        {

            ResponseResult<CommonMessageResponseModel> responseResult = new ResponseResult<CommonMessageResponseModel>();
            UserVm getUser = new UserVm();

            try
            {
                responseResult = await authRepo.VerifyUserOTPRepo(param.mobile?.ToString() ?? "", param.otpCode?.ToString() ?? "", param.otp?.ToString() ?? "", param.otpVerifyStatus);
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<CommonMessageResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("SetUserPassword")]
        public async Task<ResponseResult<CommonMessageResponseModel>> SetUserPassword([FromBody] UserPasswordRequestModel param)
        {

            ResponseResult<CommonMessageResponseModel> responseResult = new ResponseResult<CommonMessageResponseModel>();
            CommonMessageResponseModel setPassword = new CommonMessageResponseModel();

            try
            {
                responseResult = await authRepo.SetUserPassword(param.mobile?.ToString() ?? "",param.password?.ToString());
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<CommonMessageResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("VerifyUserPassword")]
        public async Task<ResponseResult<UserWithToken>> VerifyUserPassword([FromBody] UserPasswordRequestModel param)
        {

            ResponseResult<UserWithToken> responseResult = new ResponseResult<UserWithToken>();
            UserVm getUser = new UserVm();

            try
            {
                responseResult = await authRepo.VerifyUserPassword(param.mobile?.ToString() ?? "", param?.password?.ToString() ??"");
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<UserWithToken>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

    }
}
