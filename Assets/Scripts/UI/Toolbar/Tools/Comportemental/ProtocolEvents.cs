using BTV.Services.ProtocolService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    //public delegate void UpdateProtocol(ProvFile protocol);

    public class ProtocolEvents : Tool
    {
        //public event UpdateProtocol UpdateProtocol;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Dropdown m_Protocols = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_ProcessEvents = null;

       //private List<ProvFile> m_ProtocolList = new List<ProvFile>();

        public override void Initialize()
        {
            m_Protocols.options.Clear();
            int protocolCount = ProtocolService.ProtocolFiles.Count;
            for (int i = 0; i < protocolCount; i++)
            {
                m_Protocols.options.Add(new Dropdown.OptionData(ProtocolService.ProtocolFiles[i].ShortName));
            }

            m_ProcessEvents.onClick.AddListener(ProcessSelectedProtocol);
        }

        private void ProcessSelectedProtocol()
        {
            UiToTaskPerformanceMessage message = new UiToTaskPerformanceMessage
            {
                TaskToExecute = 0,
                NewProtocol = ProtocolService.ProtocolFiles[m_Protocols.value]
            };
            Messenger.Default.Send(message, MessageContext.UiToTaskPerformanceMessage);
        }
    }
}