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
    /// </summary>
    /// 0 : Cache courbes et options
    /// 1 : juste l'objet 3d , mais pas les options
    /// 2 : objet 3D et options correspondantes
    /// 3 : Possiblité de changer la courbe selectionné en cliquant sur le cerveau
    public GenericEvent<int> UpdateUiAndModuleLayout = new GenericEvent<int>();

    [SerializeField]
    private int m_MaxValue = 0;
    [SerializeField]
    private int m_MinValue = 0;
    private int m_OptionsCounter = 1;

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                if (m_OptionsCounter - 1 >= m_MinValue)
                    m_OptionsCounter -= 1;
                break;
            case PointerEventData.InputButton.Left:
                if (m_OptionsCounter + 1 <= m_MaxValue)
                    m_OptionsCounter += 1;
                break;
        }

        UpdateUiAndModuleLayout.Invoke(m_OptionsCounter);
    }
}
