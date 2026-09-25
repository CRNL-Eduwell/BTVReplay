using System.Collections.Generic;
using System.IO;
using BTV.Data;
using BTV.Services.EventsService;
using BTV.Services.UserPreferencesService;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the June §1.2 pattern one level below the patient base (review B-2): the
/// preference and event files used to load as empty on error and save non-atomically, so a
/// corrupt file loaded as "nothing" and the next save destroyed it. A failed read must now leave
/// both the file and the data already in memory alone, and a failed write must be reported.
/// </summary>
public class DataFileSafetyTests
{
    private string m_TempDir;
    private string m_SavedPreferencesPath;
    private UserPreferences m_SavedPreferences;

    [SetUp]
    public void SetUp()
    {
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-datafile-tests-" + Path.GetRandomFileName());
        Directory.CreateDirectory(m_TempDir);
        m_SavedPreferencesPath = UserPreferencesService.PATH;
        m_SavedPreferences = UserPreferencesService.UserPreferences;
    }

    [TearDown]
    public void TearDown()
    {
        // Point the service back at the real file and state; LoadError resets on the next load.
        UserPreferencesService.PATH = m_SavedPreferencesPath;
        UserPreferencesService.UserPreferences = m_SavedPreferences;
        EventsService.Reset();
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    private string TempPath(string name) => Path.Combine(m_TempDir, name);

    #region Preferences
    [Test]
    public void Preferences_CorruptFile_RunsOnDefaultsAndRefusesToSaveOverIt()
    {
        string path = TempPath("Preferences.txt");
        const string corrupt = "{ \"DatabasePreferences\": { \"PathRoots\": [ { \"Name\": \"DATA\", ";
        File.WriteAllText(path, corrupt);
        UserPreferencesService.PATH = path;

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("could not read"));
        UserPreferencesService.LoadPreferences();

        Assert.IsNotNull(UserPreferencesService.LoadError);
        Assert.IsNotNull(UserPreferencesService.UserPreferences, "the app must keep running on defaults");
        Assert.IsFalse(UserPreferencesService.SavePreferences(out string error));
        StringAssert.Contains(path, error, "the refusal must name the file");
        Assert.AreEqual(corrupt, File.ReadAllText(path), "the unreadable file must be left for recovery");
    }

    [Test]
    public void Preferences_EmptyFile_IsTreatedAsUnreadable()
    {
        string path = TempPath("Preferences.txt");
        File.WriteAllText(path, "");
        UserPreferencesService.PATH = path;

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("could not read"));
        UserPreferencesService.LoadPreferences();

        Assert.IsNotNull(UserPreferencesService.LoadError);
        Assert.IsFalse(UserPreferencesService.SavePreferences(out _));
    }

    [Test]
    public void Preferences_MissingFile_LoadsDefaultsAndSaves()
    {
        string path = TempPath("Preferences.txt");
        UserPreferencesService.PATH = path;

        UserPreferencesService.LoadPreferences();

        Assert.IsNull(UserPreferencesService.LoadError, "a first run is not a failure");
        Assert.IsTrue(UserPreferencesService.SavePreferences(out _));
        Assert.IsTrue(File.Exists(path));
    }

    [Test]
    public void Preferences_SaveThenLoad_RoundTripsPathRoots()
    {
        string path = TempPath("Preferences.txt");
        UserPreferencesService.PATH = path;
        UserPreferencesService.LoadPreferences();
        UserPreferencesService.UserPreferences.DatabasePreferences.PathRoots = new List<PathRoot> { new PathRoot("DATA", "/Volumes/eeg") };

        Assert.IsTrue(UserPreferencesService.SavePreferences(out _));
        UserPreferencesService.UserPreferences = null;
        UserPreferencesService.LoadPreferences();

        Assert.IsNull(UserPreferencesService.LoadError);
        List<PathRoot> roots = UserPreferencesService.UserPreferences.DatabasePreferences.PathRoots;
        Assert.AreEqual(1, roots.Count);
        Assert.AreEqual("DATA", roots[0].Name);
        Assert.AreEqual("/Volumes/eeg", roots[0].Path);
        Assert.IsFalse(File.Exists(path + ".tmp"));
    }
    #endregion

    #region Events
    [Test]
    public void Events_UnreadableBtv_ThrowsAndKeepsTheEventsAlreadyLoaded()
    {
        EventsService.AddEvent(new BtvEvent(7, 1500f, 0, "", "", "kept"));
        string path = TempPath("markings.btv");
        File.WriteAllText(path, "this is not an events file\nat all\n");

        Assert.Throws<InvalidDataException>(() => EventsService.Load(path));

        Assert.AreEqual(1, EventsService.Events.Count, "a failed load must not wipe the current markings");
        Assert.AreEqual("kept", EventsService.Events[0].Comment);
    }

    [Test]
    public void Events_UnreadablePos_Throws()
    {
        string path = TempPath("markings.pos");
        File.WriteAllText(path, "garbage\n");

        Assert.Throws<InvalidDataException>(() => EventsService.LoadEventsFromFile(path, 1000));
    }

    [Test]
    public void Events_EmptyBtv_LoadsAsNoEvents()
    {
        EventsService.AddEvent(new BtvEvent(7, 1500f, 0, "", "", ""));
        string path = TempPath("empty.btv");
        File.WriteAllText(path, "");

        EventsService.Load(path);

        Assert.AreEqual(0, EventsService.Events.Count, "an empty file is a legitimate empty event list");
    }

    [Test]
    public void Events_BtvSaveThenLoad_RoundTrips()
    {
        EventsService.AddEvent(new BtvEvent(12, 61250f, 300, "A1", "B2", "seizure onset"));
        EventsService.AddEvent(new BtvEvent(3, 1000f, 0, "", "", ""));
        string path = TempPath("markings.btv");

        EventsService.SaveEvents(path);
        EventsService.Reset();
        EventsService.Load(path);

        Assert.AreEqual(2, EventsService.Events.Count);
        BtvEvent onset = EventsService.Events.Find(e => e.Code == 12);
        Assert.IsNotNull(onset);
        Assert.AreEqual(61250f, onset.TimeInMilliSeconds, 0.5f);
        Assert.AreEqual(300, onset.Duration);
        Assert.AreEqual("A1", onset.SiteOfInterest);
        Assert.AreEqual("B2", onset.SecondSiteOfInterest);
        Assert.AreEqual("seizure onset", onset.Comment);
    }

    [Test]
    public void Events_PosSaveThenLoad_RoundTrips()
    {
        EventsService.AddEvent(new BtvEvent(12, 2500f, 0, "", "", ""));
        string path = TempPath("markings.pos");

        EventsService.SaveEvents(path, 1000);
        List<BtvEvent> loaded = EventsService.LoadEventsFromFile(path, 1000);

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual(12, loaded[0].Code);
        Assert.AreEqual(2500f, loaded[0].TimeInMilliSeconds, 1f);
    }

    [Test]
    public void Events_SaveToUnwritablePath_ThrowsInsteadOfFailingSilently()
    {
        EventsService.AddEvent(new BtvEvent(1, 100f, 0, "", "", ""));
        string path = Path.Combine(m_TempDir, "missing-folder", "markings.btv");

        Assert.That(() => EventsService.SaveEvents(path), Throws.InstanceOf<IOException>());
    }

    [Test]
    public void Events_SaveOverExistingFile_ReplacesItAndLeavesNoTempFile()
    {
        string path = TempPath("markings.btv");
        EventsService.AddEvent(new BtvEvent(1, 100f, 0, "", "", "first"));
        EventsService.SaveEvents(path);
        EventsService.Reset();
        EventsService.AddEvent(new BtvEvent(2, 200f, 0, "", "", "second"));

        EventsService.SaveEvents(path);
        EventsService.Reset();
        EventsService.Load(path);

        Assert.AreEqual(1, EventsService.Events.Count);
        Assert.AreEqual("second", EventsService.Events[0].Comment);
        Assert.IsFalse(File.Exists(path + ".tmp"));
    }
    #endregion
}
