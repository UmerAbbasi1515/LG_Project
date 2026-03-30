using LG_projects.Common.BaseResponse;
using LG_projects.ResponseModel.Project;

namespace LG_projects.Repository.Project
{
    public interface IProjectRepo
    {
        public Task<ResponseResult<List<ProjectVm>>> GetProjects(); 
        public Task<ResponseResult<List<ProjectVm>>> GetProjectsFilter(string searchType,string search);
    }
}
