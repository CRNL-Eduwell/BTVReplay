using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface IWorkspaceContext
    {
        Workspace Workspace { get; set; }
        string FilePath { get; set; }
    }
}