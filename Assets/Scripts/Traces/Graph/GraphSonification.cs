using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphSonification : MonoBehaviour
{
    [SerializeField] CustomVideoPlayer _Video = null;
    [SerializeField] AudioSource _AudioSourceScript = null;

    private Trace m_curve = null;
    private List<AudioClip> m_clips = new List<AudioClip>();
    private bool m_currentState = false;

    // No Play/Pause warm-up in Awake: the source has no clip yet at that point, and a clipless
    // Play() pokes FMOD at scene start - on macOS this could surface a spurious "FMOD failed
    // to switch back to normal output" error during audio-device negotiation. StartAudio()
    // does the same warm-up once the clips are actually loaded.

    private void OnEnable()
    {
        //If element is disabled, like by hiding curve
        //we want to keep sonification playing if it was on
        if (m_currentState)
            _AudioSourceScript.Play();
    }

    public void Init(Trace parentWin)
    {
        m_curve = parentWin;
        StartCoroutine(StartAudio());
    }

    public void Toggle(bool isOn)
    {
        m_currentState = isOn;
        if (m_currentState)
            _AudioSourceScript.UnPause();
        else
            _AudioSourceScript.Pause();
    }

    public void Mute()
    {
        _AudioSourceScript.pitch = 0;
    }

    public void UpdateSonification(int milliSecToLook)
    {
        if (!m_currentState) return;

        if (_Video.VideoInterface.IsPlaying)
        {
            float currentValue = (0.5f + m_curve.MostRecentValueInPercentOfTrace) / 3;
            if (currentValue >= 0 && currentValue <= 0.33f)
            {
                _AudioSourceScript.pitch = currentValue;
            }
            else if (currentValue > 0.33f)
            {
                _AudioSourceScript.pitch = 0.33f;
            }
            else //(<0)
            {
                _AudioSourceScript.pitch = 0;
            }
        }
        else
        {
            _AudioSourceScript.pitch = 0;
        }
    }

    public void ChangeAudioClip(int newIDClip)
    {
        if (!_AudioSourceScript.isPlaying)
        {
            _AudioSourceScript.clip = m_clips[newIDClip];
            _AudioSourceScript.Play();
        }
        else
        {
            _AudioSourceScript.Pause();
            _AudioSourceScript.clip = m_clips[newIDClip];
            _AudioSourceScript.Play();
        }
    }

    //Allow to load audio file not in ressource file
    private IEnumerator StartAudio()
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
        _AudioSourceScript.clip = m_clips[0];
        _AudioSourceScript.Play();
        _AudioSourceScript.Pause();
    }
}
