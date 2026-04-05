using LG_projects.Common.BaseResponse;
using LG_projects.RequestModel.Project;
using LG_projects.ResponseModel.Project;
using Microsoft.AspNetCore.Mvc;

namespace LG_projects.Repository.Project
{
    public interface IProjectRepo
    {
        public Task<ResponseResult<List<ProjectVm>>> GetProjects(); 
        public Task<ResponseResult<List<ProjectVm>>> GetProjectsFilter(string searchType,string search); 
        public Task<ResponseResult<AddFeedbackReponseModel>> AddFeedback([FromBody] AddFeedBackRequestModel param);
        public Task<ResponseResult<List<FeedbackResponseModel>>> GetFeedback([FromBody] GetFeedBackRequestModel param); 
    }
}
