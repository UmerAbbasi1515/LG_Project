using Dapper;
using LG_projects.Classes;
using LG_projects.Classes.Token;
using LG_projects.Common.BaseResponse;
using LG_projects.DAL;
using LG_projects.ResponseModel.Auth;
using System.Net;
using static Dapper.SqlMapper;
using Settings = LG_projects.Classes.Settings;

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
                            MessageUr = "ممالک کا ڈیٹا ملا", 
                            Data = getCountry
                        };
                    
                }
                else
                {
                    responseResult = new ResponseResult<List<CountryVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "countries data not found",
                        MessageUr = "ممالک کا ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
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
                            MessageUr = "زبان کا ڈیٹا ملا",
                            Data = getLanguage
                        };
                    
                }
                else
                {
                    responseResult = new ResponseResult<List<LanguageVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "language data not found",
                        MessageUr = "زبان کا ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
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
                            MessageUr = "صارف کا ڈیٹا ملا",
                            Data = data
                        };
                    }
                    else {
                        responseResult = new ResponseResult<OTPCodeWithPasswordSetModel>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "unable to generate otp, please try again later",
                            MessageUr = "او ٹی پی بنانے سے قاصر، براہ کرم بعد میں دوبارہ کوشش کریں۔",
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
                        MessageUr = "صارف کا ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
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
                            commonMessage.messageUr = "OTP کی توثیق ناکام ہو گئی۔";
                            responseResult = new ResponseResult<CommonMessageResponseModel>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification failed",
                                MessageUr = "صارف کی تصدیق ناکام ہوگئی",
                                Data = commonMessage
                            };
                        } else {
                            commonMessage.message = "OTP Verification Successfull";
                            commonMessage.messageUr = "OTP کی توثیق کامیاب";
                            responseResult = new ResponseResult<CommonMessageResponseModel>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification successfull",
                                MessageUr = "صارف کی تصدیق کامیاب",
                                Data = commonMessage
                            };
                        }
                    }
                    else {
                         responseResult = new ResponseResult<CommonMessageResponseModel>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "user verification failed",
                             MessageUr = "صارف کی تصدیق ناکام ہوگئی",
                             Data = null
                        };
                    }
                }
                else
                {
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user verification successfull",
                        MessageUr = "صارف کی تصدیق کامیاب",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
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
                // ✅ Log start
                settings.InsertLog($"SetUserPassword START | Mobile: {mobile} | Password: {password} | Query: {query}");
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@mobile", mobile);
                parameters.Add("@password", password);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var rowsAffected = db.Execute(query, parameters);
                if (rowsAffected > 0)
                {
                    // ✅ Log result
                    settings.InsertLog($"SetUserPassword EXECUTED | RowsAffected: {rowsAffected}");
                    commonMessage.message = "Password update successfully";
                    commonMessage.messageUr = "پاس ورڈ کامیابی سے اپ ڈیٹ ہو گیا۔";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Password update successfull",
                        MessageUr = "پاس ورڈ اپ ڈیٹ کامیاب",
                        Data = commonMessage
                    };
                }
                else
                {
                    settings.InsertLog("SetUserPassword FAILED - No rows affected");
                    commonMessage.message = "Password update failed";
                    commonMessage.messageUr = "پاس ورڈ اپ ڈیٹ ناکام ہو گیا۔";
                    responseResult = new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Password update failed",
                        MessageUr = "پاس ورڈ اپ ڈیٹ ناکام ہو گیا۔",
                        Data = commonMessage
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                settings.InsertLog($"SetUserPassword ERROR | {ex.Message}");
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
                        MessageUr = "صارف کا ڈیٹا ملا اور ٹوکن کامیابی سے تیار ہوا۔",
                        Data = data
                    };
                }
                else
                {
                    responseResult = new ResponseResult<UserWithToken>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Token generation failed,Please try again later / contact with support team",
                        MessageUr = " ٹوکن جنریشن ناکام، براہ کرم بعد میں دوبارہ کوشش کریں / سپورٹ ٹیم سے رابطہ کریں۔",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

    }
}
