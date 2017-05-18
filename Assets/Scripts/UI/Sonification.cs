using System;

using UnityEngine;
using UnityEngine.UI;

public class Sonification : MonoBehaviour
{
    public RawImage speakerButton = null;
    public AudioSource audioSourceScript = null;
    public MainScript2 mainScript = null;
    public bool isRunningAudio = false;

    private Texture2D texPlay = null;
    private Texture2D texNPlay = null;
    private TVCurve tv = null;
    private RemoteManager rm = null;

    void Awake()
    {
        texPlay = Resources.Load("Pictures/soundOK", typeof(Texture2D)) as Texture2D;
        texNPlay = Resources.Load("Pictures/soundNOK", typeof(Texture2D)) as Texture2D;
        audioSourceScript.Play();
        audioSourceScript.Pause();

        string[] sp = gameObject.transform.parent.name.Split(new string[] { "Dragable" }, StringSplitOptions.RemoveEmptyEntries);
        tv = GameObject.Find("LRObject" + sp[0]).GetComponent<TVCurve>();
        rm = GameObject.Find("PanelTelecommandeTest" + sp[0]).GetComponent<RemoteManager>();
        rm.sonifHasBeenToggled += new toggleSonificationEventHandler(toggleSonification);
    }

    void OnDestroy()
    {
        rm.sonifHasBeenToggled += new toggleSonificationEventHandler(toggleSonification);
    }

    void Update()
    {
        if (mainScript.init == true && isRunningAudio == true && mainScript.vlcScript.player.IsPlaying)
        {
            int sampleToLook = (int)(mainScript.vlcScript.player.currentTime * 0.064);
            int offset = (rm.currentIdElec * tv.eHandle.nbSam);
            float value = tv.eHandle.eegData[sampleToLook + offset] / 100;
            if (value <= 1 && value >= 0)
            {
                audioSourceScript.volume = value;
            }
        }
    }

    void toggleSonification()
    {
        if (isRunningAudio)
        {
            isRunningAudio = !isRunningAudio;
            audioSourceScript.Pause();
            speakerButton.texture = texNPlay;
        }
        else
        {
            isRunningAudio = !isRunningAudio;
            audioSourceScript.UnPause();
            speakerButton.texture = texPlay;
        }
    }
}
