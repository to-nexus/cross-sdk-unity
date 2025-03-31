﻿using System;
using System.Runtime.Serialization;

namespace Cross.Core.Common.Model.Errors
{
    public class NamespacesException : Exception
    {
        public NamespacesException()
        {
        }

        public NamespacesException(string message)
            : base(message)
        {
        }

        public NamespacesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NamespacesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}