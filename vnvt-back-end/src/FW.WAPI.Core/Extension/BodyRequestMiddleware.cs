using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Extension
{
    public class BodyRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ENCRYPTED_KEY = "encryptedKey";

        public BodyRequestMiddleware(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext httpContext, IConfiguration configuration)
        {
            var body = httpContext.Request.Body;

            if (body != null)
            {
                using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
                {
                    var bodyString = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(bodyString))
                    {
                        var encryptedKey = configuration.GetValue<string>(ENCRYPTED_KEY);
                        bodyString = bodyString.Replace("\"", "");
                        var descrypt = SimpleStringCipher.Instance.Decrypt(bodyString, encryptedKey, Encoding.ASCII.GetBytes(encryptedKey));
                        var requestDTO = JsonConvert.DeserializeObject<RequestDTO>(descrypt.ToString());
                        string bodySer = JsonConvert.SerializeObject(requestDTO);

                        var requestContent = new StringContent(bodySer, Encoding.UTF8, "application/json");
                        var stream = await requestContent.ReadAsStreamAsync();//modified stream
                        httpContext.Request.Body = stream;
                    }

                }
            }

            await _next.Invoke(httpContext);
        }
    }

    public static class BodyRequestMiddlewareExtension
    {
        public static IApplicationBuilder UseBodyRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BodyRequestMiddleware>();
        }
    }
}
