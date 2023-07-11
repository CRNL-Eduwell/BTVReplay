using BTV.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : Tools.Unity.Lists.SelectableItem<BtvEvent>
{
    #region Properties
    [SerializeField] private Button m_time = null;
    [SerializeField] private Text m_comment = null;
    [SerializeField] private Text m_code = null;
    private CustomVideoPlayer m_video = null;

    public override BtvEvent Object
    {
        get
        {
            return base.Object;
        }
        set
        {
            base.Object = value;
            initValues();
        }
    }
    #endregion

    #region Private Methods
    private void Start()
    {
        m_video = GameObject.Find("Video").GetComponent<CustomVideoPlayer>();
    }

    private void OnDestroy()
    {
        m_time.onClick.RemoveAllListeners();
    }

    private void initValues()
    {
        gameObject.name = "HubEvent - " + base.Object.TimeInSeconds;

        int timeInSec = (int)base.Object.TimeInSeconds;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        m_time.transform.GetChild(0).GetComponent<Text>().text = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        m_time.onClick.RemoveAllListeners();
        m_time.onClick.AddListener(() =>
        {
            ModulesToVideoMessage messageToVideo = new ModulesToVideoMessage
            {
                UpdateClickPosition = false,
                TimeMilliseconds = timeInSec * 1000
            };
            Messenger.Default.Send(messageToVideo, MessageContext.ModulesToVideoMessage);
        }); 

        m_comment.text = base.Object.Comment;
        m_code.text = base.Object.Code.ToString();
    }
    #endregion
}
