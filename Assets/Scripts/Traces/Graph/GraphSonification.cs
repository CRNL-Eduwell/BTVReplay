using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphSonification : MonoBehaviour
{
    [SerializeField] CustomVideoPlayer video = null;
    [SerializeField] AudioSource audioSourceScript = null;

    Trace m_curve = null;
    List<AudioClip> m_clips = new List<AudioClip>();
    bool m_currentState = false;

    void Awake()
    {
        audioSourceScript.Play();
        audioSourceScript.Pause();
    }

    private void OnEnable()
    {
        //If element is disabled, like by hiding curve
        //we want to keep sonification playing if it was on
        if (m_currentState)
            audioSourceScript.Play();
    }

    public void init(Trace parentWin)
    {
        m_curve = parentWin;
        StartCoroutine(StartAudio());
    }

    public void toggleSonification(bool isOn)
    {
        m_currentState = isOn;
        if (m_currentState)
            audioSourceScript.UnPause();
        else
            audioSourceScript.Pause();
    }

    public void muteSonficiation()
    {
        audioSourceScript.volume = 0;
    }

    public void updateSonif(int milliSecToLook)
    {
        //int posInArray = m_curve.TraceEeg.FileHandle.Frequency.ConvertToRoundedNumberOfSamples(milliSecToLook);
        if (video.videoInterface.IsPlaying)
        {
            float currentValue = 0.5f + ((m_curve.TraceEeg.MostRecentValue / 100) * m_curve.TraceEeg.Gain);
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

    public void changeAudioSonification(int newIDClip)
    {
        if (!audioSourceScript.isPlaying)
        {
            audioSourceScript.clip = m_clips[newIDClip];
            audioSourceScript.Play();
        }
        else
        {
            audioSourceScript.Pause();
            audioSourceScript.clip = m_clips[newIDClip];
            audioSourceScript.Play();
        }
    }

    //Allow to load audio file not in ressource file
    IEnumerator StartAudio()
    {
        AudioClip clip = null;
        for (int i = 0; i < ApplicationState.Module3D.SoundFilePaths.Count; i++)
        {
            WWW audioLoader = new WWW("file://" + ApplicationState.Module3D.SoundFilePaths[i]);
            while (!audioLoader.isDone)
                yield return null;
            clip = audioLoader.GetAudioClip(false);
            clip.name = "Audio Clip number " + i.ToString();
            m_clips.Add(clip);
        }
        audioSourceScript.clip = m_clips[0];
        audioSourceScript.Play();
        audioSourceScript.Pause();
    }
}
