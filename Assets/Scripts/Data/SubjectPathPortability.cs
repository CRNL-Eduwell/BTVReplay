using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Walks every stored file path of a subject list to expand or tokenize portable paths (see
/// PathTokens). Used exclusively by the persistence layer: ExpandTokens after deserializing a
/// base, TokenizedCopy before serializing one - so the in-memory model only ever holds
/// absolute paths and nothing else in the application changes.
/// </summary>
public static class SubjectPathPortability
{
    public static string AppConfigPath { get { return UnityEngine.Application.dataPath + "/Config"; } }

    private static IReadOnlyList<PathRoot> ConfiguredRoots
    {
        get
        {
            // The preferences are not loaded yet on some early paths (and in edit-mode tests);
            // fall back to "no named roots" - ${APPCONFIG} still resolves. Roots live with the
            // database preferences (configured in the patient-base manager's options).
            UserPreferences preferences = BTV.Services.UserPreferencesService.UserPreferencesService.UserPreferences;
            return preferences != null ? preferences.DatabasePreferences.PathRoots : null;
        }
    }

    /// <summary>
    /// Expands "${NAME}/..." tokens to absolute paths, in place. Call right after a base has
    /// been deserialized.
    /// </summary>
    public static void ExpandTokens(List<Subject> subjects)
    {
        ExpandTokens(subjects, ConfiguredRoots, AppConfigPath);
    }

    public static void ExpandTokens(List<Subject> subjects, IReadOnlyList<PathRoot> roots, string appConfigPath)
    {
        Transform(subjects, path => PathTokens.Resolve(path, roots, appConfigPath));
    }

    /// <summary>
    /// Returns a deep copy of the subjects with every path under a known root rewritten to its
    /// "${NAME}/..." form, ready to be serialized. The input subjects are never mutated - the
    /// application keeps working on absolute paths after a save.
    /// </summary>
    public static List<Subject> TokenizedCopy(List<Subject> subjects)
    {
        return TokenizedCopy(subjects, ConfiguredRoots, AppConfigPath);
    }

    public static List<Subject> TokenizedCopy(List<Subject> subjects, IReadOnlyList<PathRoot> roots, string appConfigPath)
    {
        List<Subject> copies = subjects.ConvertAll(DeepCopy);
        Transform(copies, path => PathTokens.Tokenize(path, roots, appConfigPath));
        return copies;
    }

    private static Subject DeepCopy(Subject subject)
    {
        Subject copy = new Subject
        {
            PatientName = subject.PatientName,
            AnatomicalSpaces = subject.AnatomicalSpaces.ToDictionary(entry => entry.Key, entry => new BrainDataContainer(entry.Value))
        };
        foreach (Experiment experiment in subject.Experiments)
        {
            Dictionary<string, IEegFileInfo> files = experiment.Files.ToDictionary(entry => entry.Key, entry => (IEegFileInfo)entry.Value.Clone());
            copy.Experiments.Add(new Experiment(experiment.Label, files, experiment.Video));
        }
        return copy;
    }

    private static void Transform(List<Subject> subjects, Func<string, string> transform)
    {
        foreach (Subject subject in subjects)
        {
            foreach (BrainDataContainer container in subject.AnatomicalSpaces.Values)
            {
                container.LeftHemisphere = transform(container.LeftHemisphere);
                container.RightHemisphere = transform(container.RightHemisphere);
                container.Transformation = transform(container.Transformation);
                container.Pts = transform(container.Pts);
                container.Atlas = transform(container.Atlas);
            }
            foreach (Experiment experiment in subject.Experiments)
            {
                experiment.Video = transform(experiment.Video);
                foreach (IEegFileInfo fileInfo in experiment.Files.Values)
                {
                    fileInfo.TransformPaths(transform);
                }
            }
        }
    }
}
