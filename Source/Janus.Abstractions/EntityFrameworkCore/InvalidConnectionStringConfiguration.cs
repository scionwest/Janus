using System;
using System.Runtime.Serialization;

namespace Janus.EntityFrameworkCore
{
    [Serializable]
    internal class InvalidConnectionStringConfiguration : Exception
    {
        public InvalidConnectionStringConfiguration()
        {
        }

        public InvalidConnectionStringConfiguration(string message) : base(message)
        {
        }

        public InvalidConnectionStringConfiguration(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidConnectionStringConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}