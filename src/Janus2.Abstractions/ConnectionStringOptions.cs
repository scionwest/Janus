using System;

namespace Janus
{
    public class ConnectionStringOptions
    {
        public string DatabaseKey { get; set; } = "Initial Catalog";
        public string ConfigurationKey { get; set; } = "ConnectionStrings:Default";
        public Func<string, string, string> DatabaseNameFactory { get; set; } = (dbName, context) => $"Test-{dbName}-{DateTime.Now.ToString("HHmmss.fff")}";
    }
}
