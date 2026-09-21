using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BTV.Data;
using BTV.Services;
using BTV.Services.EegFileService;
using BTV.Services.TaskPerformanceService;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Regression coverage for patient switches. Every patient-scoped value must be cleared, and
/// asynchronous montage work started before the reset must not publish into the new session.
/// </summary>
public class ApplicationStateResetTests
{
    private const BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        Session.ReplaceCurrent();
        ApplicationState.ResetAllServices();
    }

    [TearDown]
    public void TearDown()
    {
        Session.ReplaceCurrent();
        ApplicationState.ResetAllServices();
    }

    [Test]
    public void ResetAllServices_ClearsPatientScopedAnalysisState()
    {
        TimeFrequencyService.BaselineEvent = new BtvEvent(12, 345f);
        SetTaskPerformanceProperty("ProcessedTriggers", new List<EegTrigger>
        {
            new EegTrigger(new EegEvent(21, 100f))
        });
        SetTaskPerformanceProperty("Colors", new List<Color> { Color.red });

        ApplicationState.ResetAllServices();

        Assert.IsNull(TimeFrequencyService.BaselineEvent, "the previous patient's TF baseline must not survive a switch");
        Assert.IsEmpty(TaskPerformanceService.ProcessedTriggers, "the previous patient's performance triggers must not survive a switch");
        Assert.IsEmpty(TaskPerformanceService.Colors, "trigger colors are patient-scoped alongside the processed triggers");
    }

    [Test]
    public void SessionReplacement_ReplacesAndDisposesThePatientSession()
    {
        Session previous = Session.Current;

        Session.ReplaceCurrent();

        Assert.AreNotSame(previous, Session.Current);
        Assert.IsTrue(previous.IsDisposed);
    }

    [Test]
    public void SessionReplacement_LeavesDisposedCollectionsSafeToRead()
    {
        Session previous = Session.Current;
        BTV.Services.EventsService.EventsService.Events.Add(new BtvEvent(1, 100f));
        SetTaskPerformanceProperty("ProcessedTriggers", new List<EegTrigger>());
        SetTaskPerformanceProperty("Colors", new List<Color>());

        Session.ReplaceCurrent();

        Assert.AreEqual(0, BTV.Services.EventsService.EventsService.GetEventCount(previous));
        Assert.AreEqual(0, TaskPerformanceService.GetProcessedTriggers(previous).Count);
        Assert.AreEqual(0, TaskPerformanceService.GetColors(previous).Count);
    }

    [Test]
    public void EegReset_RestoresTheDefaultMontageSelection()
    {
        EegFileService.SelectedMontageID = 4;

        EegFileService.Reset();

        Assert.AreEqual(0, EegFileService.SelectedMontageID);
        Assert.AreEqual(1, EegFileService.Montages.Count);
        Assert.DoesNotThrow(() => { BtvMontage current = EegFileService.CurrentMontage; });
    }

    [Test]
    public void EegReset_DiscardsANewMontageCompletedForThePreviousPatient()
    {
        Session previousSession = Session.Current;

        Session.ReplaceCurrent();
        ApplicationState.ResetAllServices();
        bool published = InvokeMontagePublisher(
            "TryPublishNewMontage",
            previousSession,
            "Late montage",
            new BtvProgram[6],
            new List<ChannelCorrespondance>());

        Assert.IsFalse(published);
        Assert.AreEqual(1, EegFileService.Montages.Count, "late work must not enter the new patient's montage list");
        Assert.AreEqual("Default", EegFileService.Montages[0].Name);
    }

    [Test]
    public void EegReset_DiscardsAMontageEditCompletedForThePreviousPatient()
    {
        BtvMontage previousMontage = new BtvMontage("Before", new BtvProgram[6]);
        EegFileService.Montages.Add(previousMontage);
        Session previousSession = Session.Current;

        Session.ReplaceCurrent();
        ApplicationState.ResetAllServices();
        // Keep the old object reachable so only the stale session can reject publication.
        // Without the identity check, this continuation would now mutate the object.
        EegFileService.Montages.Add(previousMontage);
        bool published = InvokeMontagePublisher(
            "TryPublishEditedMontage",
            previousSession,
            previousMontage,
            "After",
            new BtvProgram[6],
            new List<ChannelCorrespondance>());

        Assert.IsFalse(published);
        Assert.AreEqual("Before", previousMontage.Name, "a stale continuation must not mutate its old montage instance");
        Assert.AreEqual(2, EegFileService.Montages.Count);
    }

    [Test]
    public void LoadingCompletion_IgnoresACircleDestroyedByThePatientSwitch()
    {
        GameObject loadingObject = new GameObject("LoadingCircle from previous scene");
        LoadingCircle destroyedCircle = loadingObject.AddComponent<LoadingCircle>();
        UnityEngine.Object.DestroyImmediate(loadingObject);

        MethodInfo method = typeof(LoadingManager).GetMethod("CloseIfAlive", StaticNonPublic);
        Assert.NotNull(method, "LoadingManager close guard not found");
        Assert.DoesNotThrow(() => method.Invoke(null, new object[] { destroyedCircle }));
    }

    [Test]
    public void PatientServices_KeepNoMutableStaticStateOutsideSession()
    {
        Type[] patientServices =
        {
            typeof(BTV.Services.SubjectInfoService.SubjectInfoService),
            typeof(EegFileService),
            typeof(BTV.Services.EventsService.EventsService),
            typeof(TracesService),
            typeof(TimeFrequencyService),
            typeof(BTV.Services.AnatomicalDataService.AnatomicalDataService),
            typeof(BTV.Services.VideoService.VideoService),
            typeof(TaskPerformanceService),
            typeof(BTV.Services.CodeMatchingService.CodeMatchingService)
        };

        string[] mutableStaticFields = patientServices
            .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(field => !field.IsLiteral && !field.IsInitOnly)
            .Select(field => field.DeclaringType.Name + "." + field.Name)
            .ToArray();

        CollectionAssert.IsEmpty(mutableStaticFields,
            "patient-owned mutable fields belong on Session; service APIs should only forward to Session.Current");
    }

    [Test]
    public void RuntimeModules_DoNotReachIntoSessionStateDirectly()
    {
        string scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
        Regex directSessionAccess = new Regex(@"\b(?:PatientSession|m_Session)\s*\.");

        string[] violations = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsUnder(path, "Services") && !IsUnder(path, "Editor"))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new { path, line, lineNumber = index + 1 }))
            .Where(entry => directSessionAccess.IsMatch(entry.line))
            .Select(entry => entry.path.Replace(scriptsRoot + Path.DirectorySeparatorChar, "") + ":" + entry.lineNumber)
            .ToArray();

        CollectionAssert.IsEmpty(violations,
            "runtime modules must use session-aware service APIs instead of Session's internal state");
    }

    [Test]
    public void SessionAwareServiceOverloads_UseTheInjectedSessionInsteadOfCurrent()
    {
        Session detachedSession = new Session();
        try
        {
            Dictionary<int, TraceOption> traceOptions =
                (Dictionary<int, TraceOption>)GetSessionProperty(detachedSession, "TraceOptions");
            TraceOption detachedOption = new TraceOption(null, Color.red);
            traceOptions.Add(0, detachedOption);

            List<BtvEvent> events = (List<BtvEvent>)GetSessionProperty(detachedSession, "Events");
            BtvEvent detachedEvent = new BtvEvent(10, 250f) { Correlation = new[] { 0.5f } };
            events.Add(detachedEvent);

            Assert.AreSame(detachedOption, TracesService.GetOptionsFor(detachedSession, 0));
            Assert.AreSame(
                ((List<BtvMontage>)GetSessionProperty(detachedSession, "Montages"))[0],
                EegFileService.GetCurrentMontage(detachedSession));

            BTV.Services.EventsService.EventsService.ClearCorrelations(detachedSession);
            Assert.IsNull(detachedEvent.Correlation);
        }
        finally
        {
            detachedSession.Dispose();
        }
    }

    [Test]
    public void SessionReplacement_DiscardsPendingEegAndAudioPublications()
    {
        Session previousSession = Session.Current;

        Session.ReplaceCurrent();
        ApplicationState.ResetAllServices();

        Assert.IsFalse(InvokePrivateBool(typeof(EegFileService), "TryPublishEegFile", previousSession, null, 0));
        Assert.IsFalse(InvokePrivateBool(typeof(BTV.Services.VideoService.VideoService), "TryPublishRawAudio", previousSession, null));
        Assert.IsFalse(InvokePrivateBool(typeof(BTV.Services.VideoService.VideoService), "TryPublishProcessedAudio", previousSession, null, "loaded"));
        Assert.IsNull(EegFileService.DefaultMontage.EegFiles[0]);
        Assert.IsFalse(BTV.Services.VideoService.VideoService.FilteredDataLoaded);
        Assert.IsNull(BTV.Services.VideoService.VideoService.GetAudioContainer());
    }

    [Test]
    public void SessionReplacement_DropsAudioSubscribersFromThePreviousScene()
    {
        BTV.Services.VideoService.AudioDataLoaded handler = () => { };
        BTV.Services.VideoService.VideoService.AudioDataLoaded += handler;
        Session previousSession = Session.Current;

        Session.ReplaceCurrent();

        PropertyInfo handlersProperty = typeof(Session).GetProperty(
            "AudioDataLoadedHandlers",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handlersProperty);
        Assert.IsNull(handlersProperty.GetValue(previousSession, null));
    }

    [Test]
    public void ResetBroadcasts_PreserveAudioSubscriberRegisteredByTheNewScene()
    {
        Session.ReplaceCurrent();
        Session newSession = Session.Current;
        bool audioLoaded = false;
        BTV.Services.VideoService.AudioDataLoaded handler = () => audioLoaded = true;
        BTV.Services.VideoService.VideoService.AudioDataLoaded += handler;

        ApplicationState.ResetAllServices();
        Assert.AreSame(newSession, Session.Current, "reset broadcasts must not replace the session created before scene load");
        bool published = InvokePrivateBool(
            typeof(BTV.Services.VideoService.VideoService),
            "TryPublishProcessedAudio",
            newSession,
            null,
            "loaded");

        Assert.IsTrue(published);
        Assert.IsTrue(audioLoaded, "the replacement scene's pre-Update subscriber must survive reset broadcasts");
    }

    private static void SetTaskPerformanceProperty(string propertyName, object value)
    {
        PropertyInfo property = typeof(TaskPerformanceService).GetProperty(propertyName);
        Assert.NotNull(property, "TaskPerformanceService property not found: " + propertyName);
        property.SetValue(null, value, null);
    }

    private static object GetSessionProperty(Session session, string propertyName)
    {
        PropertyInfo property = typeof(Session).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(property, "Session property not found: " + propertyName);
        return property.GetValue(session, null);
    }

    private static bool IsUnder(string path, string directoryName)
    {
        string marker = Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar;
        return path.Contains(marker);
    }

    private static bool InvokeMontagePublisher(string methodName, params object[] arguments)
    {
        return InvokePrivateBool(typeof(EegFileService), methodName, arguments);
    }

    private static bool InvokePrivateBool(Type type, string methodName, params object[] arguments)
    {
        MethodInfo method = type.GetMethod(methodName, StaticNonPublic);
        Assert.NotNull(method, "session publisher not found: " + type.Name + "." + methodName);
        return (bool)method.Invoke(null, arguments);
    }
}
