namespace LG_projects.RequestModel.Project
{
    public class ProjectSearchModel
    {
        public string? searchType { get; set; }
        public string? search { get; set; }
    }

    public class AddFeedBackRequestModel
    {
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProjectId { get; set; }
        public string? ComplaintFeedbackText { get; set; }

        public IFormFile? VideoFile { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? AudioFile { get; set; }
    }
}
