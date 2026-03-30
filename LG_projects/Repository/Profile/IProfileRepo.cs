using LG_projects.Common.BaseResponse;
using LG_projects.ResponseModel.Auth;

namespace LG_projects.Repository.Profile
{
    public interface IProfileRepo
    {
        Task<ResponseResult<UserVm>> GetUserProfileRepo(string userId);
    }
}
