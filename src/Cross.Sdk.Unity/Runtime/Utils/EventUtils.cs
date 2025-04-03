using Cross.Sdk.Unity.Model;

namespace Cross.Sdk.Unity.Utils
{
    public static class EventUtils
    {
        public static string GetWalletPlatform(Wallet wallet)
        {
#if UNITY_STANDALONE
            if (!string.IsNullOrWhiteSpace(wallet.DesktopLink))
                return "desktop";

            if (!string.IsNullOrWhiteSpace(wallet.WebappLink))
                return "web";
#elif UNITY_IOS || UNITY_VISIONOS || UNITY_ANDROID
            if (!string.IsNullOrWhiteSpace(wallet.MobileLink))
                return "mobile";

            if (!string.IsNullOrWhiteSpace(wallet.WebappLink))
                return "web";
#endif
            return null;
        }
    }
}