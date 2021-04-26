using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeChatLogin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    //config.Sources.Clear();
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          //启用hosting.json地址，方式二
                          .AddJsonFile("hosting.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }

                    //if (env.IsDevelopment())
                    //{
                    ////默认CreateDefaultBuilder 会自动配置机密文件，否则需要手动配置机密文件
                    //    config.AddUserSecrets<Program>();
                    //}

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    webBuilder.UseIISIntegration();
                    webBuilder.UseIIS();
                    //启用hosting.json地址，方式一
                    //webBuilder.UseUrls("http://192.168.4.179:8058", "http://localhost:8058");

                    webBuilder.UseStartup<Startup>();
                });
    }
}
