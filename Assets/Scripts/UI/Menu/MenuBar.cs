using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace BTV.UI
{
    /// <summary>
    /// Common behaviour for a bar of mutually exclusive Menu entries: opening one closes the
    /// others, hovering an entry while the bar is open switches to it, and clicking outside
    /// the menu layer closes everything. Subclasses only declare which menus they hold
    /// (the main window and the patient base manager used to duplicate all of this).
    /// </summary>
    public abstract class MenuBar : MonoBehaviour
    {
        /// <summary>
        /// Layer of the menu ui elements; a click landing on any other layer closes the bar.
        /// </summary>
        private const int MenuUiLayer = 23;

        /// <summary>
        /// The menus managed by this bar, from the subclass's serialized fields.
        /// </summary>
        protected abstract Menu[] Menus { get; }

        private Menu[] m_Menus;

        private bool IsOneMenuOpen
        {
            get
            {
                foreach (Menu menu in m_Menus)
                {
                    if (menu.IsOpen)
                        return true;
                }
                return false;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Wires the open/hover listeners. Called from Awake; public and idempotent so
        /// edit-mode tests (which run without the MonoBehaviour lifecycle) can call it.
        /// </summary>
        public void Initialize()
        {
            if (m_Menus != null)
                return;

            m_Menus = Menus;
            foreach (Menu menu in m_Menus)
            {
                Menu entry = menu;
                entry.OnChangeOpenState.AddListener((isOpen) =>
                {
                    if (isOpen)
                        Set(entry);
                });
                entry.OnHover.AddListener((isHovered) =>
                {
                    if (isHovered && IsOneMenuOpen)
                        entry.Open();
                });
            }
        }

        private void OnDestroy()
        {
            if (m_Menus == null)
                return;

            foreach (Menu menu in m_Menus)
            {
                menu.OnChangeOpenState.RemoveAllListeners();
                menu.OnHover.RemoveAllListeners();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && IsOneMenuOpen)
            {
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                // convert to a 2D position
                pointer.position = Input.mousePosition;
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);
                if (raycastResults.Count > 0)
                {
                    if (raycastResults[0].gameObject.layer != MenuUiLayer) //if it's not a menu ui element
                    {
                        CloseAll();
                    }
                }
            }
        }

        public void CloseAll()
        {
            foreach (Menu menu in m_Menus)
            {
                menu.Close();
            }
        }

        private void Set(Menu menu)
        {
            foreach (Menu other in m_Menus)
            {
                if (other != menu)
                    other.Close();
            }
        }
    }
}
