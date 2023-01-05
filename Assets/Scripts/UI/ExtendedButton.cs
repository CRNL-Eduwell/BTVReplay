using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExtendedButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent OnSingleClick = new UnityEvent();
    public UnityEvent OnDoubleClick = new UnityEvent();

    public string Text { get { return _Label.text; } }
    public Color Color { get { return _Background.color; } }

    [SerializeField] private Image _Background = null;
    [SerializeField] private Text _Label = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 1)
        {
            OnSingleClick.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            OnDoubleClick.Invoke();
        }
        else
        {
            //Nothing
        }
    }

    public void SetTabText(string text)
    {
        _Label.text = text;
    }

    public void SetColor(Color color)
    {
        _Background.color = color;
    }
}
