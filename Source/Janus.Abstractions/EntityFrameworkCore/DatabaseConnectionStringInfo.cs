using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Janus.EntityFrameworkCore
{
    public class DatabaseConnectionStringInfo
    {
        public DatabaseConnectionStringInfo(string connectionString, string useCase, JanusConnectionStringOptions connectionStringOptions)
        {
            this.ConnectionString = connectionString;
            this.UseCase = useCase;
            ConnectionStringOptions = connectionStringOptions;

            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts();
            if (connectionStringParts.TryGetValue(this.ConnectionStringOptions.DatabaseKey, out string dbName))
            {
                this.DatabaseName = dbName;
            }
            else
            {
                throw new InvalidConnectionStringConfiguration($"The {typeof(JanusConnectionStringOptions).FullName} was configured to look for the database name using '{connectionStringOptions.DatabaseKey}' as the key. The database could not be found using that key.");
            }
        }

        public DatabaseConnectionStringInfo(string connectionString, JanusConnectionStringOptions connectionStringOptions) : this(connectionString, string.Empty, connectionStringOptions)
        {
        }

        public string ConnectionString { get; }
        public string UseCase { get; }
        public string DatabaseName { get; }
        public JanusConnectionStringOptions ConnectionStringOptions { get; }

        public string GetUniqueDatabaseName()
        {
            return this.ConnectionStringOptions.DatabaseNameFactory(this.DatabaseName, this.UseCase);
        }

        public string GetUniqueConnectionString()
        {
            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts();
            connectionStringParts[this.ConnectionStringOptions.DatabaseKey] = this.GetUniqueDatabaseName();
            var connectionStringBuilder = new DbConnectionStringBuilder();

            foreach (KeyValuePair<string, string> element in connectionStringParts)
            {
                connectionStringBuilder[element.Key] = element.Value;
            }

            return connectionStringBuilder.ToString();
        }

        private Dictionary<string, string> GetConnectionStringParts()
        {
            Dictionary<string, string> parts = this.ConnectionString.Split(';')
                .Select(element => element.Split(new char[] { '=' }, 2))
                .ToDictionary(element => element[0].Trim(), element => element[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            return parts;
        }
    }
}
