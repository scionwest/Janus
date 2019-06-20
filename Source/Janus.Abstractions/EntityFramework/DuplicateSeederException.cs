using System;
using System.Runtime.Serialization;

namespace Janus.EntityFramework
{
    [Serializable]
    internal class DuplicateSeederException : Exception
    {
        public DuplicateSeederException()
        {
        }

        public DuplicateSeederException(string message) : base(message)
        {
        }

        public DuplicateSeederException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateSeederException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}