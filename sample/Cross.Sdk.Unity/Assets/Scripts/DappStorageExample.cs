using System;
using UnityEngine;
using Cross.Sdk.Unity;
using Cross.Sign.Utils;

namespace Sample
{
    /// <summary>
    ///     Storage 정리 기능 사용 예제
    /// </summary>
    public class DappStorageExample : MonoBehaviour
    {
        [SerializeField] private CrossSdk _crossSdk;

        private void Awake()
        {
            // CrossSdk 인스턴스를 찾아서 할당
            if (_crossSdk == null)
            {
                _crossSdk = FindObjectOfType<CrossSdk>();
            }
        }

        /// <summary>
        ///     예제 1: 수동으로 Storage 정리하기
        /// </summary>
        public async void CleanupStorageManually()
        {
            if (_crossSdk?.SignClient == null)
            {
                Debug.LogError("[Storage Example] CrossSdk 또는 SignClient가 초기화되지 않았습니다.");
                return;
            }

            Debug.Log("[Storage Example] 수동 Storage 정리 시작...");

            try
            {
                // 상세 로그와 함께 정리
                var options = new StorageCleanupUtility.CleanupOptions
                {
                    CleanupHistory = true,
                    CleanupMessages = true,
                    CleanupExpiredData = true,
                    CleanupUnusedKeys = false, // 안전을 위해 비활성화
                    PreserveActiveSessionData = true, // 현재 세션 보존
                    VerboseLogging = true // 상세 로그 출력
                };

                var result = await _crossSdk.SignClient.CleanupStorageAsync(options);

                Debug.Log($"[Storage Example] ✅ 정리 완료!");
                Debug.Log($"- 총 삭제된 키: {result.TotalKeysRemoved}개");
                Debug.Log($"- History: {result.HistoryKeysRemoved}개");
                Debug.Log($"- Messages: {result.MessageKeysRemoved}개");
                Debug.Log($"- 확보된 공간: 약 {result.EstimatedBytesFreed / 1024}KB");

                if (result.Errors.Count > 0)
                {
                    Debug.LogWarning($"[Storage Example] ⚠️ {result.Errors.Count}개의 오류 발생:");
                    foreach (var error in result.Errors)
                    {
                        Debug.LogWarning($"  - {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Example] ❌ 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        ///     예제 2: 간단한 정리 (기본 옵션 사용)
        /// </summary>
        public async void CleanupStorageSimple()
        {
            if (_crossSdk?.SignClient == null)
            {
                Debug.LogError("[Storage Example] CrossSdk 또는 SignClient가 초기화되지 않았습니다.");
                return;
            }

            Debug.Log("[Storage Example] 간단한 Storage 정리 시작...");

            try
            {
                // 기본 옵션으로 정리
                var result = await _crossSdk.SignClient.CleanupStorageAsync();

                Debug.Log($"[Storage Example] ✅ {result.TotalKeysRemoved}개 키 삭제, " +
                         $"약 {result.EstimatedBytesFreed / 1024}KB 확보");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Example] ❌ 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        ///     예제 3: 개발/테스트용 - 모든 Storage 데이터 삭제
        ///     경고: 세션, 페어링 등 모든 정보가 삭제됩니다!
        /// </summary>
        public async void ClearAllStorageForDevelopment()
        {
            if (_crossSdk?.SignClient == null)
            {
                Debug.LogError("[Storage Example] CrossSdk 또는 SignClient가 초기화되지 않았습니다.");
                return;
            }

            Debug.LogWarning("[Storage Example] ⚠️ 모든 Storage 데이터를 삭제하시겠습니까?");
            Debug.LogWarning("[Storage Example] 이 작업은 되돌릴 수 없습니다!");

            try
            {
                // confirmDeletion=true를 전달해야 실제로 삭제됨
                var success = await _crossSdk.SignClient.ClearAllStorageAsync(confirmDeletion: true);

                if (success)
                {
                    Debug.Log("[Storage Example] ✅ 모든 Storage 데이터가 삭제되었습니다.");
                    Debug.Log("[Storage Example] 앱을 재시작하거나 다시 연결해야 합니다.");
                }
                else
                {
                    Debug.LogWarning("[Storage Example] Storage 삭제가 취소되었습니다.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Example] ❌ 삭제 실패: {ex.Message}");
            }
        }

        /// <summary>
        ///     예제 4: 정기적 정리 (예: 앱 시작 시 또는 주기적으로)
        /// </summary>
        private async void Start()
        {
            // CrossSdk 인스턴스 확인
            if (_crossSdk == null)
            {
                _crossSdk = FindObjectOfType<CrossSdk>();
            }

            // 앱 시작 시 한 번만 정리 (조용히)
            try
            {
                if (_crossSdk?.SignClient != null)
                {
                    var options = new StorageCleanupUtility.CleanupOptions
                    {
                        VerboseLogging = false // 시작 시에는 로그 최소화
                    };

                    var result = await _crossSdk.SignClient.CleanupStorageAsync(options);

                    if (result.TotalKeysRemoved > 0)
                    {
                        Debug.Log($"[Storage Example] 시작 시 정리: {result.TotalKeysRemoved}개 키 삭제");
                    }
                }
            }
            catch
            {
                // 실패해도 앱 시작을 방해하지 않음
            }
        }

        /// <summary>
        ///     예제 5: 커스텀 정리 옵션
        /// </summary>
        public async void CleanupWithCustomOptions()
        {
            if (_crossSdk?.SignClient == null)
            {
                Debug.LogError("[Storage Example] CrossSdk 또는 SignClient가 초기화되지 않았습니다.");
                return;
            }

            Debug.Log("[Storage Example] 커스텀 옵션으로 Storage 정리 시작...");

            try
            {
                var options = new StorageCleanupUtility.CleanupOptions
                {
                    // History만 정리하고 나머지는 유지
                    CleanupHistory = true,
                    CleanupMessages = false,
                    CleanupExpiredData = false,
                    CleanupUnusedKeys = false,
                    
                    // 현재 세션은 무조건 보존
                    PreserveActiveSessionData = true,
                    
                    // 상세 로그 출력
                    VerboseLogging = true
                };

                var result = await _crossSdk.SignClient.CleanupStorageAsync(options);

                Debug.Log($"[Storage Example] ✅ History 정리 완료: {result.HistoryKeysRemoved}개 삭제");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Storage Example] ❌ 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        ///     주의: 자동 정리 비활성화하려면 SignClientOptions에서 설정
        /// </summary>
        public void ExampleDisableAutoCleanup()
        {
            // SignClient 초기화 시:
            /*
            var options = new SignClientOptions
            {
                // ... 기타 옵션 ...
                AutoCleanupStorage = false  // 자동 정리 비활성화
            };
            
            var signClient = await SignClientUnity.Create(options);
            */
            
            Debug.Log("[Storage Example] 자동 정리를 비활성화하려면 SignClientOptions.AutoCleanupStorage = false로 설정하세요");
        }
    }
}
