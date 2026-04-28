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
            var json = JsonConvert.SerializeObject(new
            {
                mobile = "+923025784083"
            });

            var encrypted = EncryptionHelper.Encrypt(json);

            Console.WriteLine(encrypted);
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

                    // ✅ BLOCK plain requests
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

                    // ✅ Step 1: Decrypt full body
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

                    // ✅ Step 2: Decrypt each field value inside the body
                    var bodyDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decryptedContent);
                    if (bodyDict != null)
                    {
                        var decryptedDict = new Dictionary<string, object>();
                        var mobileRegex = new System.Text.RegularExpressions.Regex(@"^(\+92\d{10}|0\d{10})$");

                        foreach (var kvp in bodyDict)
                        {
                            string fieldValue = kvp.Value?.ToString() ?? "";
                            string decryptedValue = fieldValue;

                            // Try to decrypt field value
                            try
                            {
                                decryptedValue = EncryptionHelper.Decrypt(fieldValue);
                            }
                            catch
                            {
                                // Not encrypted — use as is
                                decryptedValue = fieldValue;
                            }

                            // ✅ Step 3: Validate mobile format if field is mobile
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

                    // ✅ Step 4: Replace body with fully decrypted plain JSON
                    var bytes = Encoding.UTF8.GetBytes(decryptedContent);
                    var newBody = new MemoryStream(bytes);
                    newBody.Position = 0;
                    httpContext.Request.Body = newBody;
                    httpContext.Request.ContentLength = bytes.Length;
                    httpContext.Request.ContentType = "application/json; charset=utf-8";
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