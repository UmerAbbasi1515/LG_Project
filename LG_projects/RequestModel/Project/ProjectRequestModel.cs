using System.ComponentModel.DataAnnotations;

namespace LG_projects.RequestModel.Project
{
    public class ProjectSearchModel
    {
        public string? searchType { get; set; }
        public string? search { get; set; }
    }


public class AddFeedBackRequestModel : IValidatableObject
    {
        [Required(ErrorMessage = "Parameter : NameEn is required")]
        public string? NameEn { get; set; }

        [Required(ErrorMessage = "Parameter : NameUr is required")]
        public string? NameUr { get; set; }

        [Required(ErrorMessage = "Parameter : Email is required")]
        [EmailAddress(ErrorMessage = "Parameter : Invalid Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Parameter : Phone is required")]
        [RegularExpression(@"^\+\d{12}$", ErrorMessage = "Invalid mobile number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Parameter : ProjectId is required")]
        public string? ProjectId { get; set; }

        public string? TextMessage { get; set; }

        public IFormFile? VideoFile { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? AudioFile { get; set; }

        // 🔥 Custom Validation (at least ONE required)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(TextMessage) &&
                (VideoFile == null || VideoFile.Length == 0) &&
                (ImageFile == null || ImageFile.Length == 0) &&
                (AudioFile == null || AudioFile.Length == 0))
            {
                yield return new ValidationResult(
                    "At least one of TextMessage, VideoFile, ImageFile, or AudioFile must be provided",
                    new[] { nameof(TextMessage), nameof(VideoFile), nameof(ImageFile), nameof(AudioFile) }
                );
            }
        }
    }
    public class GetFeedBackRequestModel
    {
        [Required(ErrorMessage = "Parameter :ProjectId is required")]
        public string? ProjectId { get; set; }
    }
}
