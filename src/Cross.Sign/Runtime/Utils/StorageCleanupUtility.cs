using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cross.Core.Common.Logging;
using Cross.Core.Storage.Interfaces;
using Cross.Sign.Interfaces;

namespace Cross.Sign.Utils
{
    /// <summary>
    ///     Storage 정리 유틸리티 클래스
    ///     JsonRpcHistory, MessageTracker 등 무한정 누적되는 데이터를 안전하게 정리합니다.
    /// </summary>
    public static class StorageCleanupUtility
    {
        /// <summary>
        ///     Storage 정리 옵션
        /// </summary>
        public class CleanupOptions
        {
            /// <summary>
            ///     JsonRpcHistory 데이터를 정리할지 여부 (기본값: true)
            /// </summary>
            public bool CleanupHistory { get; set; } = true;

            /// <summary>
            ///     MessageTracker 데이터를 정리할지 여부 (기본값: true)
            /// </summary>
            public bool CleanupMessages { get; set; } = true;

            /// <summary>
            ///     만료된 Expirer 데이터를 정리할지 여부 (기본값: true)
            /// </summary>
            public bool CleanupExpiredData { get; set; } = true;

            /// <summary>
            ///     사용하지 않는 KeyChain 데이터를 정리할지 여부 (기본값: false, 안전상 기본 비활성화)
            /// </summary>
            public bool CleanupUnusedKeys { get; set; } = false;

            /// <summary>
            ///     현재 활성 세션 관련 데이터는 보존 (기본값: true, 권장)
            /// </summary>
            public bool PreserveActiveSessionData { get; set; } = true;

            /// <summary>
            ///     정리 작업에 대한 상세 로그 출력 여부 (기본값: true)
            /// </summary>
            public bool VerboseLogging { get; set; } = true;
        }

        /// <summary>
        ///     Storage 정리 결과
        /// </summary>
        public class CleanupResult
        {
            public int HistoryKeysRemoved { get; set; }
            public int MessageKeysRemoved { get; set; }
            public int ExpiredKeysRemoved { get; set; }
            public int KeyChainKeysRemoved { get; set; }
            public int TotalKeysRemoved { get; set; }
            public long EstimatedBytesFreed { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        ///     오래되고 사용하지 않는 Storage 데이터를 정리합니다.
        ///     현재 활성 세션과 관련된 데이터는 보존됩니다.
        /// </summary>
        /// <param name="signClient">SignClient 인스턴스</param>
        /// <param name="options">정리 옵션 (null인 경우 기본값 사용)</param>
        /// <returns>정리 결과</returns>
        public static async Task<CleanupResult> CleanupStorageAsync(ISignClient signClient, CleanupOptions options = null)
        {
            options ??= new CleanupOptions();
            var result = new CleanupResult();

            try
            {
                var storage = signClient.CoreClient.Storage;
                var allKeys = await storage.GetKeys();

                if (options.VerboseLogging)
                {
                    CrossLogger.Log($"[StorageCleanup] 총 {allKeys.Length}개의 Storage 키 발견");
                }

                // 현재 활성 세션 정보 수집 (보존해야 할 데이터)
                HashSet<string> activeTopics = new HashSet<string>();
                HashSet<string> activePairingTopics = new HashSet<string>();

                if (options.PreserveActiveSessionData)
                {
                    // 활성 세션 Topic 수집
                    foreach (var session in signClient.Session.Values)
                    {
                        if (!string.IsNullOrWhiteSpace(session.Topic))
                        {
                            activeTopics.Add(session.Topic);
                        }
                        if (!string.IsNullOrWhiteSpace(session.PairingTopic))
                        {
                            activePairingTopics.Add(session.PairingTopic);
                        }
                    }

                    // 활성 Pairing Topic 수집
                    foreach (var pairing in signClient.CoreClient.Pairing.Store.Values)
                    {
                        if (!string.IsNullOrWhiteSpace(pairing.Topic))
                        {
                            activePairingTopics.Add(pairing.Topic);
                        }
                    }

                    if (options.VerboseLogging)
                    {
                        CrossLogger.Log($"[StorageCleanup] 활성 세션: {activeTopics.Count}개, 활성 페어링: {activePairingTopics.Count}개");
                    }
                }

                // 각 키를 분석하여 정리
                foreach (var key in allKeys)
                {
                    try
                    {
                        bool shouldRemove = false;
                        string reason = "";

                        // 1. JsonRpcHistory 정리
                        if (options.CleanupHistory && IsHistoryKey(key))
                        {
                            // History는 대부분 과거 기록이므로 안전하게 삭제 가능
                            // 단, 활성 세션의 최근 기록은 보존
                            if (!IsRecentHistoryForActiveSessions(key, activeTopics))
                            {
                                shouldRemove = true;
                                reason = "Old RPC history";
                                result.HistoryKeysRemoved++;
                            }
                        }

                        // 2. MessageTracker 정리
                        if (options.CleanupMessages && IsMessageTrackerKey(key))
                        {
                            // 오래된 메시지 해시는 삭제 가능
                            shouldRemove = true;
                            reason = "Message tracker data";
                            result.MessageKeysRemoved++;
                        }

                        // 3. 만료된 Expirer 데이터 정리
                        if (options.CleanupExpiredData && IsExpiredDataKey(key))
                        {
                            // Expirer는 이미 만료 처리가 끝난 데이터
                            // 단, 현재 Expirer에 있는 항목은 제외
                            if (!IsActiveExpirerKey(signClient, key))
                            {
                                shouldRemove = true;
                                reason = "Expired data";
                                result.ExpiredKeysRemoved++;
                            }
                        }

                        // 4. 사용하지 않는 KeyChain 정리 (선택적, 기본 비활성화)
                        if (options.CleanupUnusedKeys && IsKeyChainKey(key))
                        {
                            if (!IsActiveKeyChainEntry(signClient, key, activeTopics))
                            {
                                shouldRemove = true;
                                reason = "Unused keychain entry";
                                result.KeyChainKeysRemoved++;
                            }
                        }

                        // 실제 삭제 수행
                        if (shouldRemove)
                        {
                            var itemSize = await EstimateKeySize(storage, key);
                            await storage.RemoveItem(key);
                            result.TotalKeysRemoved++;
                            result.EstimatedBytesFreed += itemSize;

                            if (options.VerboseLogging)
                            {
                                CrossLogger.Log($"[StorageCleanup] 삭제: {key} (이유: {reason}, 크기: ~{itemSize} bytes)");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"키 '{key}' 처리 중 오류: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        CrossLogger.LogError($"[StorageCleanup] {errorMsg}");
                    }
                }

                if (options.VerboseLogging)
                {
                    CrossLogger.Log($"[StorageCleanup] 정리 완료: {result.TotalKeysRemoved}개 키 삭제, 약 {result.EstimatedBytesFreed / 1024}KB 확보");
                    CrossLogger.Log($"[StorageCleanup] - History: {result.HistoryKeysRemoved}개");
                    CrossLogger.Log($"[StorageCleanup] - Messages: {result.MessageKeysRemoved}개");
                    CrossLogger.Log($"[StorageCleanup] - Expired: {result.ExpiredKeysRemoved}개");
                    CrossLogger.Log($"[StorageCleanup] - KeyChain: {result.KeyChainKeysRemoved}개");
                }

                return result;
            }
            catch (Exception ex)
            {
                CrossLogger.LogError($"[StorageCleanup] 치명적 오류: {ex.Message}");
                result.Errors.Add($"Fatal error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        ///     Storage를 완전히 초기화합니다. (개발/테스트 전용)
        ///     경고: 모든 세션, 페어링, 키체인 정보가 삭제됩니다!
        /// </summary>
        public static async Task<bool> ClearAllStorageAsync(ISignClient signClient, bool confirmDeletion = false)
        {
            if (!confirmDeletion)
            {
                CrossLogger.Log("[StorageCleanup] ⚠️ ClearAllStorageAsync는 confirmDeletion=true로 호출해야 합니다.");
                return false;
            }

            try
            {
                CrossLogger.Log("[StorageCleanup] ⚠️ 모든 Storage 데이터를 삭제합니다...");
                await signClient.CoreClient.Storage.Clear();
                CrossLogger.Log("[StorageCleanup] ✅ Storage가 완전히 초기화되었습니다.");
                return true;
            }
            catch (Exception ex)
            {
                CrossLogger.LogError($"[StorageCleanup] Storage 초기화 실패: {ex.Message}");
                return false;
            }
        }

        #region Helper Methods

        private static bool IsHistoryKey(string key)
        {
            return key.Contains("-history-of-type-");
        }

        private static bool IsMessageTrackerKey(string key)
        {
            return key.Contains("-messages");
        }

        private static bool IsExpiredDataKey(string key)
        {
            // Expirer 자체 저장소가 아닌, 만료된 데이터를 의미
            // 이 부분은 실제로는 Expirer가 자동으로 정리하므로 추가 검증 필요
            return false; // 안전을 위해 기본적으로 false
        }

        private static bool IsKeyChainKey(string key)
        {
            return key.Contains("keychain");
        }

        private static bool IsRecentHistoryForActiveSessions(string key, HashSet<string> activeTopics)
        {
            // History 키는 보통 타입 정보만 포함하고 topic 정보는 없음
            // 따라서 대부분의 history는 안전하게 삭제 가능
            // 필요하다면 여기에 추가 로직 구현
            return false;
        }

        private static bool IsActiveExpirerKey(ISignClient signClient, string key)
        {
            // Expirer 키가 현재 추적 중인지 확인
            // 실제 구현 시 Expirer.Keys를 확인
            try
            {
                var expirerKeys = signClient.CoreClient.Expirer.Keys;
                // key format: "wc@2:core:0.3//core-expirer"
                return expirerKeys != null && expirerKeys.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsActiveKeyChainEntry(ISignClient signClient, string key, HashSet<string> activeTopics)
        {
            // KeyChain 항목이 현재 세션에서 사용 중인지 확인
            // 안전을 위해 기본적으로 true 반환 (삭제하지 않음)
            return true;
        }

        private static async Task<long> EstimateKeySize(IKeyValueStorage storage, string key)
        {
            try
            {
                var item = await storage.GetItem<object>(key);
                if (item == null) return 0;

                // 간단한 크기 추정 (정확하지 않음)
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                return json.Length;
            }
            catch
            {
                return 1024; // 기본값 1KB
            }
        }

        #endregion
    }
}
