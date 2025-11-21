using System;

namespace Cross.Core.Common.Logging
{
    public class CrossLogger
    {
        public static ILogger Instance;

        public static ILogger WithContext(string context)
        {
            return new WrapperLogger(Instance, context);
        }

        public static void Log(string message)
        {
            if (Instance == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"[CrossSdk] {message}");
#endif
                return;
            }

            Instance.Log(message);
        }

        public static void LogError(string message)
        {
            if (Instance == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError($"[CrossSdk] {message}");
#endif
                return;
            }

            Instance.LogError(message);
        }

        public static void LogError(Exception e)
        {
            if (Instance == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogException(e);
#endif
                return;
            }

            Instance.LogError(e);
        }
    }
}