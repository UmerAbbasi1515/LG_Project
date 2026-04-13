using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LG_projects.Common.EncryptionDecryption
{
    public class EncryptionDecryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionDecryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();
                using var sr = new StreamReader(
                    httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
                var originalContent = await sr.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(originalContent))
                {
                    var ds = JsonConvert.DeserializeObject<RB>(originalContent);
                    if (ds != null && !string.IsNullOrEmpty(ds.RequestBody))
                    {
                        var decryptedContent = EncryptionHelper.Decrypt(ds.RequestBody);

                        var bytes = Encoding.UTF8.GetBytes(decryptedContent);
                        var newBody = new MemoryStream(bytes);
                        newBody.Position = 0;                          // ← MUST reset to 0

                        httpContext.Request.Body = newBody;
                        httpContext.Request.ContentLength = bytes.Length;

                        // ↓ THIS IS CRITICAL — without this, model binding breaks
                        httpContext.Request.ContentType = "application/json; charset=utf-8";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Middleware decryption error: {ex.Message}");
            }

            await _next(httpContext);
        }
    }

    internal class RB
    {
        public string? RequestBody { get; set; }
    }

    public static class EncryptionDecryptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseEncryptionDecryptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EncryptionDecryptionMiddleware>();
        }
    }
}