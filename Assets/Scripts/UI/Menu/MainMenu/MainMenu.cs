using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace BTV.UI.MainWindow
{
    public class MainMenu : MonoBehaviour
    {
        public FileMenu FileMenu { get { return m_FileMenu; } }
        public ToolsMenu ToolsMenu { get { return m_ToolsMenu; } }

        [SerializeField] private FileMenu m_FileMenu = null;
        [SerializeField] private ToolsMenu m_ToolsMenu = null;

        private bool IsOneMenuOpen
        {
            get
            {
                return m_FileMenu.IsOpen || m_ToolsMenu.IsOpen;
            }
        }

        private void Awake()
        {
            m_FileMenu.OnChangeOpenState.AddListener((isOpen) =>
            {
                if (isOpen)
                    Set(m_FileMenu);
            });
            m_FileMenu.OnHover.AddListener((isHovered) =>
            {
                if (isHovered && IsOneMenuOpen)
                    m_FileMenu.Open();
            });
            m_ToolsMenu.OnChangeOpenState.AddListener((isOpen) =>
            {
                if (isOpen)
                    Set(m_ToolsMenu);
            });
            m_ToolsMenu.OnHover.AddListener((isHovered) =>
            {
                if (isHovered && IsOneMenuOpen)
                    m_ToolsMenu.Open();
            });
        }

        private void OnDestroy()
        {
            m_FileMenu.OnChangeOpenState.RemoveAllListeners();
            m_FileMenu.OnHover.RemoveAllListeners();
            m_ToolsMenu.OnChangeOpenState.RemoveAllListeners();
            m_ToolsMenu.OnHover.RemoveAllListeners();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                // convert to a 2D position
                pointer.position = Input.mousePosition;
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);
                if (raycastResults.Count > 0)
                {
                    if (raycastResults[0].gameObject.layer != 23) //if it's not a menu ui element
                    {
                        CloseAll();
                    }
                }
            }
        }

        public void CloseAll()
        {
            m_FileMenu.Close();
            m_ToolsMenu.Close();
        }

        private void Set(Menu menu)
        {
            if (menu != m_FileMenu) m_FileMenu.Close();
            if (menu != m_ToolsMenu) m_ToolsMenu.Close();
        }
    }
}