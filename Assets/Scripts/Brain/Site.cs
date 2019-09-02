using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Site : MonoBehaviour
{
    #region Pubic Members
    public int ID
    {
        get;
        private set;
    }
    public bool IsFrozen
    {
        get
        {
            return m_IsFrozen;
        }
        set
        {
            if (value == true)
            {
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            m_IsFrozen = value;
        }
    }
    public string MarsAtlasName
    {
        get
        {
            if (m_Plot is Intra_Plot)
            {
                Intra_Plot currentPlot = (Intra_Plot)m_Plot;
                if (currentPlot.Atlas.nameFull != "")
                    return currentPlot.Atlas.nameFull;
                else
                    return "";
            }
            else
                return "";
        }
    }
    public string BroadmanName
    {
        get
        {
            if (m_Plot is Intra_Plot)
            {
                Intra_Plot currentPlot = (Intra_Plot)m_Plot;
                if (currentPlot.Atlas.broadman != "")
                    return currentPlot.Atlas.broadman;
                else
                    return "";
            }
            else
                return "";
        }
    }
    public Vector3 Coordinates
    {
        get
        {
            return gameObject.transform.localPosition;
        }
    }
    public Color Color
    {
        get
        {
            return m_Color;
        }
        set
        {
            m_Color = value;
            m_MeshRenderer.materials[0].color = m_Color;
        }
    }
    #endregion

    #region Private Members
    private object m_Plot = null; //Reference to the corresponding data element , either a Eeg_Plot or Intra_Plot
    private bool m_IsFrozen = false;
    private MeshRenderer m_MeshRenderer = null;
    private Color m_Color = Color.white;
    private float m_Gain = 1;
    #endregion

    private CustomVideoPlayer m_VideoPlayer = null;

    public void Init(object Plot)
    {
        m_Plot = Plot;

        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);

        //ToDO :
        //need to be removed from here later
        //will be retrieved using IoCServiceContainer once it's loaded or
        //info will be send via messenger
        m_VideoPlayer = GameObject.Find("Workable Part").transform.GetChild(0).GetChild(1).GetComponent<CustomVideoPlayer>();
        m_VideoPlayer.sendTime += new timeVideo(UpdateSize);

        //If we don't find the corresponding name beetween this object and one electrode
        //in an eeg file , we don't show the site on the 3D brain
        ELAN FileHandle = ELAN.returnFirstValidHandle(ApplicationState.EegFiles);
        ID = FileHandle.electrodes.ToList().FindIndex(x => x.name.ToLower().Equals(gameObject.name));
        if (ID == -1)
            gameObject.SetActive(false);

        m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
        IsFrozen = false;
    }

    /// <summary>
    /// Used to unsubscribe from events whent he site is deactivated
    /// otherwise it tries to update values that are note here.
    /// Maybe see to destroy the go totally ? 
    /// </summary>
    private void OnDisable()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
        m_VideoPlayer.sendTime -= new timeVideo(UpdateSize);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
        m_VideoPlayer.sendTime -= new timeVideo(UpdateSize);
    }

    private void OnBrainParametersMessage(UiToBrainMessage message)
    {
        if (message.TaskToExecute == 2) //=> gain update
            m_Gain = message.Gain;
    }

    private void UpdateSize(int milliSecToLook)
    {
        if (!IsFrozen)
        {
            int MostRecentSample = (int)(milliSecToLook * (ApplicationState.Window1.TraceEeg.fileHandle.sampFreq / 1000));
            int PositionOfSampleInArray = (ID * ApplicationState.Window1.TraceEeg.fileHandle.nbSam) + MostRecentSample;
            float currentValue = ApplicationState.Window1.TraceEeg.fileHandle.eegData[PositionOfSampleInArray] / 100;
            float scale = 2 + (m_Gain * currentValue);

            if (scale >= 7)
                scale = 7;
            else if (scale <= 0)
                scale = 0.1f;

            gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}

