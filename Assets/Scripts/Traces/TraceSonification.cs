using VLCSharp;
using UnityEngine;
using System.Collections; //IEnumerator
using System.Collections.Generic;

public class TraceSonification : MonoBehaviour
{
    [SerializeField] optionsHub hub = null;
    [SerializeField] BTVMedia media = null;
    [SerializeField] VideoPlayer video = null;

    TraceCurve curve = null;
    AudioSource audioSourceScript = null;
    ELAN eHandle = null;
    int traceID = 0;
    int elecID = 0;
    bool initDone = false;
    List<AudioClip> clips = new List<AudioClip>();

    void Awake()
    {
        audioSourceScript = gameObject.GetComponent<AudioSource>();
        audioSourceScript.Play();
        audioSourceScript.Pause();
        curve = gameObject.transform.parent.GetComponent<TraceCurve>();
        traceID = curve.idTrace;
        elecID = curve.idElectrode;

        media.mediaLoaded += new mediaLoadedEventHandler(init);
    }

    void OnDestroy()
    {
        media.mediaLoaded -= new mediaLoadedEventHandler(init);
        if (initDone)
        {
            hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(delegate (int newID)
            {
                eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
            });
            hub.traceRemotes[traceID].idElecHasChanged -= new idElecChangedEventHandler((newID) => {
                elecID = newID;
            });
            hub.traceRemotes[traceID].sonifToggled -= new toggleSonification(toggleSonification);
            hub.traceRemotes[traceID].soundChanged -= new newSoundSonif(changeAudioSonification);

            video.sendTime -= new timeVideo(updateSonif);
        }
    }

    void init()
    {
        eHandle = ELAN.returnFirstValidHandle(media.elanFiles);

        hub.traceRemotes[traceID].idFileHasChanged += new idFileChangedEventHandler(delegate (int newID)
        {
            eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
        });
        hub.traceRemotes[traceID].idElecHasChanged += new idElecChangedEventHandler((newID) => {
            elecID = newID;
        });
        hub.traceRemotes[traceID].sonifToggled += new toggleSonification(toggleSonification);
        hub.traceRemotes[traceID].soundChanged += new newSoundSonif(changeAudioSonification);
        video.sendTime += new timeVideo(updateSonif);

        StartCoroutine(StartAudio());
        initDone = true;
    }

    void toggleSonification(bool isOn)
    {
        if (isOn)
            audioSourceScript.UnPause();
        else
            audioSourceScript.Pause();
    }

    void updateSonif(int sampleToLook)
    {
        int posInArray = (elecID * eHandle.nbSam) + sampleToLook;
        float currentValue = (eHandle.eegData[posInArray] / eHandle.maxValues[elecID]) * (curve.Gain * 5);
        if (currentValue > 1)
        {
            audioSourceScript.volume = 1;
        }
        else if (currentValue <= 1 && currentValue >= 0)
        {
            audioSourceScript.volume = currentValue;
        }
        else if (currentValue < 0)
        {
            audioSourceScript.volume = 0;
        }
    }

    void changeAudioSonification(int newIDClip)
    {
        if (!audioSourceScript.isPlaying)
        {
            audioSourceScript.clip = clips[newIDClip];
            audioSourceScript.Play();
        }
        else
        {
            audioSourceScript.Pause();
            audioSourceScript.clip = clips[newIDClip];
            audioSourceScript.Play();
        }
    }

    //Allow to load audio file not in ressource file
    IEnumerator StartAudio()
    {
        AudioClip clip = null;
        for (int i = 0; i < hub.traceRemotes[traceID].soundFilesAbsPath.Count; i++)
        {
            WWW audioLoader = new WWW("file://" + hub.traceRemotes[traceID].soundFilesAbsPath[i]);
            while (!audioLoader.isDone)
                yield return null;

            clip = audioLoader.GetAudioClip(false);
            clip.name = hub.traceRemotes[traceID].soundFileShort[i];
            clips.Add(clip);
        }
        audioSourceScript.clip = clips[0];
        audioSourceScript.Play();
        audioSourceScript.Pause();
    }
}
