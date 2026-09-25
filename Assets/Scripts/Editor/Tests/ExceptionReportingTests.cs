using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for how caught exceptions reach the user (review B-5). GlobalExceptionManager
/// opens the bug reporter on every LogType.Exception, so Debug.LogException inside a catch that
/// already shows a dialog used to put the bug reporter on top of it, which trains users to
/// dismiss it. Handled failures log through BtvLog.Handled; LogException stays for the few places
/// that have no other way to surface a failure yet.
/// </summary>
public class ExceptionReportingTests
{
    // Files allowed to call Debug.LogException, each for a stated reason. Adding one should be a
    // deliberate choice: it means that failure opens the bug reporter.
    private static readonly string[] s_LogExceptionAllowList =
    {
        "Messenger/Messenger.cs",          // a handler threw unexpectedly: a genuine bug to report
        "Data/Files/DBFile2.cs",           // the patient-base save/load paths below return false or
        "Data/Files/DBFile3.cs",           // leave the repository unloaded, but show no dialog of
        "Data/Files/WorkspaceFile.cs",     // their own yet, so the bug reporter is still their
        "Services/SubjectRepository.cs",   // only visible surface
        "Services/DatabaseService.cs",
    };

    [Test]
    public void Handled_LogsAnErrorWithTheStackTrace_NotAnException()
    {
        System.Exception caught;
        try { throw new IOException("disk unplugged"); }
        catch (System.Exception e) { caught = e; }

        LogAssert.Expect(LogType.Error, new Regex(@"(?s)Saving events failed\n.*IOException: disk unplugged.*ExceptionReportingTests"));
        BtvLog.Handled("Saving events failed", caught);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void LogException_IsOnlyUsedWhereTheBugReporterIsTheIntendedSurface()
    {
        string scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
        Regex logException = new Regex(@"\bDebug\.LogException\s*\(");

        string[] violations = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => new { path, relative = path.Replace(scriptsRoot + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/') })
            .Where(file => !file.relative.StartsWith("Editor/") && !s_LogExceptionAllowList.Contains(file.relative))
            .SelectMany(file => File.ReadLines(file.path)
                .Select((line, index) => new { file.relative, line, lineNumber = index + 1 }))
            .Where(entry => !entry.line.TrimStart().StartsWith("//"))
            .Where(entry => logException.IsMatch(entry.line))
            .Select(entry => entry.relative + ":" + entry.lineNumber)
            .ToArray();

        CollectionAssert.IsEmpty(violations,
            "a caught exception that is shown to the user must use BtvLog.Handled; Debug.LogException opens the bug reporter");
    }
}
