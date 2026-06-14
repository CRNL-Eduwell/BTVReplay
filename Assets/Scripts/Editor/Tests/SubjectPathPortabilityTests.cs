using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Edit-mode tests for the subject path walker used by the persistence layer. The contract:
/// TokenizedCopy rewrites every stored path kind (anatomy, video, every EEG file format) into
/// its portable form WITHOUT mutating the in-memory subjects - the application keeps working
/// on absolute paths after a save - and ExpandTokens restores absolute paths at load.
/// </summary>
public class SubjectPathPortabilityTests
{
    private static readonly List<PathRoot> TestRoots = new List<PathRoot> { new PathRoot("DATA", "/data") };
    private const string AppConfig = "/app/Config";

    private static Subject MakeSubject()
    {
        Subject subject = new Subject
        {
            PatientName = "PAT1",
            AnatomicalSpaces = new Dictionary<string, BrainDataContainer>
            {
                ["MNI"] = new BrainDataContainer
                {
                    LeftHemisphere = "/app/Config/Data/MNI/Lhemi.tri",
                    RightHemisphere = "/app/Config/Data/MNI/Rhemi.tri",
                    Transformation = "/app/Config/Data/MNI/transfo.trm",
                    Pts = "/data/anat/PAT1.pts",
                    Atlas = ""
                }
            }
        };
        Dictionary<string, IEegFileInfo> files = new Dictionary<string, IEegFileInfo>
        {
            ["EEG1"] = new ElanFileInfo("/data/eeg/PAT1.eeg", "/data/eeg/PAT1.pos", ""),
            ["EEG2"] = new MicromedFileInfo("/data/eeg/PAT1.TRC")
        };
        subject.Experiments.Add(new Experiment("Exp", files, "/data/video/PAT1.mp4"));
        return subject;
    }

    [Test]
    public void TokenizedCopy_RewritesEveryPathKind()
    {
        List<Subject> copy = SubjectPathPortability.TokenizedCopy(new List<Subject> { MakeSubject() }, TestRoots, AppConfig);

        BrainDataContainer anat = copy[0].AnatomicalSpaces["MNI"];
        Assert.AreEqual("${APPCONFIG}/Data/MNI/Lhemi.tri", anat.LeftHemisphere);
        Assert.AreEqual("${DATA}/anat/PAT1.pts", anat.Pts);
        Assert.AreEqual("", anat.Atlas, "empty paths stay empty");

        Experiment experiment = copy[0].Experiments[0];
        Assert.AreEqual("${DATA}/video/PAT1.mp4", experiment.Video);
        Assert.AreEqual("${DATA}/eeg/PAT1.eeg", ((ElanFileInfo)experiment.Files["EEG1"]).Eeg);
        Assert.AreEqual("${DATA}/eeg/PAT1.pos", ((ElanFileInfo)experiment.Files["EEG1"]).Pos);
        Assert.AreEqual("${DATA}/eeg/PAT1.TRC", ((MicromedFileInfo)experiment.Files["EEG2"]).Trc);
    }

    [Test]
    public void TokenizedCopy_NeverMutatesTheSourceSubjects()
    {
        List<Subject> source = new List<Subject> { MakeSubject() };

        SubjectPathPortability.TokenizedCopy(source, TestRoots, AppConfig);

        Assert.AreEqual("/app/Config/Data/MNI/Lhemi.tri", source[0].AnatomicalSpaces["MNI"].LeftHemisphere);
        Assert.AreEqual("/data/video/PAT1.mp4", source[0].Experiments[0].Video);
        Assert.AreEqual("/data/eeg/PAT1.eeg", ((ElanFileInfo)source[0].Experiments[0].Files["EEG1"]).Eeg);
    }

    [Test]
    public void ExpandTokens_RestoresAbsolutePaths_RoundTrippingWithTokenizedCopy()
    {
        char s = System.IO.Path.DirectorySeparatorChar;
        List<Subject> tokenized = SubjectPathPortability.TokenizedCopy(new List<Subject> { MakeSubject() }, TestRoots, AppConfig);

        SubjectPathPortability.ExpandTokens(tokenized, TestRoots, AppConfig);

        Assert.AreEqual("/data/eeg/PAT1.eeg".Replace('/', s), ((ElanFileInfo)tokenized[0].Experiments[0].Files["EEG1"]).Eeg);
        Assert.AreEqual("/app/Config/Data/MNI/Lhemi.tri".Replace('/', s), tokenized[0].AnatomicalSpaces["MNI"].LeftHemisphere);
        Assert.AreEqual("/data/video/PAT1.mp4".Replace('/', s), tokenized[0].Experiments[0].Video);
    }

    [Test]
    public void ExpandTokens_LeavesLegacyAbsoluteBasesUntouched()
    {
        List<Subject> subjects = new List<Subject> { MakeSubject() };

        SubjectPathPortability.ExpandTokens(subjects, TestRoots, AppConfig);

        Assert.AreEqual("/data/eeg/PAT1.eeg", ((ElanFileInfo)subjects[0].Experiments[0].Files["EEG1"]).Eeg);
    }
}
