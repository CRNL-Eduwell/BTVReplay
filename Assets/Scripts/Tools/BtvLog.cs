using System.Diagnostics;

/// <summary>
/// Thin logging wrapper for non-error diagnostics. The methods are marked [Conditional], so the
/// calls (and their argument evaluation) are compiled OUT of release standalone builds, keeping
/// them only in the editor and development builds. This removes the per-frame Debug.Log spam
/// from shipped builds without losing diagnostics during development.
///
/// Use BtvLog.Log for informational logging; keep UnityEngine.Debug.LogWarning / LogError /
/// LogException for problems that must always be visible.
///
/// Exceptions: Debug.LogException opens the bug reporter (GlobalExceptionManager), so reserve it
/// for failures nobody anticipated. An exception that is caught and already reported to the user
/// (a dialog) or deliberately recovered from goes through BtvLog.Handled instead.
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

    /// <summary>
    /// Logs a caught exception with its stack trace as an error, in every build, without opening
    /// the bug reporter: for failures the user has already been told about. Not [Conditional].
    /// </summary>
    public static void Handled(string context, System.Exception exception)
    {
        UnityEngine.Debug.LogError(context + "\n" + exception);
    }
}
