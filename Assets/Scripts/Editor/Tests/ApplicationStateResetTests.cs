using System.Collections.Generic;
using System.Reflection;
using BTV.Data;
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
        ApplicationState.ResetAllServices();
    }

    [TearDown]
    public void TearDown()
    {
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
        Assert.IsNull(TaskPerformanceService.ProcessedTriggers, "the previous patient's performance triggers must not survive a switch");
        Assert.IsNull(TaskPerformanceService.Colors, "trigger colors are patient-scoped alongside the processed triggers");
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
        int previousGeneration = GetMontageGeneration();

        EegFileService.Reset();
        bool published = InvokeMontagePublisher(
            "TryPublishNewMontage",
            previousGeneration,
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
        int previousGeneration = GetMontageGeneration();

        EegFileService.Reset();
        bool published = InvokeMontagePublisher(
            "TryPublishEditedMontage",
            previousGeneration,
            previousMontage,
            "After",
            new BtvProgram[6],
            new List<ChannelCorrespondance>());

        Assert.IsFalse(published);
        Assert.AreEqual("Before", previousMontage.Name, "a stale continuation must not mutate its old montage instance");
        Assert.AreEqual(1, EegFileService.Montages.Count);
    }

    private static void SetTaskPerformanceProperty(string propertyName, object value)
    {
        PropertyInfo property = typeof(TaskPerformanceService).GetProperty(propertyName);
        Assert.NotNull(property, "TaskPerformanceService property not found: " + propertyName);
        property.SetValue(null, value, null);
    }

    private static int GetMontageGeneration()
    {
        FieldInfo field = typeof(EegFileService).GetField("m_StateGeneration", StaticNonPublic);
        Assert.NotNull(field, "montage generation guard not found");
        return (int)field.GetValue(null);
    }

    private static bool InvokeMontagePublisher(string methodName, params object[] arguments)
    {
        MethodInfo method = typeof(EegFileService).GetMethod(methodName, StaticNonPublic);
        Assert.NotNull(method, "montage publisher not found: " + methodName);
        return (bool)method.Invoke(null, arguments);
    }
}
