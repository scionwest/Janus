using System;

namespace Janus.EntityFrameworkCore
{
    public class JanusConnectionStringOptions
    {
        public JanusConnectionStringOptions()
        {
            this.DatabaseNameFactory = this.DefaultDatabaseName;
        }

        public string DatabaseKey { get; set; } = "Initial Catalog";
        public Func<string, string, string> DatabaseNameFactory { get; set; }

        private string DefaultDatabaseName(string dbName, string useCase)
        {
            string timeStamp = DateTime.Now.ToString("HHmmss.fff");
            return string.IsNullOrEmpty(useCase)
                ? $"Test-{dbName}-{timeStamp}"
                : $"Test-{dbName}-{useCase}-{timeStamp}";
        }
    }
}
