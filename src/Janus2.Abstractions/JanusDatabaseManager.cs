using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus
{
    public class JanusDatabaseManager : IDatabaseManager
    {
        private Dictionary<Type, List<IDatabaseBuilder>> databaseBuilders = new Dictionary<Type, List<IDatabaseBuilder>>();
        private readonly IDatabaseSeedFactory seedFactory;

        public JanusDatabaseManager(IDatabaseSeedFactory seedFactory)
        {
            this.seedFactory = seedFactory;
        }

        public void Reset()
        {
            if (this.IsConfigured())
            {
                throw new InvalidOperationException("You can not reset a manager that has already been built. You must dispose of the manager and instantiate a new one.");
            }

            this.databaseBuilders.Clear();
        }

        public IDatabaseSeedBuilder<TContext> ConfigureAndSeedDatabase<TContext>() where TContext : DbContext
        {
            return this.ConfigureDatabase<TContext>()
                .SeedDatabase();
        }

        public IDatabaseBuilder<TContext> ConfigureDatabase<TContext>() where TContext : DbContext
        {
            return this.ConfigureDatabase<TContext>((initialName, context) =>
            {
                string timeStamp = DateTime.Now.ToString("HHmmss.fff");
                return string.IsNullOrEmpty(context)
                    ? $"{initialName}-{timeStamp}"
                    : $"{initialName}-{context}-{timeStamp}";
            });
        }

        public IDatabaseBuilder<TContext> ConfigureDatabase<TContext>(Func<string, string, string> databaseNamefactory) where TContext : DbContext
        {
            Type contextType = typeof(TContext);
            var builderOptions = new DatabaseBuilderOptions();
            builderOptions.ConnectionStringOptions.DatabaseNameFactory = databaseNamefactory;

            IDatabaseSeedReader seedReader = this.seedFactory.CreateSeedReader();
            IDatabaseSeedWriter seedWriter = this.seedFactory.CreateSeedWriter();
            var databaseBuilder = new JanusDatabaseBuilder<TContext>(builderOptions, seedReader, seedWriter);

            this.databaseBuilders[contextType].Add(databaseBuilder);
            return databaseBuilder;
        }

        public bool IsConfigured()
        {
            foreach(IDatabaseBuilder databaseBuilder in this.databaseBuilders)
            {
                if (databaseBuilder.IsConfigured())
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public void Dispose()
        {
            foreach(IDatabaseBuilder builder in this.databaseBuilders)
            {
                builder.Dispose();
            }
        }
    }
}
