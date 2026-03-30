using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Common.CrossSiteScriptingValidation;
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
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ValidateUser")]
        public async Task<ResponseResult<OTPCode>> ValidateUser([FromBody] UserRequestMobileModel param)
        {

            ResponseResult<OTPCode> responseResult = new ResponseResult<OTPCode>();
            UserVm getUser = new UserVm();

            try
            {
                string mobile = param.mobile ??"";
                if (CrossSiteScriptingValidation.IsDangerousParameter(mobile.ToString(), out _))
                {
                    responseResult = new ResponseResult<OTPCode>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Invalid Request.",
                        Data = null
                    }; 
                    return await Task.FromResult(responseResult);
                }
                responseResult = await authRepo.ValidateUserRepo(mobile.ToString());
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<OTPCode>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("VerifyUserOTP")]
        public async Task<ResponseResult<UserWithToken>> VerifyUserOTP([FromBody] UserRequestOTPModel param)
        {

            ResponseResult<UserWithToken> responseResult = new ResponseResult<UserWithToken>();
            UserVm getUser = new UserVm();

            try
            {
                if (CrossSiteScriptingValidation.IsDangerousParameter(param.mobile?.ToString()??"", out _))
                {
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Invalid Request.",
                        Data = null
                    }; return await Task.FromResult(responseResult);
                }
                if (CrossSiteScriptingValidation.IsDangerousParameter(param.otpCode?.ToString()??"", out _))
                {
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Invalid Request.",
                        Data = null
                    }; return await Task.FromResult(responseResult);
                }
                if (CrossSiteScriptingValidation.IsDangerousParameter(param.otp?.ToString() ?? "", out _))
                {
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Invalid Request.",
                        Data = null
                    }; return await Task.FromResult(responseResult);
                }
                responseResult = await authRepo.VerifyUserOTPRepo(param.mobile?.ToString() ?? "", param.otpCode?.ToString() ?? "", param.otp?.ToString() ?? "", param.otpVerifyStatus);
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<UserWithToken>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

    }
}
