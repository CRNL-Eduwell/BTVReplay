using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WindowsManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform m_WorkableArea = null;

    [SerializeField]
    private GameObject[] m_Prefabs = null;

    private void Awake()
    {
        Messenger.Default.Register<ShowWindowMessage>(this, OnShowWindowMessage, MessageContext.ShowWindowMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.ShowWindowMessage);
    }

    public void OpenWindow(string name)
    {
        GameObject prefab = m_Prefabs.FirstOrDefault(x => x.name == name);
        if (prefab)
        {
            GameObject previousInstance = GameObject.Find(name);
            if (previousInstance == null)
            {
                GameObject instance = Instantiate(prefab, m_WorkableArea);
                instance.name = name;
            }
        }
    }
    private void OnShowWindowMessage(ShowWindowMessage message)
    {
        GameObject prefab = m_Prefabs.FirstOrDefault(x => x.name == message.WindowName);
        if (prefab)
        {
            GameObject previousInstance = GameObject.Find(message.WindowName);
            if (previousInstance == null)
            {
                GameObject instance = CreateWindow(prefab);
                instance.name = message.WindowName;
            }
        }
    }

    private GameObject CreateWindow(GameObject prefab)
    {
        GameObject gameObject = Instantiate(prefab, m_WorkableArea);
        RectTransform rectTransform = gameObject.transform as RectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        return gameObject;
    }
}
