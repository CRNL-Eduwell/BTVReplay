using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    public class CompPerformanceToolbar : Toolbar
    {
        [SerializeField]
        private Tools.ComportementalWindow m_ComportementWindow = null;

        [SerializeField]
        private Tools.ProtocolEvents m_Protocols = null;

        #region Private Methods
        protected override void AddTools()
        {
            m_Tools.Add(m_ComportementWindow);
            m_Tools.Add(m_Protocols);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_ComportementWindow.UpdateTime += SendUpdateWindowMessage;
            m_Protocols.UpdateProtocol += SendUpdateProtocolMessage;
        }

        private void SendUpdateProtocolMessage(ProvFile protocol)
        {
            UiToTaskPerformanceMessage message = new UiToTaskPerformanceMessage
            {
                TaskToExecute = 0,
                NewProtocol = protocol
            };
            Messenger.Default.Send(message, MessageContext.UiToTaskPerformanceMessage);
        }

        private void SendUpdateWindowMessage(int UpdatedTime)
        {
            UiToTaskPerformanceMessage message = new UiToTaskPerformanceMessage
            {
                TaskToExecute = 1,
                TimeWindow = UpdatedTime
            };
            Messenger.Default.Send(message, MessageContext.UiToTaskPerformanceMessage);
        }
        #endregion
    }
}
