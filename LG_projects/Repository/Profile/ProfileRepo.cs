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
