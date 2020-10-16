using UnityEngine;
using System.Collections;
using BTV.Services.UserPreferencesService;
using Tools.Unity;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] TooltipManager m_TooltipManager = null;

    private void Awake()
    {
        ApplicationState.Module3D = FindObjectOfType<BTV3DModule>();
        ApplicationState.TooltipManager = m_TooltipManager;
        //===
        UserPreferencesService.LoadPreferences();
    }
    private void OnDestroy()
    {
        //clean data used
    }
}
