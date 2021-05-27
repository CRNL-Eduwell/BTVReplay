using UnityEngine;
using UnityEditor;

public class UserPreferences
{
    public GeneralPreferences GeneralPreferences { get; set; } = null;
    public DbPreferences DatabasePreferences { get; set; } = null;

    public UserPreferences()
    {
        GeneralPreferences = new GeneralPreferences();
        DatabasePreferences = new DbPreferences();
    }

    public UserPreferences(GeneralPreferences generalPreferences, DbPreferences dbPreferences)
    {
        GeneralPreferences = new GeneralPreferences(generalPreferences);
        DatabasePreferences = new DbPreferences(dbPreferences);
    }

    public UserPreferences(UserPreferences preferences)
    {
        GeneralPreferences = new GeneralPreferences(preferences.GeneralPreferences);
        DatabasePreferences = new DbPreferences(preferences.DatabasePreferences);
    }

    //#region operators
    //public override bool Equals(object obj)
    //{
    //    if (obj is Subject subject)
    //    {
    //        bool sameName = PatientName == subject.PatientName;
    //        bool sameAnat = AnatomicalSpaces.All(k => subject.AnatomicalSpaces.Contains(k)) && AnatomicalSpaces.Count == subject.AnatomicalSpaces.Count;
    //        bool sameEeg = Files.All(k => subject.Files.Contains(k)) && Files.Count == subject.Files.Count;
    //        bool sameVideo = Video == subject.Video;
    //        return sameName && sameAnat && sameEeg && sameVideo;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //public override int GetHashCode()
    //{
    //    return base.GetHashCode();
    //}

    //public static bool operator ==(Subject a, Subject b)
    //{
    //    if (ReferenceEquals(a, b))
    //    {
    //        return true;
    //    }

    //    if (((object)a == null) || ((object)b == null))
    //    {
    //        return false;
    //    }

    //    return a.Equals(b);
    //}
    //public static bool operator !=(Subject a, Subject b)
    //{
    //    return !(a == b);
    //}
    //#endregion
}