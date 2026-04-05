using System.Data;

namespace LG_projects.Classes
{
    public static class FileHelper
    {

        public static async Task<string> SaveFile(IFormFile? file, string type, IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return "";

            // ✅ Validate type
            type = type.ToLower();
            if (type != "video" && type != "audio" && type != "image")
                throw new Exception("Invalid file type");

            // ✅ File name
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            // ✅ Base path (outside project)
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "LGPMediaFiles");
            basePath = Path.GetFullPath(basePath);

            // ✅ Folder mapping
            string folderPath = type switch
            {
                "video" => Path.Combine(basePath, "videos"),
                "audio" => Path.Combine(basePath, "audio"),
                "image" => Path.Combine(basePath, "images"),
                _ => throw new Exception("Invalid type")
            };

            // ✅ Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // ✅ Full file path
            string fullPath = Path.Combine(folderPath, fileName);

            // ✅ Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Return URL path (for frontend use)
            string relativePath = $"/media/{type}s/{fileName}";

            return relativePath;
        }
    }
}
