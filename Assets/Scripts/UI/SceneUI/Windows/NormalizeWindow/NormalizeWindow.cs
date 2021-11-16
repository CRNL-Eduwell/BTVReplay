using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;
using UnityEngine.UI;

public class NormalizeWindow : MonoBehaviour
{
    [SerializeField] private Button _Close = null;
    [SerializeField] private TimeUI _BeginTime = null;
    [SerializeField] private TimeUI _EndTime = null;
    [SerializeField] private NormalizeEventList _NormalizedEventList = null;
    [SerializeField] private Button _NormalizeOk = null;
    [SerializeField] private Button _Cancel = null;

    void Start()
    {
        _BeginTime.TimeInSeconds = 0;
        _EndTime.TimeInSeconds = 0;

        _Close.onClick.AddListener(CloseWindow);
        ((ISelectionCountable)_NormalizedEventList).OnSelectionChanged.AddListener(UpdateShownEvent);

        _NormalizeOk.onClick.AddListener(NormalizeData);
        _Cancel.onClick.AddListener(CloseWindow);
    }

    private void OnDestroy()
    {
        _Close.onClick.RemoveAllListeners();
        ((ISelectionCountable)_NormalizedEventList).OnSelectionChanged.RemoveAllListeners();

        _NormalizeOk.onClick.RemoveAllListeners();
        _Cancel.onClick.RemoveAllListeners();
    }

    private void CloseWindow()
    {
        Destroy(gameObject);
    }

    private void UpdateShownEvent()
    {
        BtvEvent[] SelectedElements = _NormalizedEventList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            _BeginTime.TimeInSeconds = SelectedElements[0].TimeInSeconds;
            _EndTime.TimeInSeconds = (SelectedElements[0].TimeInSeconds + SelectedElements[0].Duration);
            _NormalizeOk.interactable = true;
        }
        else
        {
            _BeginTime.TimeInSeconds = 0;
            _EndTime.TimeInSeconds = 0;
            _NormalizeOk.interactable = false;
        }
    }

    private void NormalizeData()
    {
        if (_BeginTime.TimeInSeconds == _EndTime.TimeInSeconds)
        {
            ApplicationState.displayMessage("Can not normalize data", "NOK", "You can not normlize data using an event with no duration ");
            return;
        }

        if (_BeginTime.TimeInSeconds > _EndTime.TimeInSeconds)
        {
            ApplicationState.displayMessage("Can not normalize data", "NOK", "You can not normlize data using an event that finishes before it starts ");
            return;
        }

        if(_BeginTime.TimeInSeconds < 0)
        { 
            ApplicationState.displayMessage("Can not normalize data", "NOK", "You can not normlize data using an event that starts at a negativ value ");
            return;
        }

        TraceDisplayerNormalize message = new TraceDisplayerNormalize
        {
            TaskToExecute = 1,
            BeginTimeBaseline = _BeginTime.TimeInSeconds,
            EndTimeBaseline = _EndTime.TimeInSeconds
        };
        Messenger.Default.Send(message, MessageContext.TraceDisplayerNormalize);

        CloseWindow();
    }
}
