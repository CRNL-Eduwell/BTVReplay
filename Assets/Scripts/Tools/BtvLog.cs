using System.Diagnostics;

/// <summary>
/// Thin logging wrapper for non-error diagnostics. The methods are marked [Conditional], so the
/// calls (and their argument evaluation) are compiled OUT of release standalone builds, keeping
/// them only in the editor and development builds. This removes the per-frame Debug.Log spam
/// from shipped builds without losing diagnostics during development.
///
/// Use BtvLog.Log for informational logging; keep UnityEngine.Debug.LogWarning / LogError /
/// LogException for problems that must always be visible.
/// </summary>
public static class BtvLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }
}
