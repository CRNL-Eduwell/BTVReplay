/// <summary>
/// Workspace parameters for all Brain data visualization
/// </summary>
public class BrainParameters 
{
    public bool IsMaxed { get; set; } = false;

    public BrainParameters(bool isMaxed = false)
    {
        IsMaxed = IsMaxed;
    }

    public BrainParameters(BrainParameters parameters)
    {
        IsMaxed = parameters.IsMaxed;
    }
}