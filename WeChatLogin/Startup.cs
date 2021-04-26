using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Unicode;
using System.Threading.Tasks;
using WeChatLogin.common;

namespace WeChatLogin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(option=>option.JsonSerializerOptions.Encoder=System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All));//设置编码，防止返回json 中文乱码
            services.AddHttpClient();//注入HttpClient
            services.AddMvc();

            //services.AddOptions<SettingConfig>();
            services.Configure<SettingConfig>(Configuration.GetSection("AppSettings"));
            //显示注册为SettingConfig 类型，不使用IOption<T>接口类型
            services.AddSingleton(resolver =>
                        resolver.GetRequiredService<IOptions<SettingConfig>>().Value);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //启用cors跨域请求
                app.UseCors(builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                       name: "default",
                       pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
