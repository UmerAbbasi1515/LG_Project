using Dapper;
using LG_projects.Classes;
using LG_projects.Common.BaseResponse;
using LG_projects.DAL;
using LG_projects.Repository.Project;
using LG_projects.RequestModel.Project;
using LG_projects.ResponseModel.Auth;
using LG_projects.ResponseModel.Project;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using System.Reflection;

namespace LG_projects.Repository.Auth
{
    public class ProjectRepo : IProjectRepo
    {
        private readonly IDBLogics db;
        private readonly IWebHostEnvironment _env;
        public ProjectRepo(IDBLogics _db, IWebHostEnvironment env)
        {
            db = _db;
            _env = env;
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

        public async Task<ResponseResult<AddFeedbackReponseModel>> AddFeedback(AddFeedBackRequestModel model)
        {
            try
            {
                // 1. Insert Feedback
                string query = @"
                    INSERT INTO Feedback (name_en,name_ur,email,phone,whatsApp_phone,TextMessage,projectId,created_at)
                    VALUES (@NameEn,@NameUr,@Email,@Phone,@Phone,@TextMessage,@ProjectId,GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() as int);
                    ";

                var feedbackId = db.ExecuteScalar<int>(query, new
                {
                    NameEn = model.NameEn,
                    NameUr = model.NameUr,
                    Email = model.Email,
                    Phone = model.Phone,
                    ComplaintFeedbackText = model.ComplaintFeedbackText,
                    ProjectId = model.ProjectId
                });

                // 2. Save files & get paths
                var imagePath = await FileHelper.SaveFile(model.ImageFile, "image", _env);
                var videoPath = await FileHelper.SaveFile(model.VideoFile, "video", _env);
                var audioPath = await FileHelper.SaveFile(model.AudioFile, "audio", _env);

                // 3. Insert into FeedbackMedia
                string mediaQuery = @"
                    INSERT INTO FeedbackMedia (feedbackId, FilePath, MediaType, created_at)
                    VALUES (@FeedbackId, @FilePath, @MediaType, GETDATE());
                    ";

                if (!string.IsNullOrEmpty(imagePath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = imagePath,
                        MediaType = "image"
                    });
                }

                if (!string.IsNullOrEmpty(videoPath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = videoPath,
                        MediaType = "video"
                    });
                }

                if (!string.IsNullOrEmpty(audioPath))
                {
                    db.Execute(mediaQuery, new
                    {
                        FeedbackId = feedbackId,
                        FilePath = audioPath,
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
    }
}