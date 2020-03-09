using System.Collections.Generic;

public class Subject
{
    public string PatientName { get; set; } = "";
    public Dictionary<string, BrainDataContainer> AnatomicalSpaces { get; set; } = new Dictionary<string, BrainDataContainer>();
    public Dictionary<string, IEegFileInfo> Files { get; set; } = new Dictionary<string, IEegFileInfo>();
    public string Video { get; set; } = "";

    public Subject()
    {

    }

    public Subject(Subject subjectToCopy)
    {
        PatientName = subjectToCopy.PatientName;
        AnatomicalSpaces = new Dictionary<string, BrainDataContainer>(subjectToCopy.AnatomicalSpaces);
        Files = new Dictionary<string, IEegFileInfo>(subjectToCopy.Files);
        Video = subjectToCopy.Video;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">"MNI" or "PAT" for now</param>
    /// <returns></returns>
    public bool HasAllInformationForSpace(string key)
    {
        if (AnatomicalSpaces.ContainsKey(key))
        {
            bool retrieved = AnatomicalSpaces.TryGetValue(key, out BrainDataContainer value);
            return retrieved ? value.HasAnat : false;
        }
        return false;
    }
}
