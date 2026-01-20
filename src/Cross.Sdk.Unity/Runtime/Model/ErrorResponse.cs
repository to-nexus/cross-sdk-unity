using System;
using Cross.Core.Common.Model.Errors;

namespace Cross.Sdk.Unity
{
    /// <summary>
    /// Unified error response structure
    /// Provides error information in { errorCode, errorMessage } format.
    /// </summary>
    public struct ErrorResponse
    {
        /// <summary>
        /// Error code
        /// </summary>
        public long errorCode;

        /// <summary>
        /// Error message
        /// </summary>
        public string errorMessage;

        public ErrorResponse(long code, string message)
        {
            errorCode = code;
            errorMessage = message;
        }

        public override string ToString()
        {
            return $"[Error {errorCode}] {errorMessage}";
        }
    }

    /// <summary>
    /// Helper class to convert Exception to ErrorResponse
    /// </summary>
    public static class ErrorResponseHelper
    {
        /// <summary>
        /// Convert Exception to ErrorResponse
        /// </summary>
        public static ErrorResponse ToErrorResponse(this Exception exception)
        {
            return exception switch
            {
                SdkException sdk => new ErrorResponse(sdk.ErrorCode, sdk.ErrorMessage),
                CrossNetworkException network => new ErrorResponse(network.Code, network.Message),
                ArgumentNullException argNull => new ErrorResponse(
                    (long)ErrorType.ARGUMENT_NULL,
                    $"Argument is null: {argNull.ParamName}"
                ),
                ArgumentException arg => new ErrorResponse(
                    (long)ErrorType.ARGUMENT_INVALID,
                    arg.Message
                ),
                _ => new ErrorResponse(
                    (long)ErrorType.UNKNOWN,
                    exception.Message
                )
            };
        }

        /// <summary>
        /// Convert ErrorType to user-friendly message
        /// </summary>
        public static string GetFriendlyMessage(ErrorType errorType, string operationName = "operation")
        {
            return errorType switch
            {
                ErrorType.WALLET_NOT_CONNECTED => "🔌 Wallet is not connected.",
                ErrorType.SESSION_EXPIRED => "⏱️ Session expired. Please reconnect your wallet.",
                ErrorType.SESSION_NOT_FOUND => "❌ Session not found. Please reconnect your wallet.",
                ErrorType.JSONRPC_REQUEST_METHOD_REJECTED => $"❌ User rejected the {operationName}.",
                ErrorType.JSONRPC_REQUEST_TIMEOUT => "⏱️ Request timed out. Please try again.",
                ErrorType.UNAUTHORIZED_JSON_RPC_METHOD => "🚫 Method is not supported.",
                ErrorType.USER_DISCONNECTED => "🔌 User disconnected.",
                _ => $"An error occurred: {errorType}"
            };
        }
    }
}

