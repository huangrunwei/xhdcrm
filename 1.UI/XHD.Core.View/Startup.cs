

using FreeSql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.View.Configs;

namespace XHD.Core.View
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IHostEnvironment _env;
        private readonly ConfigHelper _configHelper;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            _configHelper = new ConfigHelper();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<Ihr_employeeService, hr_employeeService>();
            //services.AddSingleton<Ihr_employeeRepository, hr_employeeRepository>();
            //services.AddUEditorService();
            services.AddControllersWithViews();

            services.AddService();
            services.AddRepository();

            services.AddSession();
            services.AddDb(_env);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Index";
                    options.LogoutPath = "/Account/SignOut";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(120);

                });


            #if DEBUG
                 services.AddRazorPages().AddRazorRuntimeCompilation();
            #endif

            //跨域
            services.AddCors(options => options.AddPolicy("CorsPolicy",
               builder =>
               {
                   builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       //.SetIsOriginAllowed(origin => origin.StartsWith("http://192.168.*.*"));
                       .AllowAnyOrigin();  //测试环境才用这个
                       //.WithOrigins("https://sfs.huilongtech.com");

               }));

            
        }

        //public void ConfigureContainer(ContainerBuilder builder)
        //{
        //    builder.RegisterModule(new Configs.RepositoryModule());
        //    builder.RegisterModule(new Configs.ServiceModule());
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(configure =>
                {
                    configure.Run(async context =>
                    {
                        var exHeader = context.Features.Get<IExceptionHandlerPathFeature>();
                        var ex = exHeader.Error;
                        if (ex != default)
                        {
                            await context.Response.WriteAsJsonAsync(new { code = 500, errPath = exHeader.Path, msg = $"服务器内部错误->{ex.Message}" });
                        }

                        //NLogger.WriteLog("sys_Error1_", ex.Message);
                        NLogger.WriteLog("sys_Error_", ex.ToString());

                    });
                });
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".less"] = "text/css";

            app.UseSession();
            app.UseHttpsRedirection();

            var filePath = AppContext.BaseDirectory;

            //app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            app.UseCors("CorsPolicy");

            app.UseRouting();

           
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
