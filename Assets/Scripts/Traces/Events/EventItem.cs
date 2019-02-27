using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : Tools.SelectableItem<TraceEvent>
{
    #region Properties
    [SerializeField] private Button m_time = null;
    [SerializeField] private Text m_comment = null;
    [SerializeField] private Text m_code = null;
    private VideoPlayer m_video = null;

    public override TraceEvent Object
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
        m_video = GameObject.Find("PanelR").GetComponent<VideoPlayer>();
    }

    private void OnDestroy()
    {
        m_time.onClick.RemoveAllListeners();
    }

    private void initValues()
    {
        gameObject.name = "HubEvent - " + base.Object.sample;

        int timeInSec = base.Object.sample / base.Object.samplingFrequency;
        int h = timeInSec / 3600;
        int m = (timeInSec / 60) % 60;
        int s = timeInSec % 60;

        if (h > 0)
            m_time.transform.GetChild(0).GetComponent<Text>().text = h + ":" + m + ":" + s;
        else
            m_time.transform.GetChild(0).GetComponent<Text>().text = "00:" + m + ":" + s;

        m_time.onClick.AddListener(() => m_video.setTime(timeInSec * 1000));

        m_comment.text = base.Object.comment;
        m_code.text = base.Object.code.ToString();
    }
    #endregion
}
