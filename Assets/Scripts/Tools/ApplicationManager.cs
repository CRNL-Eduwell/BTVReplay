using UnityEngine;
using System.Collections;
using BTV.Services.UserPreferencesService;
using Tools.Unity;
using BTV.Services.ProtocolService;
using BTV.Services;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] TooltipManager m_TooltipManager = null;

    private void Awake()
    {
        ApplicationState.Module3D = FindAnyObjectByType<BTV3DModule>();
        if (ApplicationState.Module3D != null)
            ApplicationState.Module3D.Initialize(Session.Current);
        ApplicationState.TooltipManager = m_TooltipManager;
        //===
        UserPreferencesService.LoadPreferences();
        ProtocolService.LoadAllProtocols();
    }
    private void OnDestroy()
    {
        //clean data used
    }
}
