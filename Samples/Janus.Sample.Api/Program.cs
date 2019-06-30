using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Janus.EntityFrameworkCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseJanus(janusBuilder =>
                {
                    janusBuilder.ConfigureDatabase(builder =>
                    {
                        builder.DefaultSetup.BuildBehavior = DatabaseBuildBehavior.DeleteOnShutdown;
                        builder.AddContext<AppContext>(null);
                    });
                        
                })
                .UseJanus2(janusBuilder =>
                {
                    janusBuilder.BuildDatabases(dbBuilder =>
                    {
                        dbBuilder.AddContext<AppContext>(dbSetup =>
                        {
                            dbSetup.BuildBehavior = DatabaseBuildBehavior.ResetOnBuild;
                            //dbSetup.ConnectionStringFactory = (info) => info.GetUniqueConnectionString();
                        });
                    });
                })
                .UseStartup<Startup>();
    }
}
