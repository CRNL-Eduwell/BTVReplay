using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventOptionToggler : MonoBehaviour
{
    [SerializeField] private Toggle _ShowEvent = null;
    [SerializeField] private Toggle _ShowTimeFrequency = null;

    private EventTrace m_EventOfInterest = null;
    private Color m_Blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private EventTFViewer m_TfViewer = null;

    private void Awake()
    {
        m_EventOfInterest = transform.GetComponent<EventTrace>();
        m_TfViewer = transform.GetComponent<EventTFViewer>();

        _ShowEvent.onValueChanged.AddListener(ToggleEventView);
        _ShowTimeFrequency.onValueChanged.AddListener(ToggleTimeFrequencyView);
        Messenger.Default.Register<TimeFrequencyResultMessage>(this, OnTimeFrequencyResultMessage, MessageContext.TimeFrequencyResultMessage);
    }

    private void OnDestroy()
    {
        _ShowEvent.onValueChanged.RemoveAllListeners();
        _ShowTimeFrequency.onValueChanged.RemoveAllListeners();
        Messenger.Default.Unregister(this, MessageContext.TimeFrequencyResultMessage);
    }

    private void ToggleEventView(bool isViewable)
    {
        if (isViewable)
        {
            transform.GetComponent<RawImage>().color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, 0.5f);
        }
        else
        {
            transform.GetComponent<RawImage>().color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, 0f);
        }
    }

    private void ToggleTimeFrequencyView(bool isViewable)
    {
        if (isViewable)
        {
            ProcessCalculationMessage message = new ProcessCalculationMessage
            {
                Task = Calculations.TF,
                EventOfInterest = new BTV.Data.BtvEvent(m_EventOfInterest.EventOfInterest),
                TraceIndex = m_EventOfInterest.ParentWindowIndex
            };
            Messenger.Default.Send(message, MessageContext.ProcessCalculationMessage);

            transform.GetComponent<RawImage>().color = Color.white;
        }
        else
        {
            transform.GetComponent<RawImage>().color = new Color(m_Blue.r, m_Blue.g, m_Blue.b, 0f);
            m_TfViewer.ResetTfOptions();
        }
    }

    private void OnTimeFrequencyResultMessage(TimeFrequencyResultMessage message)
    {
        if (message.TraceIndex != m_EventOfInterest.ParentWindowIndex) return;
        if (message.EventOfInterest.Code != m_EventOfInterest.EventOfInterest.Code) return;
        if (message.EventOfInterest.TimeInMilliSeconds != m_EventOfInterest.EventOfInterest.TimeInMilliSeconds) return;
        if (message.EventOfInterest.Duration != m_EventOfInterest.EventOfInterest.Duration) return;

        //SetTf Data in Viewer
        UnityEngine.Debug.Log("OnTimeFrequencyResultMessage : Setting tf ");
        m_TfViewer.SetTfData(message.TFData, m_EventOfInterest.EventOfInterest);
    }
}
