using System.Collections.Generic;

namespace Assets.Scripts.Data.Factory
{
    public interface IUserPreferencesContext
    {
        UserPreferences UserPreferences { get; set; } 
        string FilePath { get; set; }
    }
}