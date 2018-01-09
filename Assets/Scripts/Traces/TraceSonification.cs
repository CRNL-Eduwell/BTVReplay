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
    bool initDone = false;
    List<AudioClip> clips = new List<AudioClip>();
    bool currentState = false;

    void Awake()
    {
        audioSourceScript = gameObject.GetComponent<AudioSource>();
        audioSourceScript.Play();
        audioSourceScript.Pause();
        curve = gameObject.transform.parent.GetComponent<TraceCurve>();
        traceID = curve.idTrace;
        media.loadTrace += new initTrace(init);
    }

    private void OnEnable()
    {
        //If element is disabled, like by hiding curve
        //we want to keep sonification playing if it was on
        if (currentState)
            audioSourceScript.Play();
    }

    void OnDestroy()
    {
        media.loadTrace -= new initTrace(init);
        if (initDone)
        {
            hub.traceRemotes[traceID].idFileHasChanged -= new idFileChangedEventHandler(delegate (int newID)
            {
                eHandle = ELAN.changeHandle(eHandle, media.elanFiles, newID);
            });
            hub.traceRemotes[traceID].sonifToggled -= new toggleSonification(toggleSonification);
            hub.traceRemotes[traceID].soundChanged -= new newSoundSonif(changeAudioSonification);
            video.stopTimeVideo -= new stopVideo(() =>
            {
                audioSourceScript.volume = 0;
            });

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
        hub.traceRemotes[traceID].sonifToggled += new toggleSonification(toggleSonification);
        hub.traceRemotes[traceID].soundChanged += new newSoundSonif(changeAudioSonification);
        video.sendTime += new timeVideo(updateSonif);
        video.stopTimeVideo += new stopVideo(() =>
        {
            audioSourceScript.volume = 0;
        });

        StartCoroutine(StartAudio());
        initDone = true;
    }

    void toggleSonification(bool isOn)
    {
        currentState = isOn;
        if (isOn)
            audioSourceScript.UnPause();
        else
            audioSourceScript.Pause();
    }

    void updateSonif(int milliSecToLook)
    {
        int posInArray = (curve.idElectrode * eHandle.nbSam) + (int)(milliSecToLook * (eHandle.sampFreq / 1000));
        if (video.videoInterface.isPlaying)
        {
            float currentValue = 0.5f + ((eHandle.eegData[posInArray] / 100) * curve.Gain);
            if (currentValue > 1)
                audioSourceScript.volume = 1;
            else if (currentValue <= 1 && currentValue >= 0)
                audioSourceScript.volume = currentValue;
            else if (currentValue < 0)
                audioSourceScript.volume = 0;
        }
        else
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
