using System;
using System.Runtime.Serialization;

namespace Janus.EntityFrameworkCore
{
    [Serializable]
    internal class InvalidDbContext : Exception
    {
        private const string message = "The Type '{0}' is not an instance of DbContext and can not be used.";

        public InvalidDbContext(Type contextType) : base(string.Format(message, contextType))
        {
        }

        public InvalidDbContext(Type type, Exception innertException) : base(string.Format(message, type), innertException)
        {
        }

        protected InvalidDbContext(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}