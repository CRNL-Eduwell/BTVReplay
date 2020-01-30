using BTV.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventsTexture : MonoBehaviour
{
    [SerializeField] CustomVideoPlayer m_VideoPlayer = null;
    [SerializeField] RawImage m_ScrollBarRawImage = null;

    private Texture2D m_DefaultTexturePrefab = null;
    private Texture2D m_CurrentTexture = null;
    private Color[] m_TextureColorData;
    private Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
    private Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);

    private void Start()
    {
        m_DefaultTexturePrefab = Resources.Load("Pictures/eventScroll", typeof(Texture2D)) as Texture2D;
        m_CurrentTexture = Instantiate(m_DefaultTexturePrefab);
        m_ScrollBarRawImage.texture = m_CurrentTexture;
        m_TextureColorData = m_CurrentTexture.GetPixels();
    }

    public void AddEvents(List<BtvEvent> events)
    {
        int eventsCount = events.Count;
        for (int i = 0; i < eventsCount; i++)
        {
            AddEvent(events[i]);
        }
    }

    public void AddEvent(BtvEvent currentEvent)
    {
        float perC = ((currentEvent.TimeInMilliSeconds / m_VideoPlayer.videoInterface.TotalVideoTime));// * 1000);
        int pixelID = (int)(perC * m_CurrentTexture.width);

        if (currentEvent.Duration > 0)
        {
            if (currentEvent.Duration > 1000)
            {
                float perCDuration = ((currentEvent.TimeInMilliSeconds + currentEvent.Duration) / m_VideoPlayer.videoInterface.TotalVideoTime);// * 1000;
                int pixelIDDuration = (int)(perCDuration * m_CurrentTexture.width);
                for (int i = 0; i < m_CurrentTexture.height / 2; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        m_TextureColorData[(pixelID + j) + (i * m_CurrentTexture.width)] = orange;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < m_CurrentTexture.height; i++)
                    m_TextureColorData[pixelID + (i * m_CurrentTexture.width)] = orange;
            }
        }
        else
        {
            for (int i = 0; i < m_CurrentTexture.height; i++)
                m_TextureColorData[pixelID + (i * m_CurrentTexture.width)] = Color.red;
        }
        m_CurrentTexture.SetPixels(m_TextureColorData);
        m_CurrentTexture.Apply();
    }

    public void RemoveAllEvents()
    {
        int textureSize = m_TextureColorData.Length;
        for (int i = 0; i < textureSize; i++)
            m_TextureColorData[i] = hardBlue;

        m_CurrentTexture.SetPixels(m_TextureColorData);
        m_CurrentTexture.Apply();
    }

    public void RemoveEvents(List<BtvEvent> events)
    {
        int eventsCount = events.Count;
        for (int i = 0; i < eventsCount; i++)
        {
            RemoveEvent(events[i]);
        }
    }

    public void RemoveEvent(BtvEvent currentEvent)
    {
        float perC = (currentEvent.TimeInMilliSeconds / m_VideoPlayer.videoInterface.TotalVideoTime);// * 1000);
        int pixelID = (int)(perC * m_CurrentTexture.width);

        if (currentEvent.Duration > 0)
        {
            if (currentEvent.Duration > 1000)
            {
                float perCDuration = ((currentEvent.TimeInMilliSeconds + currentEvent.Duration) / m_VideoPlayer.videoInterface.TotalVideoTime);// * 1000;
                int pixelIDDuration = (int)(perCDuration * m_CurrentTexture.width);
                for (int i = 0; i < m_CurrentTexture.height / 2; i++)
                {
                    for (int j = 0; j < pixelIDDuration - pixelID; j++)
                        m_TextureColorData[(pixelID + j) + (i * m_CurrentTexture.width)] = hardBlue;
                }
            }
            else //if duration < 1000ms, too thin to see the red streak on the scrollbar
            {
                for (int i = 0; i < m_CurrentTexture.height; i++)
                    m_TextureColorData[pixelID + (i * m_CurrentTexture.width)] = hardBlue;
            }
        }
        else
        {
            for (int i = 0; i < m_CurrentTexture.height; i++)
                m_TextureColorData[pixelID + (i * m_CurrentTexture.width)] = hardBlue;
        }
        m_CurrentTexture.SetPixels(m_TextureColorData);
        m_CurrentTexture.Apply();
    }
}
