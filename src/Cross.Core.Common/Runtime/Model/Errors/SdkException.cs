using System;

namespace Cross.Core.Common.Model.Errors
{
    /// <summary>
    /// Exception class representing SDK internal errors
    /// Contains error code and message based on ErrorType.
    /// </summary>
    public class SdkException : Exception
    {
        /// <summary>
        /// Error code (ErrorType enum value)
        /// </summary>
        public long ErrorCode { get; }

        /// <summary>
        /// Error type
        /// </summary>
        public ErrorType ErrorType { get; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage => Message;

        /// <summary>
        /// SdkException constructor
        /// </summary>
        /// <param name="errorType">Error type</param>
        /// <param name="message">Error message</param>
        public SdkException(ErrorType errorType, string message) : base(message)
        {
            ErrorCode = (long)errorType;
            ErrorType = errorType;
        }

        /// <summary>
        /// SdkException constructor (with inner exception)
        /// </summary>
        /// <param name="errorType">Error type</param>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public SdkException(ErrorType errorType, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = (long)errorType;
            ErrorType = errorType;
        }

        public override string ToString()
        {
            return $"[SdkException] Code: {ErrorCode} ({ErrorType}), Message: {Message}";
        }
    }
}

