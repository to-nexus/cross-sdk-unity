using System;
using UnityEngine;
using ILogger = Cross.Core.Common.Logging.ILogger;

namespace Cross.Sign.Unity
{
    public class UnityLogger : ILogger
    {
        public void Log(string message) => Debug.Log(message);
        
        public void LogError(string message) => Debug.LogError(message);
        
        public void LogError(Exception e) => Debug.LogException(e);
    }
}