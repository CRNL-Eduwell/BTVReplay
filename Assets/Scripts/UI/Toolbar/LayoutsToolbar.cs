using UnityEngine;

namespace BTV.UI.Module3D
{
    public class LayoutsToolbar : Toolbar
    {
        [SerializeField]
        Tools.LayoutsLoader m_LayoutsLoader = null;
        [SerializeField]
        Tools.LayoutsSaver m_LayoutsSaver = null;

        protected override void AddTools()
        {
            m_Tools.Add(m_LayoutsLoader);
            m_Tools.Add(m_LayoutsSaver);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_LayoutsLoader.LoadFile.AddListener(LoadFile);
            m_LayoutsSaver.SaveFile.AddListener(SaveFile);
        }

        private void LoadFile(string filePath)
        {
            UiToLayoutsMessage message = new UiToLayoutsMessage
            {
                TaskToExecute = 0,
                Path = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToLayouts);
        }

        private void SaveFile(string filePath)
        {
            UiToLayoutsMessage message = new UiToLayoutsMessage
            {
                TaskToExecute = 1,
                Path = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToLayouts);
        }
    }
}