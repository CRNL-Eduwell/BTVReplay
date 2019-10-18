using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void UpdateProtocol(ProvFile protocol);

    public class ProtocolEvents : Tool
    {
        public event UpdateProtocol UpdateProtocol;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Dropdown m_Protocols = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_ProcessEvents = null;

        private List<ProvFile> m_ProtocolList = new List<ProvFile>();

        public override void Initialize()
        {
            LoadProtocols();
            m_ProcessEvents.onClick.AddListener(()=> { UpdateProtocol(m_ProtocolList[m_Protocols.value]); });
        }

        private void LoadProtocols()
        {
            //get file list from folder
            string[] protocolPaths = Directory.GetFiles(Application.dataPath + @"/Config/Prov/", "*.prov");

            m_Protocols.options.Clear();
            for (int i = 0; i < protocolPaths.Count(); i++)
            {
                //Fill Dropdown
                string[] splitPath = protocolPaths[i].Split(new char[] { '/', '.' });
                string shortName = splitPath[splitPath.Count() - 2];
                m_Protocols.options.Add(new Dropdown.OptionData(shortName));

                //Add to protocol list
                m_ProtocolList.Add(new ProvFile(protocolPaths[i]));  
            }
        }
    }
}