using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoVolume : MonoBehaviour
{
    /// <summary>
    /// </summary>
    [SerializeField]
    private Button m_VolumeButton= null;
    /// <summary>
    /// </summary>
    [SerializeField]
    private Scrollbar m_VolumeScrollbar = null;

    private RawImage m_VolumeImage = null;
    private Texture2D m_VolumeMute = null;
    private Texture2D m_VolumeLow = null;
    private Texture2D m_VolumeMiddle = null;
    private Texture2D m_VolumeHigh = null;

    private float m_memoryVolume = 0;

    private void Awake()
    {
        m_VolumeMute = Resources.Load("Pictures/VolumeMute", typeof(Texture2D)) as Texture2D;
        m_VolumeLow = Resources.Load("Pictures/VolumeLow", typeof(Texture2D)) as Texture2D;
        m_VolumeMiddle = Resources.Load("Pictures/VolumeMiddle", typeof(Texture2D)) as Texture2D;
        m_VolumeHigh = Resources.Load("Pictures/VolumeHigh", typeof(Texture2D)) as Texture2D;

        m_VolumeImage = m_VolumeButton.GetComponent<RawImage>();

        m_VolumeButton.onClick.AddListener(OnVolumeButtonClicked);
        m_VolumeScrollbar.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        m_VolumeButton.onClick.RemoveAllListeners();
        m_VolumeScrollbar.onValueChanged.RemoveAllListeners();
    }

    private void OnVolumeButtonClicked()
    {
        if (m_VolumeScrollbar.value == 0)
        {
            m_VolumeScrollbar.value = m_memoryVolume;
            m_memoryVolume = 0;
        }
        else
        {
            m_memoryVolume = m_VolumeScrollbar.value;
            m_VolumeScrollbar.value = 0;
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (value == 0)
        {
            m_VolumeImage.texture = m_VolumeMute;
        }
        else if (value <= 0.333333333f)
        {
            m_VolumeImage.texture = m_VolumeLow;
        }
        else if (value <= 0.666666666f)
        {
            m_VolumeImage.texture = m_VolumeMiddle;
        }
        else
        {
            m_VolumeImage.texture = m_VolumeHigh;
        }
    }
}
