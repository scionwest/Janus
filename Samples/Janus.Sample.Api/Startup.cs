using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janus.EntityFrameworkCore;
using Janus.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApplication4
{
    public class FooEntity
    {

    }

    public class FooSeeder : JanusEntitySeeder<FooEntity>
    {
        protected override bool MapEntities(FooEntity[] seededEntities, ISeedReader seedReader)
        {
            throw new NotImplementedException();
        }

        protected override IList<FooEntity> Seed(JanusSeedOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options) { }
    }

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

            services.AddDbContext<AppContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            services.AddJanus(options =>
            {
                options
                    .AddDatabaseForContext<AppContext>(Configuration.GetConnectionString("Default"))
                    .AddSeeding()
                        .AddSeeder<FooSeeder>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
