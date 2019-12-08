using System.Diagnostics;

public static class Logger
{

    [Conditional("UNITY_EDITOR")]
    public static void Debug(string logMsg)
    {
        UnityEngine.Debug.Log(logMsg);

    }

    [Conditional("UNITY_EDITOR")]
    public static void DebugWarning(string logMsg)
    {
        UnityEngine.Debug.LogWarning(logMsg);
    }

}