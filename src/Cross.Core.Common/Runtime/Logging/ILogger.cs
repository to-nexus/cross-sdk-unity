﻿using System;

namespace Cross.Core.Common.Logging
{
    public interface ILogger
    {
        void Log(string message);

        void LogError(string message);

        void LogError(Exception e);
    }
}