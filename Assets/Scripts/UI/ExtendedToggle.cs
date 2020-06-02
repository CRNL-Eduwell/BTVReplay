using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//Brain min 1 , max 2
//EEG min 0, max 3
//perf min 0 , max 2
//video min 1, max 2
//events min 1, max 2

public class ExtendedToggle : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Send the UpdateUiAndModuleLayout currently clicked index
    /// 0 : Cache courbes et options
    /// 1 : juste l'objet 3d , mais pas les options
    /// 2 : objet 3D et options correspondantes
    /// 3 : Possiblité de changer la courbe selectionné en cliquant sur le cerveau
    /// </summary>
    public GenericEvent<int> UpdateUiAndModuleLayout = new GenericEvent<int>();

    [SerializeField]
    private Image m_BackgroundImage = null;
    [SerializeField]
    private bool m_LeftRightClick = false;
    [SerializeField]
    private int m_MinValue = 0;
    [SerializeField]
    private int m_MaxValue = 0;

    private int m_OptionsCounter = 1;
    private Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
    private Color transparent = new Color(0, 0, 0, 0);

    public void ResetToggle()
    {
        //if it's in special selection mode we reset it
        //otherwise it stays the way it is
        if (m_OptionsCounter >= 2)
        {
            m_OptionsCounter = 1;
            UpdateUiAndModuleLayout.Invoke(m_OptionsCounter);
        }
        m_BackgroundImage.color = transparent;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_LeftRightClick)
            OnLeftRightPointerClick(eventData);
        else
            OnLeftPointerClick(eventData);
    }

    private void OnLeftPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if (m_OptionsCounter + 1 <= m_MaxValue)
                {
                    m_OptionsCounter += 1;
                    UnityEngine.Debug.Log("Increasing counter " + m_OptionsCounter);
                }
                else
                {
                    if (m_OptionsCounter - 1 >= m_MinValue)
                    {
                        m_OptionsCounter -= 1;
                        UnityEngine.Debug.Log("Decreasing counter " + m_OptionsCounter);
                    }
                }
                m_BackgroundImage.color = (m_OptionsCounter <= 1) ? transparent : blue;
                UpdateUiAndModuleLayout.Invoke(m_OptionsCounter);
                break;
        }
    }

    private void OnLeftRightPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                if (m_OptionsCounter - 1 >= m_MinValue)
                    m_OptionsCounter -= 1;
                m_BackgroundImage.color = (m_OptionsCounter <= 1) ? transparent : blue;
                UpdateUiAndModuleLayout.Invoke(m_OptionsCounter);
                break;
            case PointerEventData.InputButton.Left:
                if (m_OptionsCounter + 1 <= m_MaxValue)
                    m_OptionsCounter += 1;
                m_BackgroundImage.color = (m_OptionsCounter <= 1) ? transparent : blue;
                UpdateUiAndModuleLayout.Invoke(m_OptionsCounter);
                break;
        }
    }
}
