using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Repository.Project;
using LG_projects.RequestModel.Project;
using LG_projects.ResponseModel.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace LG_projects.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepo projectRepo;
        private readonly TokenService tokenService;
        private readonly IConfiguration configuration;
        public ProjectController(IProjectRepo _projectRepo, TokenService _tokenService,IConfiguration _configuration)
        {
            projectRepo = _projectRepo;
            tokenService = _tokenService;
            configuration = _configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetProjects")]
        public async Task<ResponseResult<List<ProjectVm>>> GetProjects()
        {
            ResponseResult<List<ProjectVm>> responseResult = new ResponseResult<List<ProjectVm>>();
            try
            {
                string bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                string jwtKey = configuration["Jwt:Key"]!.ToString();
                string jwtIssuer = configuration["Jwt:Issuer"]!.ToString();
                bool isValidToken = tokenService.IsTokenValid(bearerToken);

                if (isValidToken)
                {
                    responseResult = await projectRepo.GetProjects();
                }
                else {
                    responseResult = new ResponseResult<List<ProjectVm>>
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
                responseResult = new ResponseResult<List<ProjectVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetProjectsFilter")]
        public async Task<ResponseResult<List<ProjectVm>>> GetProjectsFilter([FromBody] ProjectSearchModel param)
        {
            ResponseResult<List<ProjectVm>> responseResult = new ResponseResult<List<ProjectVm>>();
            try
            {
                string bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                string jwtKey = configuration["Jwt:Key"]!.ToString();
                string jwtIssuer = configuration["Jwt:Issuer"]!.ToString();
                bool isValidToken = tokenService.IsTokenValid(bearerToken);

                if (isValidToken)
                {
                    responseResult = await projectRepo.GetProjectsFilter(param.searchType??"",param.search??"");
                }
                else
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
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
                responseResult = new ResponseResult<List<ProjectVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddProjectFeedback")]
        public async Task<ResponseResult<AddFeedbackReponseModel>> AddProjectFeedback([FromForm] AddFeedBackRequestModel param)
        {
            ResponseResult<AddFeedbackReponseModel> responseResult = new ResponseResult<AddFeedbackReponseModel>();
            try
            {
                string bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                string jwtKey = configuration["Jwt:Key"]!.ToString();
                string jwtIssuer = configuration["Jwt:Issuer"]!.ToString();
                bool isValidToken = tokenService.IsTokenValid(bearerToken);

                if (isValidToken)
                {
                    responseResult = await projectRepo.AddFeedback(param);
                }
                else
                {
                    responseResult = new ResponseResult<AddFeedbackReponseModel>
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
                responseResult = new ResponseResult<AddFeedbackReponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetProjectFeedback")]
        public async Task<ResponseResult<List<FeedbackResponseModel>>> GetProjectFeedback([FromBody] GetFeedBackRequestModel param)
        {
            ResponseResult<List<FeedbackResponseModel>> responseResult = new ResponseResult<List<FeedbackResponseModel>>();
            try
            {
                string bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

                string jwtKey = configuration["Jwt:Key"]!.ToString();
                string jwtIssuer = configuration["Jwt:Issuer"]!.ToString();
                bool isValidToken = tokenService.IsTokenValid(bearerToken);

                if (isValidToken)
                {
                    responseResult = await projectRepo.GetFeedback(param);
                }
                else
                {
                    responseResult = new ResponseResult<List<FeedbackResponseModel>>
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
                responseResult = new ResponseResult<List<FeedbackResponseModel>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }




    }
}
