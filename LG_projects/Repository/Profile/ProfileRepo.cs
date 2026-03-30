using Dapper;
using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Common.ListConvertor;
using LG_projects.DAL;
using LG_projects.ResponseModel.Auth;
using System.Net;

namespace LG_projects.Repository.Profile
{
    public class ProfileRepo : IProfileRepo
    {
        private readonly IDBLogics db;
        public ProfileRepo(IDBLogics _db)
        {
            db = _db;
        }

        public async Task<ResponseResult<UserVm>> GetUserProfileRepo(string userId)
        {

            ResponseResult<UserVm> responseResult = new ResponseResult<UserVm>();
            UserVm getUser = new UserVm();

            try
            {
                string query = "select * from Users where id =" + userId;
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@userID", userId);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                // want to change here
                var response = db.ExecuteSingle<UserVm>(query);

                if (response != null)
                {
                    getUser = response;
                    responseResult = new ResponseResult<UserVm>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user data found",
                        Data = getUser
                    };
                }
                else
                {
                    responseResult = new ResponseResult<UserVm>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user data not found",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
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
