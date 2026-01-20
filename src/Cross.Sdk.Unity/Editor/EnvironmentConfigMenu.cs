using UnityEditor;
using UnityEngine;
using System.IO;

namespace Cross.Sdk.Unity.Editor
{
    /// <summary>
    /// Unity Editor 메뉴에서 환경 설정을 변경할 수 있는 메뉴를 제공합니다.
    /// </summary>
    public static class EnvironmentConfigMenu
    {
        private const string MenuRoot = "Cross SDK/Environment/";
        private const string StagingMenu = MenuRoot + "🔵 Staging";
        private const string ProductionMenu = MenuRoot + "🔴 Production";
        private const string ResourcePath = "Assets/Resources/CrossEnvironmentSettings.asset";

        [MenuItem(StagingMenu, false, 1)]
        private static void SetStaging()
        {
            var settings = GetOrCreateSettings();
            settings.SetEnvironment(true);
            Menu.SetChecked(StagingMenu, true);
            Menu.SetChecked(ProductionMenu, false);
            Debug.Log("[Cross SDK] Environment set to: STAGING");
        }

        [MenuItem(ProductionMenu, false, 2)]
        private static void SetProduction()
        {
            var settings = GetOrCreateSettings();
            settings.SetEnvironment(false);
            Menu.SetChecked(StagingMenu, false);
            Menu.SetChecked(ProductionMenu, true);
            Debug.Log("[Cross SDK] Environment set to: PRODUCTION");
        }

        [MenuItem(StagingMenu, true)]
        private static bool ValidateStaging()
        {
            var settings = EnvironmentSettings.Instance;
            Menu.SetChecked(StagingMenu, settings.IsStaging);
            return true;
        }

        [MenuItem(ProductionMenu, true)]
        private static bool ValidateProduction()
        {
            var settings = EnvironmentSettings.Instance;
            Menu.SetChecked(ProductionMenu, !settings.IsStaging);
            return true;
        }

        private static EnvironmentSettings GetOrCreateSettings()
        {
            var settings = EnvironmentSettings.Instance;
            
            // ScriptableObject가 아직 저장되지 않았으면 생성
            if (!AssetDatabase.Contains(settings))
            {
                // Resources 폴더가 없으면 생성
                var resourcesDir = "Assets/Resources";
                if (!Directory.Exists(resourcesDir))
                {
                    Directory.CreateDirectory(resourcesDir);
                }

                // ScriptableObject 저장
                AssetDatabase.CreateAsset(settings, ResourcePath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Cross SDK] Created EnvironmentSettings at: {ResourcePath}");
            }

            return settings;
        }

        /// <summary>
        /// 현재 환경 정보를 표시하는 메뉴
        /// </summary>
        [MenuItem(MenuRoot + "Show Current Environment", false, 100)]
        private static void ShowCurrentEnvironment()
        {
            var settings = EnvironmentSettings.Instance;
            var env = settings.IsStaging ? "STAGING" : "PRODUCTION";
            var apiUrl = EnvironmentConfig.GetApiBaseUrl();
            var relayUrl = EnvironmentConfig.GetRelayUrl();
            
            var message = $"Current Environment: {env}\n\n" +
                         $"API Base URL:\n{apiUrl}\n\n" +
                         $"Relay URL:\n{relayUrl}\n\n" +
                         $"Chain Info API:\n{EnvironmentConfig.GetChainInfoApiUrl()}";
            
            EditorUtility.DisplayDialog("Cross SDK Environment", message, "OK");
        }
    }
}

