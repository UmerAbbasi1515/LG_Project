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
            if (httpContext.Request.Method == "GET" ||
                httpContext.Request.ContentLength == null ||
                httpContext.Request.ContentLength == 0)
            {
                await _next(httpContext);
                return;
            }

            try
            {
                var contentType = httpContext.Request.ContentType ?? "";

                // ✅ FORM DATA HANDLING
                if (contentType.Contains("multipart/form-data") ||
                    contentType.Contains("application/x-www-form-urlencoded"))
                {
                    await HandleFormData(httpContext);
                }
                // ✅ JSON HANDLING
                else if (contentType.Contains("application/json"))
                {
                    await HandleJsonData(httpContext);
                }
                else
                {
                    // Block unknown content types
                    httpContext.Response.StatusCode = 400;
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        StatusCode = 400,
                        Message = "Unsupported content type.",
                        MessageUr = "غیر معاون کنٹینٹ ٹائپ۔"
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Middleware error: {ex.Message}");
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = 500,
                    Message = "Server error."
                }));
                return;
            }

            await _next(httpContext);
        }

        // ──────────────────────────────────────────
        // ✅ JSON HANDLER
        // ──────────────────────────────────────────
        private async Task HandleJsonData(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            using var sr = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
            var originalContent = await sr.ReadToEndAsync();
            httpContext.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(originalContent)) return;

            var ds = JsonConvert.DeserializeObject<RB>(originalContent);

            // BLOCK plain JSON
            if (string.IsNullOrEmpty(ds?.RequestBody))
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = 400,
                    Message = "Unencrypted requests are not allowed.",
                    MessageUr = "غیر انکرپٹڈ درخواست قبول نہیں۔"
                }));
                return;
            }

            // Decrypt full body
            string decryptedContent;
            try
            {
                decryptedContent = EncryptionHelper.Decrypt(ds.RequestBody);
            }
            catch
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = 400,
                    Message = "Invalid encrypted payload.",
                    MessageUr = "انکرپشن ڈیٹا غلط ہے۔"
                }));
                return;
            }

            // Decrypt each field + validate mobile
            var bodyDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decryptedContent);
            if (bodyDict != null)
            {
                var decryptedDict = new Dictionary<string, object>();
                var mobileRegex = new System.Text.RegularExpressions.Regex(@"^(\+92\d{10}|0\d{10})$");

                foreach (var kvp in bodyDict)
                {
                    string fieldValue = kvp.Value?.ToString() ?? "";
                    string decryptedValue = fieldValue;

                    try { decryptedValue = EncryptionHelper.Decrypt(fieldValue); }
                    catch { decryptedValue = fieldValue; }

                    if (kvp.Key.ToLower() == "mobile" && !string.IsNullOrEmpty(decryptedValue))
                    {
                        if (!mobileRegex.IsMatch(decryptedValue))
                        {
                            httpContext.Response.StatusCode = 400;
                            httpContext.Response.ContentType = "application/json";
                            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                StatusCode = 400,
                                Message = "Invalid mobile number format. Use 03001234567 or +923001234567",
                                MessageUr = "موبائل نمبر کا فارمیٹ غلط ہے۔"
                            }));
                            return;
                        }
                    }

                    decryptedDict[kvp.Key] = decryptedValue;
                }

                decryptedContent = JsonConvert.SerializeObject(decryptedDict);
            }

            var bytes = Encoding.UTF8.GetBytes(decryptedContent);
            var newBody = new MemoryStream(bytes);
            newBody.Position = 0;
            httpContext.Request.Body = newBody;
            httpContext.Request.ContentLength = bytes.Length;
            httpContext.Request.ContentType = "application/json; charset=utf-8";
        }

        // ──────────────────────────────────────────
        // ✅ FORM DATA HANDLER
        // ──────────────────────────────────────────
        private async Task HandleFormData(HttpContext httpContext)
        {
            var mobileRegex = new System.Text.RegularExpressions.Regex(@"^(\+92\d{10}|0\d{10})$");

            // Read existing form fields
            var form = httpContext.Request.Form;
            var decryptedFields = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            bool hasRequestBody = form.ContainsKey("RequestBody");

            // BLOCK plain form data — must have RequestBody key
            if (!hasRequestBody)
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = 400,
                    Message = "Unencrypted requests are not allowed.",
                    MessageUr = "غیر انکرپٹڈ درخواست قبول نہیں۔"
                }));
                return;
            }

            // Decrypt the RequestBody field value
            string decryptedContent;
            try
            {
                decryptedContent = EncryptionHelper.Decrypt(form["RequestBody"].ToString());
            }
            catch
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = 400,
                    Message = "Invalid encrypted payload.",
                    MessageUr = "انکرپشن ڈیٹا غلط ہے۔"
                }));
                return;
            }

            // Parse decrypted content as JSON fields
            var bodyDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decryptedContent);
            if (bodyDict != null)
            {
                foreach (var kvp in bodyDict)
                {
                    string fieldValue = kvp.Value?.ToString() ?? "";
                    string decryptedValue = fieldValue;

                    try { decryptedValue = EncryptionHelper.Decrypt(fieldValue); }
                    catch { decryptedValue = fieldValue; }

                    // Validate mobile
                    if (kvp.Key.ToLower() == "mobile" && !string.IsNullOrEmpty(decryptedValue))
                    {
                        if (!mobileRegex.IsMatch(decryptedValue))
                        {
                            httpContext.Response.StatusCode = 400;
                            httpContext.Response.ContentType = "application/json";
                            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                            {
                                StatusCode = 400,
                                Message = "Invalid mobile number format. Use 03001234567 or +923001234567",
                                MessageUr = "موبائل نمبر کا فارمیٹ غلط ہے۔"
                            }));
                            return;
                        }
                    }

                    decryptedFields[kvp.Key] = decryptedValue;
                }
            }

            // ✅ Rebuild form collection with decrypted values
            // Keep original files, replace fields with decrypted ones
            var formCollection = new FormCollection(decryptedFields, form.Files);
            httpContext.Request.Form = formCollection;
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