using System;
using Newtonsoft.Json;

namespace Cross.Core.Common.Model.Errors
{
    /// <summary>
    ///     An exception that is thrown internally by Cross.Core.Network. This
    ///     type can also be JSON serialized
    /// </summary>
    public class CrossNetworkException : Exception
    {
        /// <summary>
        ///     Create a new exception with the given message and error type
        /// </summary>
        /// <param name="message">The message that is shown with the exception</param>
        /// <param name="type">The error type for this exception (determines error code)</param>
        public CrossNetworkException(string message, ErrorType type) : base(message)
        {
            Code = (uint)type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        /// <summary>
        ///     Create a new exception with the given message and error type
        /// </summary>
        /// <param name="message">The message that is shown with the exception</param>
        /// <param name="type">The error type for this exception (determines error code)</param>
        /// <param name="innerException">The cause of this exception</param>
        public CrossNetworkException(string message, Exception innerException, ErrorType type) : base(message, innerException)
        {
            Code = (uint)type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        /// <summary>
        ///     The error code of this exception
        /// </summary>
        [JsonProperty("code")]
        public uint Code { get; private set; }

        /// <summary>
        ///     The error type of this exception
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; private set; }

        /// <summary>
        ///     The error code type as an ErrorType
        /// </summary>
        [JsonIgnore]
        public ErrorType CodeType
        {
            get => (ErrorType)Code;
        }

        /// <summary>
        ///     A helper function that creates an exception given an ErrorType, a message parameter,
        ///     an (optional) dictionary of parameters for the error message and an (optional) inner
        ///     exception
        /// </summary>
        /// <param name="type">The error type of the exception</param>
        /// <param name="message">An (optional) message for the error</param>
        /// <param name="context">An (optional) context for the error message</param>
        /// <param name="innerException">An (optional) inner exception that caused this exception</param>
        /// <returns>A new exception</returns>
        public static CrossNetworkException FromType(ErrorType type, string message = null, string context = null, Exception innerException = null)
        {
            var errorMessage = message ?? SdkErrors.MessageFromType(type, context);

            return innerException != null
                ? new CrossNetworkException(errorMessage, innerException, type)
                : new CrossNetworkException(errorMessage, type);
        }
    }
}