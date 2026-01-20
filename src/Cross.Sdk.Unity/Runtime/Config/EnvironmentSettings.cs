using UnityEngine;

namespace Cross.Sdk.Unity
{
    /// <summary>
    /// 환경 설정을 저장하는 ScriptableObject입니다.
    /// Unity Editor 메뉴에서 Stage/Production을 선택할 수 있습니다.
    /// </summary>
    public class EnvironmentSettings : ScriptableObject
    {
        private const string ResourcePath = "CrossEnvironmentSettings";
        private static EnvironmentSettings _instance;

        [SerializeField]
        [Tooltip("true = Staging, false = Production")]
        private bool isStaging = true;

        /// <summary>
        /// 현재 환경이 Staging인지 반환합니다.
        /// Release Build에서는 무조건 Production(false)을 반환합니다.
        /// </summary>
        public bool IsStaging
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                // Editor 또는 Development Build: 저장된 설정 사용
                return isStaging;
#else
                // Release Build: 무조건 Production
                return false;
#endif
            }
        }

        /// <summary>
        /// 환경 설정 인스턴스를 가져옵니다.
        /// </summary>
        public static EnvironmentSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EnvironmentSettings>(ResourcePath);
                    
                    // Resources에 없으면 기본값 생성 (Editor에서는 true, Build에서는 빌드 타입 따름)
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EnvironmentSettings>();
#if UNITY_EDITOR
                        _instance.isStaging = true;
#elif DEVELOPMENT_BUILD
                        _instance.isStaging = true;
#else
                        _instance.isStaging = false;
#endif
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 환경 설정을 변경합니다 (Editor 전용).
        /// </summary>
        public void SetEnvironment(bool staging)
        {
#if UNITY_EDITOR
            isStaging = staging;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[EnvironmentSettings] Environment changed to: {(staging ? "STAGING" : "PRODUCTION")}");
#endif
        }
    }
}

