using Janus.EntityFrameworkCore;
using Janus.Seeding;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddJanus(this IServiceCollection services, Action<JanusBuilder> builderDelegate)
        {
            var janusBuilder = new JanusBuilder();
            builderDelegate?.Invoke(janusBuilder);
            services.AddSingleton<JanusBuilder>(_ => janusBuilder);

            IConfiguration configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            if (janusBuilder.DatabaseBuilderConfiguration != null)
            {
                services.AddSingleton<IDatabaseSeeder, JanusDatabaseSeeder>();
                services.AddSingleton<IDatabaseBuilder, JanusDatabaseBuilder>(provider =>
                {
                    JanusBuilder serviceBuilder = provider.GetService<JanusBuilder>();
                    var databaseBuilder = new JanusDatabaseBuilder();
                    serviceBuilder.DatabaseBuilderConfiguration?.Invoke(databaseBuilder);
                    return databaseBuilder;
                });

                if (janusBuilder.ConnectionStringConfiguration == JanusBuilderConnectionStringConfiguration.WithConfigurationKey)
                {
                    string connectionString = configuration[janusBuilder.ConnectionStringConfigurationValue];
                }
            }

            if (janusBuilder.SeedManagerConfiguration != null)
            {
                services.AddSingleton<ISeedManager, JanusSeedManager>(provider =>
                {
                    JanusBuilder serviceBuilder = provider.GetService<JanusBuilder>();
                    ISeedReaderFactory seedReaderFactory = provider.GetService<ISeedReaderFactory>();

                    var seedMaanger = new JanusSeedManager(seedReaderFactory);
                    serviceBuilder.SeedManagerConfiguration?.Invoke(seedMaanger);
                    return seedMaanger;
                });
                services.AddSingleton<ISeedReaderFactory, JanusSeedReaderFactory>();
            }
            return services;
        }
    }

    //public static class IServiceCollectionExtensions
    //{
    //    public static IServiceCollection AddJanus(this IServiceCollection services, Action<JanusBuilder> optionsBuilder)
    //    {
    //        services.AddSingleton<IStartupFilter, JanusSetup>();

    //        var builder = new JanusBuilder(services);
    //        optionsBuilder?.Invoke(builder);

    //        return services;
    //    }
    //}

    //public class JanusSetup : IStartupFilter
    //{
    //    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    //    {
    //        return builder =>
    //        {
    //            IServiceProvider services = builder.ApplicationServices;
    //            var bui = services.GetService<IEnumerable<IDatabaseBuilder>>();
    //            var a = bui.ToArray();
    //        };
    //    }
    //}

    //public class JanusSeedBuilder
    //{
    //    private readonly IServiceCollection serviceBuilder;

    //    public JanusSeedBuilder(IServiceCollection services)
    //    {
    //        this.serviceBuilder = services;
    //    }

    //    public JanusSeedBuilder AddSeeder<TSeeder>() where TSeeder : class, IEntitySeeder, new()
    //    {
    //        this.serviceBuilder.AddTransient<IEntitySeeder, TSeeder>();
    //        return this;
    //    }

    //    public JanusSeedBuilder AddSeeder(Type type)
    //    {
    //        this.serviceBuilder.AddTransient(provider => (IEntitySeeder)Activator.CreateInstance(type));
    //        return this;
    //    }

    //    public JanusSeedBuilder AddSeederCollection<TCollection>() where TCollection : ISeedCollection, new()
    //    {
    //        ISeedCollection collection = new TCollection();
    //        foreach (Type seedType in collection.GetSeederTypes())
    //        {
    //            this.AddSeeder(seedType);
    //        }

    //        return this;
    //    }
    //}

    //public class JanusBuilder
    //{
    //    private readonly IServiceCollection serviceBuilder;
    //    private readonly IConfigurationBuilder configurationBuilder;

    //    public JanusBuilder(IServiceCollection services)
    //    {
    //        this.serviceBuilder = services;
    //    }

    //    public JanusBuilder AddDatabaseForContext<TContext>(string connectionStringConfigurationKey) where TContext : DbContext
    //    {
    //        //this.serviceBuilder.AddTransient<IDatabaseBuilder, JanusDatabaseBuilder<TContext>>(provider =>
    //        //{
    //        //    return new JanusDatabaseBuilder<TContext>(connectionString);
    //        //});
    //        this.serviceBuilder.AddTransient<IDatabaseSeeder, JanusDatabaseSeeder>();
    //        return this;
    //    }

    //    public JanusSeedBuilder AddSeeding()
    //    {
    //        this.serviceBuilder.AddTransient<ISeedReader, JanusSeedReader>();
    //        this.serviceBuilder.AddTransient<ISeedReaderFactory, JanusSeedReaderFactory>();
    //        this.serviceBuilder.AddTransient<ISeedManager, JanusSeedManager>();
    //        return new JanusSeedBuilder(this.serviceBuilder);
    //    }
    //}
}
