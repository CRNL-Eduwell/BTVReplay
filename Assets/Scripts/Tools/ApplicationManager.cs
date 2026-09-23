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
        // Built players default to the "Fastest" quality level, which has vSync off, and nothing
        // capped the frame rate: the video clock ticks every rendered frame and every module
        // redraws on it, paused included, so a player spun hundreds of frames a second. The
        // levels with vSync on (the editor's default) are left alone.
        if (QualitySettings.vSyncCount == 0)
            Application.targetFrameRate = 60;

        ApplicationState.Module3D = FindAnyObjectByType<BTV3DModule>();
        if (ApplicationState.Module3D != null)
            ApplicationState.Module3D.Initialize(Session.Current);
        ApplicationState.TooltipManager = m_TooltipManager;
        //===
        UserPreferencesService.LoadPreferences();
        ProtocolService.LoadAllProtocols();
    }
    // Once per run: this scene reloads on every patient switch.
    private static bool s_PreferencesErrorShown = false;

    private void Start()
    {
        // After every Awake, so the message window exists. Without this the user would only
        // learn about an unreadable preferences file when ${NAME} paths stop resolving.
        if (UserPreferencesService.LoadError != null && !s_PreferencesErrorShown)
        {
            s_PreferencesErrorShown = true;
            ApplicationState.displayMessage("Preferences not loaded", "NOK",
                "The preferences file could not be read, so default preferences are in use and the file will not be overwritten.\n\n"
                + UserPreferencesService.PATH + "\n" + UserPreferencesService.LoadError);
        }
    }
    private void OnDestroy()
    {
        //clean data used
    }
}
