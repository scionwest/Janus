using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Data;

namespace WebApplication1
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<AccountContext>(options =>
            {
                string connectionString = this.Configuration.GetConnectionString("Default");
                options.UseSqlite(connectionString);
            });

            services.AddJanusDatabaseSeeding(options =>
            {
                options.DatabaseBehavior = (Janus.DatabaseBehavior.ResetOnSeed | Janus.DatabaseBehavior.DeleteOnShutdown);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.ApplyJanusDatabase()
                    .ConfigureAndSeedDatabase<MasterContext>()
                    .WithData(context => context.Add(new UserEntity()));
            }

            app.UseMvc();
        }
    }
}
