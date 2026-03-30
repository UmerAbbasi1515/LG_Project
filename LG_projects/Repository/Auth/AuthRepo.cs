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
                string query = "select * from Language";
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
        public async Task<ResponseResult<OTPCode>> ValidateUserRepo(string mobile)
        {

            ResponseResult<OTPCode> responseResult = new ResponseResult<OTPCode>();
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
                        var data = new OTPCode
                        {
                            otpCode = otpCode
                        };
                        responseResult = new ResponseResult<OTPCode>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "user found",
                            Data = data
                        };
                    }
                    else {
                        responseResult = new ResponseResult<OTPCode>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "unable to generate otp, please try again later",
                            Data = null
                        };
                    }
                }
                else
                {
                    responseResult = new ResponseResult<OTPCode>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "user not found",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<OTPCode>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" +" ("+ ex.Message+")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        // Wil return token and user Data if user verify otp code also
        public async Task<ResponseResult<UserWithToken>> VerifyUserOTPRepo(string mobile,string OTPCode, string otp, string otpVerifyStatus)
        {

            ResponseResult<UserWithToken> responseResult = new ResponseResult<UserWithToken>();
            UserVm getUser = new UserVm();

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
                        getUser = response;
                        string generatedToken = "";
                        var dataWithToken = new UserWithToken();
                        if (isverified == 0) {
                             generatedToken = "";
                            dataWithToken = new UserWithToken
                            {
                                User = null,
                                Token = generatedToken
                            };
                            responseResult = new ResponseResult<UserWithToken>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification failed",
                                Data = dataWithToken
                            };
                        } else {
                             generatedToken = tokenService.BuildToken(getUser);
                            dataWithToken = new UserWithToken
                            {
                                User = getUser,
                                Token = generatedToken
                            };
                            responseResult = new ResponseResult<UserWithToken>
                            {
                                StatusCode = (int)HttpStatusCode.OK,
                                Message = "user verification successfull",
                                Data = dataWithToken
                            };
                        }
                           
                         
                      
                    }
                    else {
                         responseResult = new ResponseResult<UserWithToken>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Token generation failed,Please try again later / contact with support team",
                            Data = null
                        };
                    }
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
