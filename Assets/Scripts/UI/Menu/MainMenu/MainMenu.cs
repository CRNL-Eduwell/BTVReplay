using UnityEngine;

namespace BTV.UI.MainWindow
{
    /// <summary>
    /// Menu bar of the main window. All behaviour lives in <see cref="MenuBar"/>.
    /// </summary>
    public class MainMenu : MenuBar
    {
        [SerializeField] private FileMenu m_FileMenu = null;
        [SerializeField] private ToolsMenu m_ToolsMenu = null;

        protected override Menu[] Menus
        {
            get
            {
                return new Menu[] { m_FileMenu, m_ToolsMenu };
            }
        }
    }
}
