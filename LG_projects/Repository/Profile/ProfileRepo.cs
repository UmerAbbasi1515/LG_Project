using Dapper;
using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.Common.ListConvertor;
using LG_projects.DAL;
using LG_projects.RequestModel.Auth;
using LG_projects.ResponseModel.Auth;
using System.Net;
using System.Reflection;
using static Dapper.SqlMapper;
using Settings = LG_projects.Classes.Settings;

namespace LG_projects.Repository.Profile
{
    public class ProfileRepo : IProfileRepo
    {
        private readonly IDBLogics db;
        private readonly Settings settings;
        public ProfileRepo(IDBLogics _db, Settings _settings)
        {
            db = _db;
            this.settings = _settings;
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
                        Message = "user profile data found",
                        MessageUr = "صارف کا پروفائل ڈیٹا ملا",
                        Data = getUser
                    };
                }
                else
                {
                    responseResult = new ResponseResult<UserVm>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user profile data not found",
                        MessageUr = "صارف کا پروفائل ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        public async Task<ResponseResult<CommonMessageResponseModel>> UpdateUserProfileRepo(UpdateUserProfileRequestModel model)
        {

            ResponseResult<CommonMessageResponseModel> responseResult = new ResponseResult<CommonMessageResponseModel>();

            try
            {
                string query = @"
                    UPDATE Users
                    SET 
                        name_en = @NameEn,
                        name_ur = @NameUr,
                        email = @Email,
                        phone = @Phone,
                        address = @Address,
                        addressur = @AddressUr,
                        updated_at = GETDATE()
                    WHERE id = @UserId";
                settings. InsertLog($"UpdateUserProfile START | UserId: {model.UserId} | NameEn: {model.NameEn} | Email: {model.Email} | Phone: {model.Phone} | Query: {query}");

                var parameters = new DynamicParameters();
                    parameters.Add("@UserId", model.UserId);
                    parameters.Add("@NameEn", model.NameEn);
                    parameters.Add("@NameUr", model.NameUr);
                    parameters.Add("@Email", model.Email);
                    parameters.Add("@Phone", model.Phone);
                    parameters.Add("@Address", model.Address);
                    parameters.Add("@AddressUr", model.AddressUr);
                    var rowsAffected =  db.Execute(query, parameters);

                CommonMessageResponseModel profileUpdated = new CommonMessageResponseModel();
                if (rowsAffected > 0) {

                    settings.InsertLog($"UpdateUserProfile EXECUTED | UserId: {model.UserId} | RowsAffected: {rowsAffected}");

                    profileUpdated.message = "Profile updated successfully";
                    profileUpdated.messageUr = "پروفائل کامیابی کے ساتھ اپ ڈیٹ ہو گیا۔";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Success",
                        MessageUr = "کامیابی",
                        Data = profileUpdated
                    };
                }
                else
                {
                    settings.InsertLog($"UpdateUserProfile FAILED | UserId: {model.UserId} | No rows affected");

                    profileUpdated.message = "Profile updated failed";
                    profileUpdated.messageUr = "پروفائل کو اپ ڈیٹ کرنا ناکام ہو گیا۔";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Failed",
                        MessageUr = "ناکام",
                        Data = null
                    };
                }
                    return await Task.FromResult(responseResult);    // want to change here
                }
            catch (Exception ex)
            {
                settings.InsertLog($"UpdateUserProfile ERROR | UserId: {model.UserId} | Exception: {ex.Message}");

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

        

    }
}
