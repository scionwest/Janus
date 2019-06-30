using Janus.EntityFrameworkCore;
using Janus.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore
{
    public class JanusBuilder2
    {
        internal List<Action<IDatabaseBuilder>> databaseBuilderDelegates = new List<Action<IDatabaseBuilder>>();
        internal List<Action<ISeedManager>> seedManagerDelegates = new List<Action<ISeedManager>>();

        public JanusBuilder2 ConfigureDatabase(Action<IDatabaseBuilder> builderSetup)
        {
            this.databaseBuilderDelegates.Add(builderSetup);
            return this;
        }

        public JanusBuilder2 ConfigureSeeding(Action<ISeedManager> seedManagerDelegate)
        {
            this.seedManagerDelegates.Add(seedManagerDelegate);
            return this;
        }
    }

    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseJanus(this IWebHostBuilder hostBuilder, Action<JanusBuilder2> janusBuilder)
        {
            var builder = new JanusBuilder2();
            janusBuilder.Invoke(builder);
            hostBuilder.ConfigureServices((hostContext, services) => ConfigureJanusServices(builder, hostContext, services));
            return hostBuilder;
        }

        public static void ConfigureJanusServices(JanusBuilder2 janusBuilder, WebHostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            services.AddSingleton(janusBuilder);
            services.AddTransient<IDatabaseBuilder, JanusDatabaseBuilder>(provider =>
            {
                var dbBuilder = new JanusDatabaseBuilder();
                JanusBuilder2 jbuilder = provider.GetService<JanusBuilder2>();
                foreach(Action<IDatabaseBuilder> dbDelegate in jbuilder.databaseBuilderDelegates)
                {
                    dbDelegate.Invoke(dbBuilder);
                }

                return dbBuilder;
            });
        }

        public static IWebHostBuilder UseJanus2(this IWebHostBuilder hostBuilder, Action<JanusBuilder> builderDelegate)
        {
            var janusBuilder = new JanusBuilder();
            builderDelegate?.Invoke(janusBuilder);

            hostBuilder.ConfigureServices(services => services.AddSingleton<JanusBuilder>(_ => janusBuilder));
            hostBuilder.ConfigureServices(ConfigureServiceCollection);

            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
            {
                var connectionString = new Dictionary<string, string>()
                    { { "ConnectionStrings:Default", "Lorem Ipsum"}};
                configBuilder.AddInMemoryCollection(connectionString);
            });

            return hostBuilder;
        }

        private static void ConfigureServiceCollection(WebHostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder().AddConfiguration(hostBuilderContext.Configuration);
            var connectionString = new Dictionary<string, string>()
                    { { "ConnectionStrings:Default", "FooBar"}};

            configBuilder.AddInMemoryCollection(connectionString);
            IConfiguration configuration = configBuilder.Build();
            hostBuilderContext.Configuration = configuration;
            services.AddSingleton<IConfiguration>(_ => configuration);

            services.AddSingleton<IDatabaseBuilder, JanusDatabaseBuilder>();
            services.AddSingleton<IStartupFilter, JanusStartup>();
        }
    }

    public class JanusStartup : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                IServiceProvider services = builder.ApplicationServices;
                IConfiguration config = services.GetRequiredService<IConfiguration>();

                IDatabaseBuilder databaseBuilder = services.GetService<IDatabaseBuilder>();
                JanusBuilder janusBuilder = services.GetService<JanusBuilder>();
                janusBuilder.DatabaseBuilderConfiguration.Invoke(databaseBuilder);

            };
        }
    }

    public enum JanusBuilderConnectionStringConfiguration
    {
        WithConfigurationKey = 1,
        WithConnectionString = 2
    }

    public class JanusBuilder
    {
        internal Action<IDatabaseBuilder> DatabaseBuilderConfiguration { get; private set; }
        internal Action<ISeedManager> SeedManagerConfiguration { get; private set; }
        internal JanusBuilderConnectionStringConfiguration ConnectionStringConfiguration { get; private set; }
        internal string ConnectionStringConfigurationValue { get; private set; }

        public JanusBuilder WithConnectionStringConfiguration(JanusBuilderConnectionStringConfiguration connectionStringConfiguration, string configurationValue)
        {
            this.ConnectionStringConfiguration = connectionStringConfiguration;
            this.ConnectionStringConfigurationValue = ConnectionStringConfigurationValue;
            return this;
        }

        public JanusBuilder BuildDatabases(Action<IDatabaseBuilder> databaseBuilderDelegate)
        {
            this.DatabaseBuilderConfiguration = databaseBuilderDelegate;
            return this;
        }

        public JanusBuilder BuildSeeding(Action<ISeedManager> seedManagerConfigurationDelegate)
        {
            this.SeedManagerConfiguration = seedManagerConfigurationDelegate;
            return this;
        }
    }

    public class JanusSeedingBuilder
    {
        public JanusSeedingBuilder AddSeeder<TSeeder>() where TSeeder : IEntitySeeder
        {
            return this;
        }
    }

    public class JanusDbContextConfiguration
    {
        public DatabaseBuildBehavior DatabaseBehavior { get; set; }
        public Type DbContextType { get; set; }
    }

    public class JanusDbContextBuilder
    {
        internal Dictionary<Type, JanusDbContextConfiguration> ContextBuilders = new Dictionary<Type, JanusDbContextConfiguration>();

        public JanusDbContextBuilder WithDataContext<TContext>(DatabaseBuildBehavior databaseBehavior)
        {
            Type contextType = typeof(TContext);
            if (this.ContextBuilders.TryGetValue(contextType, out JanusDbContextConfiguration contextBuilder))
            {
                this.ContextBuilders[contextType].DatabaseBehavior = databaseBehavior;
                return this;
            }

            contextBuilder = new JanusDbContextConfiguration
            {
                DatabaseBehavior = databaseBehavior,
                DbContextType = contextType
            };

            this.ContextBuilders.Add(contextType, contextBuilder);
            return this;
        }
    }
}
