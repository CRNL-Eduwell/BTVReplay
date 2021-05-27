using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BTV.UI.MainWindow
{
    /// <summary>
    /// Base class for a menu 
    /// 
    /// From HiBoP Menu class
    /// 
    /// Note : The Function SwapOpenState must be added manualy to the 
    /// on click event of the button of the menu gameboject
    /// </summary>
    public class Menu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GenericEvent<bool> OnHover { get; } = new GenericEvent<bool>();
        public GenericEvent<bool> OnChangeOpenState { get; } = new GenericEvent<bool>();
        public bool IsOpen
        {
            get
            {
                return m_IsOpen;
            }
            set
            {
                if (m_IsOpen != value)
                {
                    m_IsOpen = value;
                    if (m_SubMenu /* maybe add an interactable property ? */)
                        m_SubMenu.gameObject.SetActive(value);
                    OnChangeOpenState.Invoke(value);
                }
            }
        }
        public bool IsHovered
        {
            get
            {
                return m_IsHovered;
            }
            set
            {
                if (m_IsHovered != value)
                {
                    m_IsHovered = value;
                    OnHover.Invoke(value);
                }
            }
        }
        //==
        [SerializeField] private RectTransform m_SubMenu = null;

        private bool m_IsOpen = false, m_IsHovered = false;

        public void Open()
        {
            IsOpen = true;
        }
        public void Close()
        {
            IsOpen = false;
        }
        public void SwapOpenState()
        {
            IsOpen = !IsOpen;
        }
        public void OnPointerEnter(PointerEventData data)
        {
            IsHovered = true;
        }
        public void OnPointerExit(PointerEventData data)
        {
            IsHovered = false;
        }
    }
}