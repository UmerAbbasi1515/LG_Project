using System.Data;

namespace LG_projects.Classes
{
    public static class FileHelper
    {
            public static async Task<string> SaveFile(IFormFile file, string type, IWebHostEnvironment env)
            {
                if (file == null) return "";

                // Root path
                string rootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                string folderPath = Path.Combine(rootPath, "Uploads", type);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string fullPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return DB path
                return $"/Uploads/{type}/{fileName}";
            }
        }
}
