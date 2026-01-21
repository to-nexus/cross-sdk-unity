using mixpanel;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sample
{
    public class AppInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainScene;

        [Space]
        [SerializeField] private GameObject _debugConsole;

        private void Awake()
        {
            // Subscribe to deep link events
            Application.deepLinkActivated += OnDeepLinkActivated;
            
            // Check if app was opened via deep link
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        private void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        private void Start()
        {
            InitDebugConsole();
            ConfigureMixpanel();
            SceneManager.LoadScene(_mainScene);
        }

        private void OnDeepLinkActivated(string url)
        {
            Debug.Log($"[AppInit] Deep link activated: {url}");
            
            // The deep link brings the app to foreground
            // WalletConnect will handle the connection via WebSocket automatically
            // when the app comes back to foreground
        }

        private void InitDebugConsole()
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            DontDestroyOnLoad(gameObject);
            _debugConsole.SetActive(true);
#endif
        }

        private void ConfigureMixpanel()
        {
            Application.logMessageReceived += (logString, stackTrace, type) =>
            {
                var props = new Value
                {
                    ["type"] = type.ToString(),
                    ["scene"] = SceneManager.GetActiveScene().name
                };
                Mixpanel.Track(logString, props);
            };
        }
    }
}