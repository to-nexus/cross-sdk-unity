using System;

namespace Cross.Core.Common.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class VersionMarkerAttribute : Attribute 
    {
    }
}