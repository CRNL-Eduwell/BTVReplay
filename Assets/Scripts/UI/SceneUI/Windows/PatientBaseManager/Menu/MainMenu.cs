using UnityEngine;

namespace BTV.UI.PatientBaseManager
{
    /// <summary>
    /// Menu bar of the patient base manager window. All behaviour lives in <see cref="MenuBar"/>.
    /// </summary>
    public class MainMenu : MenuBar
    {
        [SerializeField] private FileMenu m_FileMenu = null;
        [SerializeField] private EditMenu m_EditMenu = null;
        [SerializeField] private OptionMenu m_Option = null;

        protected override Menu[] Menus
        {
            get
            {
                return new Menu[] { m_FileMenu, m_EditMenu, m_Option };
            }
        }
    }
}
