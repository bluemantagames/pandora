using System.Diagnostics;
using UnityEngine.Profiling;

public static class Logger
{
    private static CustomSampler _debugSampler = null, _warningSampler = null;
    
    private static CustomSampler debugSampler {
        get {
            if (_debugSampler == null) {
                _debugSampler = CustomSampler.Create("Debug.Log");
            }

            return _debugSampler;
        }
    }

    private static CustomSampler warningSampler {
        get {
            if (_warningSampler == null) {
                _warningSampler = CustomSampler.Create("Debug.LogWarning");
            }

            return _warningSampler;
        }
    }


    public static void Debug(string logMsg)
    {
        UnityEngine.Debug.Log(logMsg);
    }

    public static void DebugWarning(string logMsg)
    {
        UnityEngine.Debug.LogWarning(logMsg);
    }

}