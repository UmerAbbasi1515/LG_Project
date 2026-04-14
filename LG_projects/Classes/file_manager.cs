using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace LG_projects.Classes
{
    public static class FileHelper
    {
        public static async Task<string> SaveFile(IFormFile? file, string type, IWebHostEnvironment env)
        {
            // If no file provided, return empty — caller handles this
            if (file == null || file.Length == 0)
                return "";

            type = type.ToLower();

            // ✅ Step 1 — Validate type string
            if (type != "video" && type != "audio" && type != "image")
                throw new Exception("Invalid file type. Must be image, video, or audio.");

            // ✅ Step 2 — Allowed extensions per type
            var allowedExtensions = type switch
            {
                "image" => new[] { ".jpg", ".jpeg", ".png", ".bmp", ".webp", ".heic", ".avif" },
                "video" => new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" },
                "audio" => new[] { ".mp3", ".wav", ".aac", ".ogg", ".m4a" },
                _ => throw new Exception("Invalid type")
            };

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                throw new Exception($"Invalid file extension '{ext}' for type '{type}'. " +
                                    $"Allowed: {string.Join(", ", allowedExtensions)}");

            // ✅ Step 3 — File size limits
            long maxSize = type switch
            {
                "image" => 5L * 1024 * 1024,   //   5 MB
                "audio" => 20L * 1024 * 1024,   //  20 MB
                "video" => 100L * 1024 * 1024,  // 100 MB
                _ => 10L * 1024 * 1024
            };

            if (file.Length > maxSize)
                throw new Exception($"File too large. Max allowed for {type}: {maxSize / 1024 / 1024} MB");

            // ✅ Step 4 — Generate unique filename using GUID
            string fileName = Guid.NewGuid().ToString() + ext;

            // ✅ Step 5 — Build folder path outside project folder
            string basePath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "..", "LGPMediaFiles"));

            string folderPath = type switch
            {
                "video" => Path.Combine(basePath, "videos"),
                "audio" => Path.Combine(basePath, "audio"),
                "image" => Path.Combine(basePath, "images"),
                _ => throw new Exception("Invalid type")
            };

            // ✅ Step 6 — Create folder if it does not exist
            Directory.CreateDirectory(folderPath); // safe to call even if already exists

            // ✅ Step 7 — Full path on disk
            string fullPath = Path.Combine(folderPath, fileName);

            // ✅ Step 8 — Write file from memory to disk
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Step 9 — Return relative URL path (stored in DB)
            // Frontend/browser will use BaseUrl + this path to preview
            return $"/media/{type}s/{fileName}";
            // Examples:
            //   /media/images/d4f8a1b2-xxxx.jpg
            //   /media/videos/a1b2c3d4-xxxx.mp4
            //   /media/audios/e5f6g7h8-xxxx.mp3
        }
    }
}