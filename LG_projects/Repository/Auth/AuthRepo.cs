using Dapper;
using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.DAL;
using LG_projects.ResponseModel.Auth;
using System.Net;

namespace LG_projects.Repository.Auth
{
    public class AuthRepo : IAuthRepo
    {
        private readonly TokenService tokenService;
        private readonly IDBLogics db;
        private readonly Settings settings;
        public AuthRepo(IDBLogics _db, Settings _settings, TokenService _tokenService, IConfiguration _config)
        {
            tokenService = _tokenService;
            settings = _settings;
            db = _db;
        }
        public async Task<ResponseResult<List<CountryVm>>> GetCountriesRepo()
        {

            ResponseResult<List<CountryVm>> responseResult = new ResponseResult<List<CountryVm>>();
            List<CountryVm> getCountry = new List<CountryVm>();

            try
            {
                string query = "select * from Countries where Active = 1";
                var parameters = new Dapper.DynamicParameters();

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteList<CountryVm>(query, parameters);

                if (response != null)
                {
                    getCountry = response.ToList();

                        responseResult = new ResponseResult<List<CountryVm>>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "countries data found",
                            Data = getCountry
                        };
                    
                }
                else
                {
                    responseResult = new ResponseResult<List<CountryVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "countries data not found",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<List<CountryVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }
        public async Task<ResponseResult<List<LanguageVm>>> GetLanguagesRepo()
        {

            ResponseResult<List<LanguageVm>> responseResult = new ResponseResult<List<LanguageVm>>();
            List<LanguageVm> getLanguage = new List<LanguageVm>();

            try
            {
                string query = "select * from Language where Active = 1";
                var parameters = new Dapper.DynamicParameters();

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteList<LanguageVm> (query, parameters);

                if (response != null)
                {
                    getLanguage = response.ToList();

                        responseResult = new ResponseResult<List<LanguageVm>>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "language data found",
                            Data = getLanguage
                        };
                    
                }
                else
                {
                    responseResult = new ResponseResult<List<LanguageVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "language data not found",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<List<LanguageVm>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }
        // In this function will check only user exist or not if exist than return token & OTP that we also save in Users table but will not return user once user will verify otp than will return user data
        public async Task<ResponseResult<OTPCodeWithPasswordSetModel>> ValidateUserRepo(string mobile)
        {

            ResponseResult<OTPCodeWithPasswordSetModel> responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>();
            UserVm getUser = new UserVm();

            try
            {
                string query = "select * from Users where phone = @mobile";
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@mobile", mobile);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteSingle<UserVm>(query, parameters);

                if (response != null)
                {
                    getUser = response;

                    var otpCode = settings.GenerateAlphaNumericOtp(20);
                    string updateQuery = "UPDATE Users SET otpCode = @otpCode WHERE phone = @mobile";
                    
                    var updateParameters = new Dapper.DynamicParameters();
                    updateParameters.Add("@otpCode", otpCode);
                    updateParameters.Add("@mobile", mobile);
                    var rowsAffected = db.Execute(updateQuery, updateParameters);

                    if (rowsAffected > 0)
                    {
                        var data = new OTPCodeWithPasswordSetModel
                        {
                            otpCode = otpCode,
                            isPasswordSet = getUser.IsPassword
                        };
                        responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "user data found",
                            Data = data
                        };
                    }
                    else {
                        responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "unable to generate otp, please try again later",
                            Data = null
                        };
                    }
                }
                else
                {
                    responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
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
                responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" +" ("+ ex.Message+")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }
        // Wil return token and user Data if user verify otp code also
        public async Task<ResponseResult<CommonMessageResponseModel>> VerifyUserOTPRepo(string mobile,string OTPCode, string otp, string otpVerifyStatus)
        {

            ResponseResult<CommonMessageResponseModel> responseResult = new ResponseResult<CommonMessageResponseModel>();
            CommonMessageResponseModel commonMessage = new CommonMessageResponseModel();

            try
            {
                string query = "select * from Users where phone = @mobile AND otpCode = @OTPCode";
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@mobile", mobile);
                parameters.Add("@otpCode", OTPCode);
                parameters.Add("@otp", otp);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteSingle<UserVm>(query, parameters);
                int isverified = int.Parse(otpVerifyStatus);
                if (response != null)
                {
                    string insertQuery = @"
                    INSERT INTO UserOtps 
                    (Mobile, Otp, OtpCode, IsVerified, CreatedAt, VerifiedAt)
                    VALUES 
                    (@Mobile, @Otp, @OtpCode, @IsVerified, GETDATE(), GETDATE())";
                   
                    var insertParameters = new Dapper.DynamicParameters();
                    insertParameters.Add("@Mobile", mobile);
                    insertParameters.Add("@Otp", otp);
                    insertParameters.Add("@OtpCode", OTPCode);
                    insertParameters.Add("@IsVerified", isverified);

                    var rowsAffected = db.Execute(insertQuery, insertParameters );

                    if (rowsAffected > 0)
                    {
                        var dataWithToken = new UserWithToken();
                        if (isverified == 0) {
                            commonMessage.message = "OTP Verification Failed";
                            responseResult = new ResponseResult<CommonMessageResponseModel>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification failed",
                                Data = commonMessage
                            };
                        } else {
                            commonMessage.message = "OTP Verification Successfull";
                            responseResult = new ResponseResult<CommonMessageResponseModel>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification successfull",
                                Data = commonMessage
                            };
                        }
                    }
                    else {
                         responseResult = new ResponseResult<CommonMessageResponseModel>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Token generation failed,Please try again later / contact with support team",
                            Data = null
                        };
                    }
                }
                else
                {
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Token generation failed,Please try again later / contact with support team",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<CommonMessageResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        public async Task<ResponseResult<CommonMessageResponseModel>> SetUserPassword(string mobile, string password)
        {

            ResponseResult<CommonMessageResponseModel> responseResult = new ResponseResult<CommonMessageResponseModel>();
            CommonMessageResponseModel commonMessage = new CommonMessageResponseModel();

            try
            {
                string query = "UPDATE Users SET Password = @Password, IsPassword = 1 WHERE Phone = @Mobile;";
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@mobile", mobile);
                parameters.Add("@password", password);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var rowsAffected = db.Execute(query, parameters);
                if (rowsAffected > 0)
                {
                    commonMessage.message = "Password update successfully";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Password update successfull",
                        Data = commonMessage
                    };
                }
                else
                {
                    commonMessage.message = "Password update failed";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Password update failed",
                        Data = commonMessage
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<CommonMessageResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        public async Task<ResponseResult<UserWithToken>> VerifyUserPassword(string mobile, string password)
        {

            ResponseResult<UserWithToken> responseResult = new ResponseResult<UserWithToken>();
            UserVm getUser = new UserVm();

            try
            {
                string query = @"SELECT *  FROM Users  WHERE Phone = @Mobile AND Password = @Password";

                var parameters = new DynamicParameters();
                parameters.Add("@mobile", mobile);
                parameters.Add("@password", password);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteSingle<UserVm>(query, parameters);
                if (response != null)
                {
                    getUser = response;
                    var generatedToken = tokenService.BuildToken(getUser);
                    UserWithToken data = new UserWithToken {
                        Token = generatedToken,
                        User = getUser
                    };
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user data found and token generated successully",
                        Data = data
                    };
                }
                else
                {
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Token generation failed,Please try again later / contact with support team",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
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
