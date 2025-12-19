using UnityEngine;

namespace Cross.Sdk.Unity
{
    /// <summary>
    /// 환경별 설정을 관리합니다.
    /// Editor 및 Development Build = Staging
    /// Release Build = Production
    /// </summary>
    public static class EnvironmentConfig
    {
        /// <summary>
        /// 현재 환경이 Staging인지 확인합니다.
        /// </summary>
        public static bool IsStaging
        {
            get
            {
#if UNITY_EDITOR
                // Editor는 항상 Staging
                return true;
#elif DEVELOPMENT_BUILD
                // Development Build도 Staging
                return true;
#else
                // Release Build는 Production
                return false;
#endif
            }
        }

        /// <summary>
        /// 현재 환경 이름을 반환합니다.
        /// </summary>
        public static string EnvironmentName => IsStaging ? "STAGING" : "PRODUCTION";

        /// <summary>
        /// 환경별 API Base URL을 반환합니다.
        /// </summary>
        public static string GetApiBaseUrl()
        {
            return IsStaging 
                ? "https://stg-wallet-server.crosstoken.io"
                : "https://wallet-server.crosstoken.io";
        }

        /// <summary>
        /// 환경별 Relay URL을 반환합니다.
        /// </summary>
        public static string GetRelayUrl()
        {
            return IsStaging
                ? "wss://stg-cross-relay.crosstoken.io/ws"
                : "wss://cross-relay.crosstoken.io/ws";
        }

        /// <summary>
        /// Chain Info API 경로를 반환합니다.
        /// </summary>
        public static string GetChainInfoApiPath()
        {
            return "api/v1/public/chain/info?from=cross-sdk-unity";
        }

        /// <summary>
        /// 전체 Chain Info API URL을 반환합니다.
        /// </summary>
        public static string GetChainInfoApiUrl()
        {
            return $"{GetApiBaseUrl()}/{GetChainInfoApiPath()}";
        }
    }
}

