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
                // Always decrypt the request body
                httpContext.Request.EnableBuffering(); // Allow reading multiple times

                using var sr = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
                var originalContent = await sr.ReadToEndAsync();
                httpContext.Request.Body.Position = 0; // Reset position for downstream

                if (!string.IsNullOrWhiteSpace(originalContent))
                {
                    var ds = JsonConvert.DeserializeObject<RB>(originalContent);

                    if (ds != null && !string.IsNullOrEmpty(ds.RequestBody))
                    {
                        // Decrypt
                        var decryptedContent = EncryptionHelper.Decrypt(ds.RequestBody);

                        // Replace the request body with decrypted JSON
                        var bytes = Encoding.UTF8.GetBytes(decryptedContent);
                        httpContext.Request.Body = new MemoryStream(bytes);
                        httpContext.Request.ContentLength = bytes.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Middleware decryption error: {ex.Message}");
                // Continue pipeline even if decryption fails
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