using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cross.Core.Common.Logging;
using Cross.Core.Interfaces;
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
            ///     만료된 Expirer 데이터를 정리할지 여부 (기본값: false)
            ///     주의: Expirer 모듈이 자동으로 만료 데이터를 정리하므로 일반적으로 불필요합니다.
            /// </summary>
            public bool CleanupExpiredData { get; set; } = false;

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
        ///     오래되고 사용하지 않는 Storage 데이터를 수동으로 정리합니다.
        ///     
        ///     ⚠️ 중요 제한사항:
        ///     이 메서드는 Storage만 정리하며, 이미 메모리에 로드된 모듈 캐시는 수정하지 않습니다.
        ///     MessageTracker 및 JsonRpcHistory 모듈은 다음 Persist() 호출 시 메모리 캐시를
        ///     Storage에 다시 저장하므로, 정리한 데이터가 복구될 수 있습니다.
        ///     
        ///     권장 사용 방법:
        ///     1. 앱 종료 직전에 호출 (다음 실행 시 정리된 Storage에서 로드)
        ///     2. 정리 후 앱 재시작
        ///     3. 런타임 정리보다는 자동 정리(AutoCleanupStorage) 사용 권장
        ///     
        ///     완전한 정리를 원하면:
        ///     - AutoCleanupStorage 옵션을 활성화하여 초기화 시 자동 정리
        ///     - CleanupStorageBeforeInit은 메모리 캐시 문제가 없음
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
                            // History는 pending request(응답 대기 중인 요청)를 포함할 수 있음
                            if (options.PreserveActiveSessionData && (activeTopics.Count > 0 || activePairingTopics.Count > 0))
                            {
                                // 활성 세션이 있으면 pending request 확인 후 정리
                                var hasPending = await HasPendingRequests(storage, key);
                                if (!hasPending)
                                {
                                    // Pending request가 없으면 안전하게 삭제
                                    shouldRemove = true;
                                    reason = "Resolved RPC history";
                                    result.HistoryKeysRemoved++;
                                }
                                else if (options.VerboseLogging)
                                {
                                    CrossLogger.Log($"[StorageCleanup] History 보존 (pending request 존재): {key}");
                                }
                            }
                            else
                            {
                                // 활성 세션이 없으면 전체 삭제
                                shouldRemove = true;
                                reason = "Old RPC history (no active sessions)";
                                result.HistoryKeysRemoved++;
                            }
                        }

                        // 2. MessageTracker 정리
                        if (options.CleanupMessages && IsMessageTrackerKey(key))
                        {
                            // MessageTracker는 단일 키에 모든 topic의 메시지를 저장하므로
                            // 키를 삭제하지 않고 내부 데이터를 필터링해야 함
                            if (options.PreserveActiveSessionData && (activeTopics.Count > 0 || activePairingTopics.Count > 0))
                            {
                                // 활성 세션이 있으면 부분 정리
                                var cleanupInfo = await CleanupMessageTrackerData(storage, key, activeTopics, activePairingTopics);
                                if (cleanupInfo.topicsRemoved > 0)
                                {
                                    result.MessageKeysRemoved += cleanupInfo.topicsRemoved;
                                    result.TotalKeysRemoved += cleanupInfo.topicsRemoved;
                                    result.EstimatedBytesFreed += cleanupInfo.bytesFreed;
                                    if (options.VerboseLogging)
                                    {
                                        CrossLogger.Log($"[StorageCleanup] MessageTracker 부분 정리: {cleanupInfo.topicsRemoved}개 topic 삭제, ~{cleanupInfo.bytesFreed} bytes 확보");
                                    }
                                }
                            }
                            else
                            {
                                // 활성 세션이 없으면 전체 삭제
                                shouldRemove = true;
                                reason = "Message tracker data (no active sessions)";
                                result.MessageKeysRemoved++;
                            }
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
        ///     Storage Init 직후, 모듈 Init 전에 정리합니다.
        ///     이 시점에서 정리하면 메모리 캐시 복구 문제를 방지할 수 있습니다.
        ///     
        ///     사용 시나리오:
        ///     - SDK 초기화 시 자동 정리 (SignClient.Initialize에서 자동 호출)
        ///     - 활성 세션 정보가 없으므로 보수적 정리만 수행
        ///     
        ///     제한 사항:
        ///     - Resolved된 JsonRpcHistory만 정리 (pending 없는 것만)
        ///     - MessageTracker는 정리하지 않음 (활성 세션 정보 필요)
        ///     
        ///     런타임에 수동 정리가 필요하면 CleanupStorageAsync를 사용하세요.
        /// </summary>
        /// <param name="storage">Storage 인스턴스 (반드시 Init되어 있어야 함)</param>
        /// <param name="options">정리 옵션 (null인 경우 기본값 사용)</param>
        /// <returns>정리 결과</returns>
        public static async Task<CleanupResult> CleanupStorageBeforeInit(IKeyValueStorage storage, CleanupOptions options = null)
        {
            options ??= new CleanupOptions();
            var result = new CleanupResult();

            try
            {
                var allKeys = await storage.GetKeys();

                if (options.VerboseLogging)
                {
                    CrossLogger.Log($"[StorageCleanup] 총 {allKeys.Length}개의 Storage 키 발견");
                }

                // Init 전이므로 활성 세션 정보를 알 수 없음
                // 보수적으로 정리: pending이 없는 resolved history만 삭제
                var keysToRemove = new List<string>();

                foreach (var key in allKeys)
                {
                    try
                    {
                        // 1. Resolved된 JsonRpcHistory만 정리
                        if (options.CleanupHistory && IsHistoryKey(key))
                        {
                            var hasPending = await HasPendingRequests(storage, key);
                            if (!hasPending)
                            {
                                keysToRemove.Add(key);
                                result.HistoryKeysRemoved++;
                                if (options.VerboseLogging)
                                {
                                    CrossLogger.Log($"[StorageCleanup] 삭제 예정: {key} (Resolved RPC history)");
                                }
                            }
                            else if (options.VerboseLogging)
                            {
                                CrossLogger.Log($"[StorageCleanup] 보존: {key} (pending requests 존재)");
                            }
                        }

                        // 2. MessageTracker는 보존 (활성 세션 정보를 알 수 없음)
                        // 사용자가 수동으로 CleanupStorageAsync 호출 시에만 정리 가능
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"키 '{key}' 분석 중 오류: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        CrossLogger.LogError($"[StorageCleanup] {errorMsg}");
                    }
                }

                // 실제 삭제 수행 (Storage와 메모리에서 모두 삭제)
                foreach (var key in keysToRemove)
                {
                    try
                    {
                        var itemSize = await EstimateKeySize(storage, key);
                        
                        // Storage에서 삭제
                        await storage.RemoveItem(key);
                        
                        result.TotalKeysRemoved++;
                        result.EstimatedBytesFreed += itemSize;
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"키 '{key}' 삭제 중 오류: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        CrossLogger.LogError($"[StorageCleanup] {errorMsg}");
                    }
                }

                if (options.VerboseLogging || result.TotalKeysRemoved > 0)
                {
                    CrossLogger.Log($"[StorageCleanup] 정리 완료: {result.TotalKeysRemoved}개 키 삭제, 약 {result.EstimatedBytesFreed / 1024}KB 확보");
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
            // 현재 구현되지 않음: Expirer 모듈이 자동으로 만료 데이터를 정리하므로
            // 이 cleanup 유틸리티에서 중복 처리할 필요가 없습니다.
            // 향후 필요 시 구현 가능하지만, 현재는 Expirer의 자동 처리를 신뢰합니다.
            return false;
        }

        private static bool IsKeyChainKey(string key)
        {
            return key.Contains("keychain");
        }

        /// <summary>
        ///     JsonRpcHistory에 pending request(응답 대기 중인 요청)가 있는지 확인합니다.
        /// </summary>
        private static async Task<bool> HasPendingRequests(IKeyValueStorage storage, string key)
        {
            try
            {
                // JsonRpcHistory 데이터 구조: Dictionary<long, JsonRpcRecord>
                // JsonRpcRecord의 Response가 null이면 pending
                var historyData = await storage.GetItem<object>(key);
                if (historyData == null)
                    return false;

                // 타입이 다양하므로 dynamic으로 처리하거나 JSON으로 확인
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(historyData);
                
                // Response가 null인 항목이 있는지 간단히 체크
                // 더 정확한 방법: 실제로 배열을 파싱해서 각 record의 response 확인
                // 하지만 안전을 위해 데이터가 있으면 일단 보존하는 것이 좋음
                
                // History 데이터는 배열 형태: JsonRpcRecord<T, TR>[]
                if (json.Contains("\"response\":null") || json.Contains("\"Response\":null"))
                {
                    // Pending request가 있음
                    return true;
                }

                // 모든 요청이 resolved되었으면 삭제 가능
                return false;
            }
            catch
            {
                // 오류 발생 시 안전을 위해 보존
                return true;
            }
        }

        private static bool IsActiveExpirerKey(ISignClient signClient, string key)
        {
            // 특정 키가 Expirer에서 현재 추적 중인지 확인
            // 참고: IsExpiredDataKey()가 현재 구현되지 않았으므로 이 메서드도 호출되지 않음
            try
            {
                // Storage key와 Expirer의 topic 키는 형식이 다를 수 있음
                // 안전하게 키 배열에서 직접 확인
                var expirerKeys = signClient.CoreClient.Expirer.Keys;
                return expirerKeys != null && expirerKeys.Contains(key);
            }
            catch
            {
                // 오류 시 보수적으로 true 반환 (삭제 안함)
                return true;
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

        /// <summary>
        ///     MessageTracker 데이터를 부분적으로 정리합니다.
        ///     활성 세션의 메시지는 보존하고, 비활성 topic의 메시지만 삭제합니다.
        /// </summary>
        private static async Task<(int topicsRemoved, long bytesFreed)> CleanupMessageTrackerData(
            IKeyValueStorage storage, 
            string key, 
            HashSet<string> activeTopics, 
            HashSet<string> activePairingTopics)
        {
            try
            {
                // MessageTracker 데이터 구조: Dictionary<string, MessageRecord>
                // MessageRecord는 Dictionary<string, string>을 상속한 클래스
                var messages = await storage.GetItem<Dictionary<string, MessageRecord>>(key);
                if (messages == null || messages.Count == 0)
                    return (0, 0);

                // 정리 전 크기 추정
                var sizeBefore = EstimateMessageTrackerSize(messages);

                var removedCount = 0;
                var allActiveTopics = new HashSet<string>(activeTopics);
                allActiveTopics.UnionWith(activePairingTopics);

                // 비활성 topic만 제거
                var keysToRemove = new List<string>();
                foreach (var topic in messages.Keys)
                {
                    if (!allActiveTopics.Contains(topic))
                    {
                        keysToRemove.Add(topic);
                    }
                }

                foreach (var topicToRemove in keysToRemove)
                {
                    messages.Remove(topicToRemove);
                    removedCount++;
                }

                // 변경된 데이터 저장 및 크기 계산
                long bytesFreed = 0;
                if (removedCount > 0)
                {
                    await storage.SetItem(key, messages);
                    
                    // 정리 후 크기 추정
                    var sizeAfter = EstimateMessageTrackerSize(messages);
                    bytesFreed = sizeBefore - sizeAfter;
                }

                return (removedCount, bytesFreed);
            }
            catch (Exception ex)
            {
                CrossLogger.LogError($"[StorageCleanup] MessageTracker 정리 중 오류: {ex.Message}");
                return (0, 0);
            }
        }

        /// <summary>
        ///     MessageTracker 데이터의 크기를 추정합니다.
        /// </summary>
        private static long EstimateMessageTrackerSize(Dictionary<string, MessageRecord> messages)
        {
            try
            {
                // 간단한 JSON 직렬화 기반 크기 추정
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
                return json.Length;
            }
            catch
            {
                // 추정 실패 시 대략적인 크기 반환
                return messages.Count * 1024; // 1KB per topic
            }
        }

        #endregion
    }
}
