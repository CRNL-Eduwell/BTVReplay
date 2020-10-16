using System.Runtime.Serialization;

[DataContract]
public class VersionInfo
{
    [DataMember(Name = "tag_name")]
    public string VersionNumber { get; set; }

    [DataMember(Name = "html_url")]
    public string URL { get; set; }
}