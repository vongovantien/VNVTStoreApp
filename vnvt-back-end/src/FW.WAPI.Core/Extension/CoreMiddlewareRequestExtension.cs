using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.MediaType;
using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace FW.WAPI.Core.Extension
{
    public static class CoreMiddlewareRequestExtension
    {
        public static void UseCoreWithoutDomainResolver(this IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseDomainResolver();

            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var err = new { error = ex.Error.Message };
                        await context.Response.WriteAsync(JsonUtilities.ConvertObjectToJson(err));
                    }
                });
            });

            app.UseStatusCodePages(async ctx =>
            {
                // case 401: unauthorized
                if (ctx.HttpContext.Response.StatusCode == 401)
                {
                    var headerAuthen = ctx.HttpContext.Response.Headers["WWW-Authenticate"].ToString();
                    var response = new ResponseDTO();

                    switch (headerAuthen)
                    {
                        case "Bearer":
                            response.Code = 904;
                            response.Message = "Cannot found the token";
                            break;

                        case "Bearer error=\"invalid_token\", error_description=\"The token is expired\"":
                            response.Code = 905;
                            response.Message = "The token has expired";
                            break;

                        case "Bearer error=\"invalid_token\"":
                            response.Code = 904;
                            response.Message = "Invalid token";
                            break;

                        default:
                            break;
                    }

                    ctx.HttpContext.Response.ContentType = "application/json";
                    await ctx.HttpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
                }
            });

            app.UseAuthentication();
            app.UseResponseCompression();

            app.UseCors(CorsPolicy.CORS_POLICY_NAME);
            //app.Use(async (context, next) =>
            //{
            //    await next.Invoke();
            //});

            app.UseMvc(route =>
            {
                route.MapRoute("default_route", "{controller}/{action}/{id?}");
            });

            app.UseStaticFiles();
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

        public static void UseCore(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDomainResolver();

            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = MediaTypeName.JSON;
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var err = new { error = ex.Error.Message };
                        await context.Response.WriteAsync(JsonUtilities.ConvertObjectToJson(err));
                    }
                });
            });


            app.UseStatusCodePages(async ctx =>
            {
                // case 401: unauthorized
                if (ctx.HttpContext.Response.StatusCode == 401)
                {
                    var headerAuthen = ctx.HttpContext.Response.Headers["WWW-Authenticate"].ToString();
                    var response = new ResponseDTO();

                    switch (headerAuthen)
                    {
                        case "Bearer":
                            response.Code = 904;
                            response.Message = "Cannot found the token";
                            break;

                        case "Bearer error=\"invalid_token\", error_description=\"The token is expired\"":
                            response.Code = 905;
                            response.Message = "The token has expired";
                            break;

                        case "Bearer error=\"invalid_token\"":
                            response.Code = 904;
                            response.Message = "Invalid token";
                            break;

                        default:
                            break;
                    }

                    ctx.HttpContext.Response.ContentType = "application/json";
                    await ctx.HttpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
                }
            });

            app.UseAuthentication();
            app.UseResponseCompression();

            app.UseCors(CorsPolicy.CORS_POLICY_NAME);

            app.UseMvc(route =>
            {
                route.MapRoute("default_route", "{controller}/{action}/{id?}");
            });

            app.UseStaticFiles();
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}