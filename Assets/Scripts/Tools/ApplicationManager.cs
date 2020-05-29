using UnityEngine;
using System.Collections;
using BTV.Services.UserPreferencesService;

public class ApplicationManager : MonoBehaviour
{
    private void Awake()
    {
        ApplicationState.Module3D = FindObjectOfType<BTV3DModule>();
        
        //===
        UserPreferencesService.LoadPreferences();
    }
    private void OnDestroy()
    {
        //clean data used
    }
}
