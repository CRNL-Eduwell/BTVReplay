using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NormalizeTF : MonoBehaviour
{
    public BtvEvent Baseline
    {
        get
        {
            return (_NormalizedEventList.ObjectsSelected.Length > 0) ? _NormalizedEventList.ObjectsSelected[0] : null;
        }
    }

    [SerializeField] private Button _Close = null;
    [SerializeField] private NormalizeEventList _NormalizedEventList = null;
    [SerializeField] private Button _NormalizeOk = null;
    [SerializeField] private Button _SetAsDefault = null;

    public void Initialize(UnityAction yesAction, UnityAction cancelAction)
    {
        _Close.onClick.AddListener(Close);
        ((ISelectionCountable)_NormalizedEventList).OnSelectionChanged.AddListener(UpdateShownEvent);

        _NormalizeOk.onClick.AddListener(yesAction);
        _SetAsDefault.onClick.AddListener(cancelAction);

        if (TimeFrequencyService.BaselineEvent != null)
        {
            int indexOf = _NormalizedEventList.Objects.IndexOf(TimeFrequencyService.BaselineEvent);
            if (indexOf == -1)
            {
                UnityEngine.Debug.LogError("Baseline event not found in event list");
            }
            else
            {
                UnityEngine.Debug.Log("index : " + indexOf);
                _NormalizedEventList.Select(_NormalizedEventList.Objects[indexOf]);
            }
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _Close.onClick.RemoveAllListeners();
        ((ISelectionCountable)_NormalizedEventList).OnSelectionChanged.RemoveAllListeners();

        _NormalizeOk.onClick.RemoveAllListeners();
        _SetAsDefault.onClick.RemoveAllListeners();
    }

    private void UpdateShownEvent()
    {
        BtvEvent[] SelectedElements = _NormalizedEventList.ObjectsSelected;
        _NormalizeOk.interactable = (SelectedElements.Length > 0);
    }
}
