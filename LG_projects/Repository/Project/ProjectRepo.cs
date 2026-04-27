using Dapper;
using LG_projects.Classes;
using LG_projects.Common.BaseResponse;
using LG_projects.DAL;
using LG_projects.Repository.Project;
using LG_projects.RequestModel.Project;
using LG_projects.ResponseModel.Auth;
using LG_projects.ResponseModel.Project;
using Microsoft.AspNetCore.Connections;
using System.Net;
using System.Reflection;

namespace LG_projects.Repository.Auth
{
    public class ProjectRepo : IProjectRepo
    {
        private readonly IDBLogics db;
        private readonly IWebHostEnvironment _env;

        private readonly IConfiguration configuration;
        private readonly Settings settings;
        public ProjectRepo(IDBLogics _db, Settings _settings, IWebHostEnvironment env, IConfiguration configuration)
        {
            db = _db;
            _env = env;
            this.settings = _settings;
            this.configuration = configuration;
        }


        public async Task<ResponseResult<List<ProjectVm>>> GetProjects()
        {

            ResponseResult<List<ProjectVm>> responseResult = new ResponseResult<List<ProjectVm>>();
            List<ProjectVm> getProjects = new List<ProjectVm>();

            try
            {
               string query = "SELECT p.id, p.name_en, p.name_ur, p.description_en, p.description_ur, p.location_en, p.location_ur, p.adp_year, p.suspended, p.created_at, p.committee_members_name_en, p.committee_members_name_ur, h.id AS HalkaId, h.name_en AS HalkaNameEn, h.name_ur AS HalkaNameUr, uc.id AS UCId, uc.name_en AS UCNameEn, uc.name_ur AS UCNameUr, w.id AS WardId, w.name_en AS WardNameEn, w.name_ur AS WardNameUr, pmo.id AS PmoId, pmo.name_en AS PmoNameEn, pmo.name_ur AS PmoNameUr, pl.id AS ProjectLeaderId, pl.name_en AS ProjectLeaderNameEn, pl.name_ur AS ProjectLeaderNameUr FROM Projects p LEFT JOIN Halka h ON p.halka_id = h.id LEFT JOIN UC uc ON p.uc_id = uc.id LEFT JOIN Ward w ON p.ward_id = w.id LEFT JOIN PMO pmo ON p.pmo_id = pmo.id LEFT JOIN ProjectLeader pl ON p.project_leader_id = pl.id";

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                getProjects = db.ExecuteQueryMultipleList<ProjectVm, HalkaVm, UCVm, WardVm, PMOVm, ProjectLeaderVm, ProjectVm>(
                    query,
                    (project, halka, uc, ward, pmo, leader) =>
                    {
                        project.Halka = halka;
                        project.UC = uc;
                        project.Ward = ward;
                        project.PMO = pmo;
                        project.ProjectLeader = leader;
                        return project;
                    },
                    splitOn: "HalkaId,UCId,WardId,PmoId,ProjectLeaderId"
                );

                if (getProjects != null && getProjects.Count > 0)
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Projects data found",
                        MessageUr = "پروجیکٹ کا ڈیٹا ملا",
                        Data = getProjects
                    };
                }
                else
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Projects data not found",
                        MessageUr = "پروجیکٹ کا ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        public async Task<ResponseResult<List<ProjectVm>>> GetProjectsFilter(string searchType, string search)
        {
            // Halka = 1
            // UC = 2
            // Ward = 3
            // PMO = 4
            // ProjectLeader = 5
            // ProjectName = 6
            ResponseResult<List<ProjectVm>> responseResult = new ResponseResult<List<ProjectVm>>();
            List<ProjectVm> getProjects = new List<ProjectVm>();

            try
            {
                int searchTypeInt = 0;
                int.TryParse(searchType, out searchTypeInt);

                string baseQuery = @"SELECT p.id, p.name_en, p.name_ur, p.description_en, p.description_ur, 
                            p.location_en, p.location_ur, p.adp_year, p.suspended, p.created_at, 
                            p.committee_members_name_en, p.committee_members_name_ur, 
                            h.id AS HalkaId, h.name_en AS HalkaNameEn, h.name_ur AS HalkaNameUr, 
                            uc.id AS UCId, uc.name_en AS UCNameEn, uc.name_ur AS UCNameUr, 
                            w.id AS WardId, w.name_en AS WardNameEn, w.name_ur AS WardNameUr, 
                            pmo.id AS PmoId, pmo.name_en AS PmoNameEn, pmo.name_ur AS PmoNameUr, 
                            pl.id AS ProjectLeaderId, pl.name_en AS ProjectLeaderNameEn, pl.name_ur AS ProjectLeaderNameUr 
                            FROM Projects p 
                            LEFT JOIN Halka h ON p.halka_id = h.id 
                            LEFT JOIN UC uc ON p.uc_id = uc.id 
                            LEFT JOIN Ward w ON p.ward_id = w.id 
                            LEFT JOIN PMO pmo ON p.pmo_id = pmo.id 
                            LEFT JOIN ProjectLeader pl ON p.project_leader_id = pl.id";

                // Apply filter based on searchType
                string filterQuery = searchTypeInt switch
                {
                    1 => " WHERE (h.name_en LIKE @search OR h.name_ur LIKE @search)",       // Halka
                    2 => " WHERE (uc.name_en LIKE @search OR uc.name_ur LIKE @search)",     // UC
                    3 => " WHERE (w.name_en LIKE @search OR w.name_ur LIKE @search)",       // Ward
                    4 => " WHERE (pmo.name_en LIKE @search OR pmo.name_ur LIKE @search)",   // PMO
                    5 => " WHERE (pl.name_en LIKE @search OR pl.name_ur LIKE @search)",     // ProjectLeader
                    6 => " WHERE (p.name_en LIKE @search OR p.name_ur LIKE @search)",       // ProjectName
                    _ => ""                                                                  // No filter
                };

                string finalQuery = baseQuery + filterQuery;

                DefaultTypeMap.MatchNamesWithUnderscores = true;

                getProjects = db.ExecuteQueryMultipleList<ProjectVm, HalkaVm, UCVm, WardVm, PMOVm, ProjectLeaderVm, ProjectVm>(
                    finalQuery,
                    (project, halka, uc, ward, pmo, leader) =>
                    {
                        project.Halka = halka;
                        project.UC = uc;
                        project.Ward = ward;
                        project.PMO = pmo;
                        project.ProjectLeader = leader;
                        return project;
                    },
                    parameters: new { search = $"%{search}%" },
                    splitOn: "HalkaId,UCId,WardId,PmoId,ProjectLeaderId"
                );

                if (getProjects != null && getProjects.Count > 0)
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Projects data found",
                        MessageUr = "پروجیکٹ کا ڈیٹا ملا",
                        Data = getProjects
                    };
                }
                else
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Projects data not found",
                        MessageUr = "پروجیکٹ کا ڈیٹا نہیں ملا",
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
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }


        public async Task<ResponseResult<IsFeedbackAddedResponseModel>> IsAddedFeedback(string projectID)
        {

            ResponseResult<IsFeedbackAddedResponseModel> responseResult = new ResponseResult<IsFeedbackAddedResponseModel>();
            IsFeedbackAddedResponseModel isFeedback = new IsFeedbackAddedResponseModel();

            try
            {
                string query = "select isFeedbackAdded from Projects where id = @id";
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@id", projectID);

                DefaultTypeMap.MatchNamesWithUnderscores = true;
                var response = db.ExecuteScalar<dynamic>(query, parameters);

                if (response != null)
                {
                    isFeedback.isfeedbackAdded = response;

                    responseResult = new ResponseResult<IsFeedbackAddedResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "data found",
                        MessageUr = "ڈیٹا ملا",
                        Data = isFeedback
                    };

                }
                else
                {
                    responseResult = new ResponseResult<IsFeedbackAddedResponseModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "data not found",
                        MessageUr = "ڈیٹا نہیں ملا",
                        Data = null
                    };
                }
                return await Task.FromResult(responseResult);
            }
            catch (Exception ex)
            {
                responseResult = new ResponseResult<IsFeedbackAddedResponseModel>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        public async Task<ResponseResult<CommonMessageResponseModel>> AddFeedback(AddFeedBackRequestModel model)
        {
           
            try
            {
                // ── Step 1: Insert Feedback ───────────────────────────────
                string query = @"
        INSERT INTO Feedback 
            (name_en, name_ur, email, phone, whatsApp_phone, TextMessage, projectId, created_at)
        VALUES 
            (@NameEn, @NameUr, @Email, @Phone, @WhatsAppPhone, @TextMessage, @ProjectId, GETDATE());
        SELECT CAST(SCOPE_IDENTITY() AS int);";

                settings.InsertLog($"AddFeedback QUERY(Feedback) | {query} | Params: Name:{model.NameEn}, Phone:{model.Phone}, ProjectId:{model.ProjectId}");

                int feedbackId = 0;

                try
                {
                    settings.InsertLog("AddFeedback EXECUTE(Feedback)");

                    feedbackId = db.ExecuteScalar<int>(query, new
                    {
                        NameEn = model.NameEn,
                        NameUr = model.NameUr,
                        Email = model.Email,
                        Phone = model.Phone,
                        WhatsAppPhone = model.Phone,
                        TextMessage = model.TextMessage,
                        ProjectId = model.ProjectId
                    });

                    settings.InsertLog($"AddFeedback SUCCESS(Feedback) | FeedbackId:{feedbackId}");
                }
                catch (Exception ex)
                {
                    settings.InsertLog($"AddFeedback FAILED(Feedback) | Error:{ex.Message} | Query:{query}");
                    throw;
                }

                // ── Step 2: Save Files ───────────────────────────────
                
                var imagePath = await FileHelper.SaveFile(model.ImageFile, "image", _env);
                var videoPath = await FileHelper.SaveFile(model.VideoFile, "video", _env);
                var audioPath = await FileHelper.SaveFile(model.AudioFile, "audio", _env);

                settings.InsertLog($"AddFeedback FILE SAVE DONE | Image:{imagePath} | Video:{videoPath} | Audio:{audioPath}");

                // ── Step 3: Insert Media ───────────────────────────────
                string mediaQuery = @"
        INSERT INTO FeedbackMedia (feedbackId, FilePath, MediaType, created_at)
        VALUES (@FeedbackId, @FilePath, @MediaType, GETDATE());";

                // IMAGE
                if (!string.IsNullOrEmpty(imagePath))
                {
                    settings.InsertLog($"AddFeedback QUERY(Media-Image) | {mediaQuery} | FeedbackId:{feedbackId}");

                    try
                    {
                        
                        db.Execute(mediaQuery, new
                        {
                            FeedbackId = feedbackId,
                            FilePath = imagePath,
                            MediaType = "image"
                        });

                        settings.InsertLog("AddFeedback SUCCESS(Media-Image)");
                    }
                    catch (Exception ex)
                    {
                        settings.InsertLog($"AddFeedback FAILED(Media-Image) | Error:{ex.Message}");
                        throw;
                    }
                }

                // VIDEO
                if (!string.IsNullOrEmpty(videoPath))
                {
                    settings.InsertLog($"AddFeedback QUERY(Media-Video) | {mediaQuery} | FeedbackId:{feedbackId}");

                    try
                    {
                      
                        db.Execute(mediaQuery, new
                        {
                            FeedbackId = feedbackId,
                            FilePath = videoPath,
                            MediaType = "video"
                        });

                        settings.InsertLog("AddFeedback SUCCESS(Media-Video)");
                    }
                    catch (Exception ex)
                    {
                        settings.InsertLog($"AddFeedback FAILED(Media-Video) | Error:{ex.Message}");
                        throw;
                    }
                }

                // AUDIO
                if (!string.IsNullOrEmpty(audioPath))
                {
                    
                    try
                    {
                        settings.InsertLog("AddFeedback EXECUTE(Media-Audio)");

                        db.Execute(mediaQuery, new
                        {
                            FeedbackId = feedbackId,
                            FilePath = audioPath,
                            MediaType = "audio"
                        });

                        settings.InsertLog("AddFeedback SUCCESS(Media-Audio)");
                    }
                    catch (Exception ex)
                    {
                        settings.InsertLog($"AddFeedback FAILED(Media-Audio) | Error:{ex.Message}");
                        throw;
                    }
                }

                // ── Step 4: Update Project ───────────────────────────────
                string updateQuery = "UPDATE Projects SET isFeedbackAdded = @isFeedbackAdded WHERE id = @id";

                settings.InsertLog($"AddFeedback QUERY(UpdateProject) | {updateQuery} | ProjectId:{model.ProjectId}");

                int res = 0;

                try
                {
                   
                    var updateParameters = new Dapper.DynamicParameters();
                    updateParameters.Add("@isFeedbackAdded", 1);
                    updateParameters.Add("@id", model.ProjectId);

                    res = db.Execute(updateQuery, updateParameters);

                    settings.InsertLog($"AddFeedback SUCCESS(UpdateProject) | Rows:{res}");
                }
                catch (Exception ex)
                {
                    settings.InsertLog($"AddFeedback FAILED(UpdateProject) | Error:{ex.Message}");
                    throw;
                }

                // ── Final Response ───────────────────────────────
                if (res > 0)
                {
                    settings.InsertLog("AddFeedback END SUCCESS");

                    return new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = 200,
                        Message = "Success",
                        Data = new CommonMessageResponseModel
                        {
                            message = "Feedback added successfully",
                            messageUr = "تاثرات کامیابی کے ساتھ شامل ہو گئے۔"
                        }
                    };
                }
                else
                {
                    settings.InsertLog("AddFeedback END FAILED");

                    return new ResponseResult<CommonMessageResponseModel>
                    {
                        StatusCode = 200,
                        Message = "Failed",
                        Data = new CommonMessageResponseModel
                        {
                            message = "Feedback added failed",
                            messageUr = "تاثرات شامل کرنا ناکام ہو گیا۔"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                settings.InsertLog($"AddFeedback CATCH ERROR | ProjectId:{model.ProjectId} | Error:{ex.Message}");

                return new ResponseResult<CommonMessageResponseModel>
                {
                    StatusCode = 500,
                    Message = "Internal Server Error (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی (" + ex.Message + ")",
                };
            }
        }
        //public async Task<ResponseResult<CommonMessageResponseModel>> AddFeedback(AddFeedBackRequestModel model)
        //{
        //    try
        //    {
        //        // ── Step 1: Insert Feedback row ───────────────────────────────
        //        string query = @"
        //        INSERT INTO Feedback 
        //            (name_en, name_ur, email, phone, whatsApp_phone, TextMessage, projectId, created_at)
        //        VALUES 
        //            (@NameEn, @NameUr, @Email, @Phone, @WhatsAppPhone, @TextMessage, @ProjectId, GETDATE());
        //        SELECT CAST(SCOPE_IDENTITY() AS int);";
        //        //                                  ↑
        //       
        //        var feedbackId = db.ExecuteScalar<int>(query, new
        //        {
        //            NameEn = model.NameEn,
        //            NameUr = model.NameUr,
        //            Email = model.Email,
        //            Phone = model.Phone,
        //            WhatsAppPhone = model.Phone,
        //            TextMessage = model.TextMessage,
        //            ProjectId = model.ProjectId
        //        });
        //        // ── Step 2: Save files to disk, get relative paths ────────────
        //        // FileHelper returns "" if file is null — safe to call always
        //        var imagePath = await FileHelper.SaveFile(model.ImageFile, "image", _env);
        //        var videoPath = await FileHelper.SaveFile(model.VideoFile, "video", _env);
        //        var audioPath = await FileHelper.SaveFile(model.AudioFile, "audio", _env);

        //        // ── Step 3: Insert each file into FeedbackMedia table ─────────
        //        string mediaQuery = @"
        //        INSERT INTO FeedbackMedia (feedbackId, FilePath, MediaType, created_at)
        //        VALUES (@FeedbackId, @FilePath, @MediaType, GETDATE());";

        //        // Only insert if file was actually uploaded and saved
        //        if (!string.IsNullOrEmpty(imagePath))
        //        {

        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = imagePath,   // e.g. /media/images/abc.jpg
        //                MediaType = "image"
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(videoPath))
        //        {
        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = videoPath,   // e.g. /media/videos/abc.mp4
        //                MediaType = "video"
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(audioPath))
        //        {
        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = audioPath,   // e.g. /media/audios/abc.mp3
        //                MediaType = "audio"
        //            });
        //        }

        //        string updateQuery = "UPDATE Projects SET isFeedbackAdded = @isFeedbackAdded WHERE id = @id";
        //        var updateParameters = new Dapper.DynamicParameters();
        //        updateParameters.Add("@isFeedbackAdded", 1);
        //        updateParameters.Add("@id", model.ProjectId);
        //      var res =  db.Execute(updateQuery, updateParameters);
        //        if (res > 0) {
        //            return new ResponseResult<CommonMessageResponseModel>
        //            {
        //                StatusCode = 200,
        //                Message = "Success",
        //                Data = new CommonMessageResponseModel
        //                {
        //                    message = "Feedback added successfully",
        //                    messageUr = "تاثرات کامیابی کے ساتھ شامل ہو گئے۔"
        //                }
        //            };
        //        } else {
        //            return new ResponseResult<CommonMessageResponseModel>
        //            {
        //                StatusCode = 200,
        //                Message = "Failed",
        //                Data = new CommonMessageResponseModel
        //                {
        //                    message = "Feedback added failed",
        //                    messageUr = "تاثرات شامل کرنا ناکام ہو گیا۔"
        //                }
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseResult<CommonMessageResponseModel>
        //        {
        //            StatusCode = 500,
        //            Message = "Internal Server Error" + " (" + ex.Message + ")",
        //            MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
        //        };
        //    }
        //}

        //List of feedback
        public async Task<ResponseResult<List<FeedbackResponseModel>>> GetFeedbackList(GetFeedBackRequestModel model)
        {
            try
            {
                // ── Step 1: Build BaseUrl for preview links ───────────────────
                // This reads your app URL from appsettings.json
                // In appsettings.json add:
                //   "AppSettings": { "BaseUrl": "https://localhost:7000" }
                //
                // OR pass IConfiguration into your repo constructor and read it.
                // For now it reads from _config which you already have injected.
                string baseUrl = configuration["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";

                // ── Step 2: Query Feedback + Media joined ─────────────────────
                string query = @"
            SELECT 
                f.id,
                f.name_en,
                f.name_ur,
                f.email,
                f.phone,
                f.whatsApp_phone,
                f.TextMessage,
                f.projectId,
                fm.FilePath,
                fm.MediaType
            FROM Feedback f
            LEFT JOIN FeedbackMedia fm ON f.id = fm.feedbackId
            WHERE f.projectId = @ProjectId
            ORDER BY f.id DESC";

                var result = db.ExecuteList<dynamic>(query, new { ProjectId = model.ProjectId });

                if (result == null)
                {
                    return new ResponseResult<List<FeedbackResponseModel>>
                    {
                        StatusCode = 200,
                        Message = "Feedback data not found",
                        MessageUr = "تاثرات کا ڈیٹا نہیں ملا",
                        Data = null
                    };
                }

                // ── Step 3: Group rows by feedback id ────────────────────────
                // Because LEFT JOIN returns one row PER media file,
                // one feedback with 3 files = 3 rows in result.
                // We group them back into one object with a Media list.
                var feedbackDict = new Dictionary<int, FeedbackResponseModel>();

                foreach (var item in result)
                {
                    int id = (int)item.id;

                    // First time we see this feedback id — create the object
                    if (!feedbackDict.ContainsKey(id))
                    {
                        feedbackDict[id] = new FeedbackResponseModel
                        {
                            Id = id,
                            NameEn = item.name_en,
                            NameUr = item.name_ur,
                            Email = item.email,
                            Phone = item.phone,
                            WhatsAppPhone = item.phone,
                            TextMessage = item.TextMessage,
                            ProjectId = item.projectId,
                            Media = new List<MediaModel>()
                        };
                    }

                    // If this row has a media file, add it to the Media list
                    if (item.FilePath != null)
                    {
                        string filePath = (string)item.FilePath;
                        // filePath from DB = "/media/images/abc.jpg"
                        // PreviewUrl      = "https://localhost:7000/media/images/abc.jpg"
                        // ↑ User can paste this in browser and see/play the file

                        feedbackDict[id].Media.Add(new MediaModel
                        {
                            FilePath = filePath,
                            MediaType = item.MediaType,
                            PreviewUrl = baseUrl + filePath
                            // Examples:
                            //   https://localhost:7000/media/images/abc.jpg  → browser shows image
                            //   https://localhost:7000/media/videos/abc.mp4  → browser plays video
                            //   https://localhost:7000/media/audios/abc.mp3  → browser plays audio
                        });
                    }
                }

                return new ResponseResult<List<FeedbackResponseModel>>
                {
                    StatusCode = 200,
                    Message = "Feedback data found",
                    MessageUr = "تاثرات کا ڈیٹا ملا",
                    Data = feedbackDict.Values.ToList()
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<FeedbackResponseModel>>
                {
                    StatusCode = 500,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
            }
        }

        public async Task<ResponseResult<FeedbackResponseModel>> GetFeedback(GetFeedBackRequestModel model)
        {
            try
            {
                string baseUrl = configuration["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";

                string query = @" 
        SELECT 
            f.id,
            f.name_en,
            f.name_ur,
            f.email,
            f.phone,
            f.whatsApp_phone,
            f.TextMessage,
            f.projectId,
            fm.FilePath,
            fm.MediaType
        FROM Feedback f
        LEFT JOIN FeedbackMedia fm ON f.id = fm.feedbackId
        WHERE f.projectId = @ProjectId
        AND f.id = (
            SELECT TOP 1 id 
            FROM Feedback 
            WHERE projectId = @ProjectId 
            ORDER BY id DESC
        )";

                var result = db.ExecuteList<dynamic>(query, new { ProjectId = model.ProjectId });

                if (result == null || !result.Any())
                {
                    return new ResponseResult<FeedbackResponseModel>
                    {
                        StatusCode = 404,
                        Message = "Feedback data found",
                        MessageUr = "تاثرات کا ڈیٹا ملا",
                        Data = null
                    };
                }

                // Create single object
                var first = result.First();
                int id = Convert.ToInt32(first.id);
                var feedback = new FeedbackResponseModel
                {
                    Id = id,
                    NameEn = first.name_en,
                    NameUr = first.name_ur,
                    Email = first.email,
                    Phone = first.phone,
                    WhatsAppPhone = first.whatsApp_phone,
                    TextMessage = first.TextMessage,
                    ProjectId = first.projectId,
                    Media = new List<MediaModel>()
                };

                // Add media
                foreach (var item in result)
                {
                    if (item.FilePath != null)
                    {
                        string filePath = (string)item.FilePath;

                        feedback.Media.Add(new MediaModel
                        {
                            FilePath = filePath,
                            MediaType = item.MediaType,
                            PreviewUrl = baseUrl + filePath
                        });
                    }
                }

                return new ResponseResult<FeedbackResponseModel>
                {
                    StatusCode = 200,
                    Message = "Feedback data found",
                    MessageUr = "تاثرات کا ڈیٹا نہیں ملا",
                    Data = feedback
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult<FeedbackResponseModel>
                {
                    StatusCode = 500,
                    Message = "Internal Server Error" + " (" + ex.Message + ")",
                    MessageUr = "اندرونی سرور کی خرابی۔" + " (" + ex.Message + ")",
                    Data = null
                };
            }
        }
    }
}