using System.Reflection;

namespace Cross.Core.Common
{
    /// <summary>
    ///     Class defining SDK constants
    /// </summary>
    public static class SDKConstants
    {
        /// <summary>
        ///     The current version of the SDK
        /// </summary>
        public static readonly string SDK_VERSION = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0-undefined";
    }
}