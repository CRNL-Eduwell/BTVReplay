/// <summary>
/// A named folder used to make stored paths portable across machines: a path saved as
/// "${NAME}/rest/of/path" resolves against the root configured under that name on the current
/// machine. Sites name their own roots (e.g. one "DATA" root, or separate "ANAT" and "EEG"
/// roots) - the application imposes no organisation.
/// </summary>
public class PathRoot
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";

    public PathRoot() { }

    public PathRoot(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public PathRoot(PathRoot rootToCopy)
    {
        Name = rootToCopy.Name;
        Path = rootToCopy.Path;
    }
}
