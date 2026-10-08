using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BTV.Services.ProtocolService;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the .prov reader (June review §2, "loaders swallow exceptions"). A protocol
/// that failed to parse used to be reported through Console.WriteLine only, which Unity never shows,
/// and was still offered in the Protocol events dropdown with the blocs read before the bad line.
/// </summary>
public class ProtocolLoadTests
{
    private const string GoodProv =
        "ROW;COL;Name;Path;Window;BaseLine;Main Event;Maint Event Label;Secondary Events;Secondary Events Label;Sort\n" +
        "1;1;OK;/Config/Pictures/MARD/OK.png;-200:3000;-150:-50;10;OK;100;ok;C0_L1\n" +
        "NO_CHANGE_CODE\n";

    private string m_TempDir;

    [SetUp]
    public void SetUp()
    {
        // No dot in the folder name: Protocol takes its short name from the last '.'-separated part.
        m_TempDir = Path.Combine(Path.GetTempPath(), "btv-prov-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(m_TempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TempDir))
            Directory.Delete(m_TempDir, true);
    }

    [Test]
    public void UnreadableProtocol_IsLoggedAndLeftOut()
    {
        string good = Path.Combine(m_TempDir, "GOOD.prov");
        string bad = Path.Combine(m_TempDir, "BAD.prov");
        File.WriteAllText(good, GoodProv);
        File.WriteAllText(bad, GoodProv.Replace("1;1;OK", "1;1;OK\nx;1;NOK"));

        LogAssert.Expect(LogType.Error, new Regex("could not be read: .*BAD\\.prov"));
        List<Protocol> protocols = ProtocolService.LoadProtocols(new[] { good, bad });

        Assert.AreEqual(new[] { "GOOD" }, protocols.Select(p => p.ShortName).ToArray());
        Assert.AreEqual(1, protocols[0].Blocs.Count);
    }

    [Test]
    public void MissingProtocol_IsLoggedAndLeftOut()
    {
        string missing = Path.Combine(m_TempDir, "GONE.prov");

        LogAssert.Expect(LogType.Error, new Regex("does not exist: .*GONE\\.prov"));
        List<Protocol> protocols = ProtocolService.LoadProtocols(new[] { missing });

        Assert.IsEmpty(protocols);
    }

    [Test]
    public void BundledProtocols_AllLoad()
    {
        string[] paths = Directory.GetFiles(Path.Combine(Application.dataPath, "Config/Prov"), "*.prov");
        Assert.That(paths, Is.Not.Empty);

        List<Protocol> protocols = ProtocolService.LoadProtocols(paths);

        LogAssert.NoUnexpectedReceived();
        Assert.AreEqual(paths.Length, protocols.Count);
        Assert.That(protocols.All(p => p.Blocs.Count > 0), "every bundled protocol has at least one bloc");
    }
}
