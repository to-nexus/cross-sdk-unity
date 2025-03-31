using System;
using System.Runtime.Serialization;

namespace Cross.Core.Common.Model.Errors
{
    public class ExpiredException : Exception
    {
        public ExpiredException()
        {
        }

        public ExpiredException(string message)
            : base(message)
        {
        }

        public ExpiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}