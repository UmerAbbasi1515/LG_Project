using Dapper;
using LG_projects.Classes;
using LG_projects.Common.BaseResponse;
using LG_projects.DAL;
using LG_projects.Repository.Project;
using LG_projects.RequestModel.Project;
using LG_projects.ResponseModel.Project;
using System.Net;

namespace LG_projects.Repository.Auth
{
    public class ProjectRepo : IProjectRepo
    {
        private readonly IDBLogics db;
        private readonly IWebHostEnvironment _env;

        private readonly IConfiguration configuration;
        public ProjectRepo(IDBLogics _db, IWebHostEnvironment env, IConfiguration configuration)
        {
            db = _db;
            _env = env;
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
                        Data = getProjects
                    };
                }
                else
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "no record found",
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
                        Data = getProjects
                    };
                }
                else
                {
                    responseResult = new ResponseResult<List<ProjectVm>>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "No record found",
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
                    Data = null
                };
                return await Task.FromResult(responseResult);
            }
        }

        //public async Task<ResponseResult<AddFeedbackReponseModel>> AddFeedback(AddFeedBackRequestModel model)
        //{
        //    try
        //    {
        //        // 1. Insert Feedback
        //        string query = @"
        //            INSERT INTO Feedback (name_en,name_ur,email,phone,whatsApp_phone,TextMessage,projectId,created_at)
        //            VALUES (@NameEn,@NameUr,@Email,@Phone,@Phone,@TextMessage,@ProjectId,GETDATE());
        //            SELECT CAST(SCOPE_IDENTITY() as int);
        //            ";

        //        var feedbackId = db.ExecuteScalar<int>(query, new
        //        {
        //            NameEn = model.NameEn,
        //            NameUr = model.NameUr,
        //            Email = model.Email,
        //            Phone = model.Phone,
        //            TextMessage = model.TextMessage,
        //            ProjectId = model.ProjectId
        //        });

        //        // 2. Save files & get paths

        //        var imagePath = await FileHelper.SaveFile(model.ImageFile, "image", _env);
        //        var videoPath = await FileHelper.SaveFile(model.VideoFile, "video", _env);
        //        var audioPath = await FileHelper.SaveFile(model.AudioFile, "audio", _env);

        //        // 3. Insert into FeedbackMedia
        //        string mediaQuery = @"
        //            INSERT INTO FeedbackMedia (feedbackId, FilePath, MediaType, created_at)
        //            VALUES (@FeedbackId, @FilePath, @MediaType, GETDATE());
        //            ";

        //        if (!string.IsNullOrEmpty(imagePath))
        //        {
        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = imagePath,
        //                MediaType = "image"
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(videoPath))
        //        {
        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = videoPath,
        //                MediaType = "video"
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(audioPath))
        //        {
        //            db.Execute(mediaQuery, new
        //            {
        //                FeedbackId = feedbackId,
        //                FilePath = audioPath,
        //                MediaType = "audio"
        //            });
        //        }

        //        return new ResponseResult<AddFeedbackReponseModel>
        //        {
        //            StatusCode = 200,
        //            Message = "Success",
        //            Data = new AddFeedbackReponseModel
        //            {
        //                message = "Feedback added successfully"
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseResult<AddFeedbackReponseModel>
        //        {
        //            StatusCode = 500,
        //            Message = ex.Message
        //        };
        //    }
        //}

        //public async Task<ResponseResult<List<FeedbackResponseModel>>> GetFeedback(GetFeedBackRequestModel model)
        //{
        //    try
        //    {
        //        string query = @"
        //SELECT 
        //    f.id, 
        //    f.name_en,
        //    f.name_ur,
        //    f.email,
        //    f.phone,
        //    f.TextMessage,
        //    f.projectId,
        //    fm.FilePath, 
        //    fm.MediaType
        //FROM Feedback f
        //LEFT JOIN FeedbackMedia fm ON f.id = fm.feedbackId
        //WHERE f.projectId = @ProjectId
        //ORDER BY f.id DESC";

        //        // Use your DAL method
        //        var parameters = new { ProjectId = model.ProjectId };
        //        var result = db.ExecuteList<dynamic>(query, parameters);

        //        // Dictionary to group media by feedback id
        //        var feedbackDict = new Dictionary<int, FeedbackResponseModel>();

        //        foreach (var item in result)
        //        {
        //            int id = (int)item.id;

        //            if (!feedbackDict.ContainsKey(id))
        //            {
        //                feedbackDict[id] = new FeedbackResponseModel
        //                {
        //                    Id = id,
        //                    NameEn = item.name_en,
        //                    NameUr = item.name_ur,
        //                    Email = item.email,
        //                    Phone = item.phone,
        //                    TextMessage = item.TextMessage,
        //                    ProjectId = item.projectId,
        //                    Media = new List<MediaModel>()
        //                };
        //            }

        //            if (item.FilePath != null)
        //            {
        //                feedbackDict[id].Media.Add(new MediaModel
        //                {
        //                    FilePath = item.FilePath,
        //                    MediaType = item.MediaType
        //                });
        //            }
        //        }

        //        return new ResponseResult<List<FeedbackResponseModel>>
        //        {
        //            StatusCode = 200,
        //            Message = "feeback data found",
        //            Data = feedbackDict.Values.ToList()
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseResult<List<FeedbackResponseModel>>
        //        {
        //            StatusCode = 500,
        //            Message = ex.Message,
        //            Data = null
        //        };
        //    }
        //}

        // ============================================================
        // FILE 4: Repository/FeedbackRepo.cs
        // Changes:
        //   ✅ Fixed @Phone → @WhatsAppPhone bug
        //   ✅ Added WhatsAppPhone to parameters
        //   ✅ GET: builds full PreviewUrl = BaseUrl + FilePath
        // ============================================================

        public async Task<ResponseResult<AddFeedbackReponseModel>> AddFeedback(AddFeedBackRequestModel model)
        {
            try
            {
                // ── Step 1: Insert Feedback row ───────────────────────────────
                string query = @"
            INSERT INTO Feedback 
                (name_en, name_ur, email, phone, whatsApp_phone, TextMessage, projectId, created_at)
            VALUES 
                (@NameEn, @NameUr, @Email, @Phone, @WhatsAppPhone, @TextMessage, @ProjectId, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS int);";
                //                                  ↑
                //   FIXED: was @Phone before — now correctly @WhatsAppPhone

                var feedbackId = db.ExecuteScalar<int>(query, new
                {
                    NameEn = model.NameEn,
                    NameUr = model.NameUr,
                    Email = model.Email,
                    Phone = model.Phone,
                    WhatsAppPhone = model.Phone,
                    TextMessage = model.TextMessage,
                    ProjectId = model.ProjectId
                });

                // ── Step 2: Save files to disk, get relative paths ────────────
                // FileHelper returns "" if file is null — safe to call always
                var imagePath = await FileHelper.SaveFile(model.ImageFile, "image", _env);
                var videoPath = await FileHelper.SaveFile(model.VideoFile, "video", _env);
                var audioPath = await FileHelper.SaveFile(model.AudioFile, "audio", _env);

                // ── Step 3: Insert each file into FeedbackMedia table ─────────
                string mediaQuery = @"
            INSERT INTO FeedbackMedia (feedbackId, FilePath, MediaType, created_at)
            VALUES (@FeedbackId, @FilePath, @MediaType, GETDATE());";

                // Only insert if file was actually uploaded and saved
                if (!string.IsNullOrEmpty(imagePath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = imagePath,   // e.g. /media/images/abc.jpg
                        MediaType = "image"
                    });
                }

                if (!string.IsNullOrEmpty(videoPath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = videoPath,   // e.g. /media/videos/abc.mp4
                        MediaType = "video"
                    });
                }

                if (!string.IsNullOrEmpty(audioPath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = audioPath,   // e.g. /media/audios/abc.mp3
                        MediaType = "audio"
                    });
                }

                return new ResponseResult<AddFeedbackReponseModel>
                {
                    StatusCode = 200,
                    Message = "Success",
                    Data = new AddFeedbackReponseModel
                    {
                        message = "Feedback added successfully"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult<AddFeedbackReponseModel>
                {
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }


        public async Task<ResponseResult<List<FeedbackResponseModel>>> GetFeedback(GetFeedBackRequestModel model)
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
                    Data = feedbackDict.Values.ToList()
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<FeedbackResponseModel>>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

    }
}