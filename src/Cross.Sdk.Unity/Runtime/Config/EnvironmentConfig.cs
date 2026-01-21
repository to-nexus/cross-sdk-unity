using UnityEngine;

namespace Cross.Sdk.Unity
{
    /// <summary>
    /// 환경별 설정을 관리합니다.
    /// Unity Editor에서는 메뉴(Cross SDK > Environment)로 Stage/Production 선택 가능
    /// Build에서는 Development Build = Staging, Release Build = Production
    /// </summary>
    public static class EnvironmentConfig
    {
        /// <summary>
        /// 현재 환경이 Staging인지 확인합니다.
        /// Editor에서는 EnvironmentSettings의 설정을 따르고,
        /// Build에서는 빌드 타입에 따라 자동 결정됩니다.
        /// </summary>
        public static bool IsStaging => EnvironmentSettings.Instance.IsStaging;

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

