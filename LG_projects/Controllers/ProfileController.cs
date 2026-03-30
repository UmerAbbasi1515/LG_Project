using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Repository.Profile;
using LG_projects.ResponseModel.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace LG_projects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepo profileRepo;
        private readonly ITokenService tokenService;

        public ProfileController(IProfileRepo _profileRepo,ITokenService _tokenService)
        {
            profileRepo = _profileRepo;
            tokenService = _tokenService; 
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetUserProfile")]
        public async Task<ResponseResult<UserVm>> GetUserProfile()
        {

            ResponseResult<UserVm> responseResult = new ResponseResult<UserVm>();
            UserVm getUser = new UserVm();

            try
            {
                var bearToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                if (tokenService.IsTokenValid(bearToken)) {
                    var list = tokenService.DecodeJWTToken(bearToken);
                    string userId = list[0].ToString();
                    string name = list[1].ToString();
                    string phone = list[2].ToString();
                    if (userId != null || userId != "")
                    {
                        responseResult = await profileRepo.GetUserProfileRepo(userId?.ToString()??"");
                    }
                    else {
                        responseResult = new ResponseResult<UserVm>
                        {
                            StatusCode = (int)HttpStatusCode.Unauthorized,
                            Message = "unauthorized",
                            Data = null
                        };
                    }
                    
                } else {
                    responseResult = new ResponseResult<UserVm>
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "unauthorized",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                responseResult = new ResponseResult<UserVm>
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
