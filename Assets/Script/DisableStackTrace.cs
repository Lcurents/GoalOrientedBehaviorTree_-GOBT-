using UnityEngine;

/// <summary>
/// Disable stacktrace untuk Debug.Log() to make console cleaner
/// Attach ke any GameObject in scene atau auto-run via [RuntimeInitializeOnLoadMethod]
/// </summary>
public class DisableStackTrace
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void DisableLogStackTrace()
    {
        // Disable stacktrace for Debug.Log (keep for warnings/errors)
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        
        // Keep stacktrace for warnings (optional: set to None if too verbose)
        // Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
    }
}
