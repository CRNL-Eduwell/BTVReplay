/// <summary>
/// Stable ids for the two dock areas, stored in saved workspaces (TraceParameters.Parent).
/// Workspaces used to store the dock parent's live scene-object name and look it up again with
/// GameObject.Find, so renaming either object silently broke every saved workspace. The ids are
/// now written by which WindowLayout a window sits in. They keep the legacy names as values, so
/// workspaces saved before and after stay interchangeable, including with older builds.
/// </summary>
public static class DockSide
{
    public const string Left = "Pannel";
    public const string Right = "RightPannel";

    /// <summary>Unknown or missing ids dock right, as the old lookup did when Find found nothing.</summary>
    public static bool IsLeft(string savedId) => savedId == Left;
}
