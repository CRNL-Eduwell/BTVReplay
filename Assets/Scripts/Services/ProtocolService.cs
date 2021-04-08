using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BTV.Services.ProtocolService
{
    public static class ProtocolService
    {
        //short name and path , do we need path ? 
        public static List<KeyValuePair<string, string>> Protocols { get; private set; } = null;

        //protocol change code old
        //protocol change code new
        //protocol blocs (List)

        private static List<ProvFile> m_ProtocolFiles = null;

        public static void LoadAllProtocols()
        {
            string[] protocolPaths = Directory.GetFiles(Application.dataPath + @"/Config/Prov/", "*.prov");

            m_ProtocolFiles = new List<ProvFile>();
            Protocols = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < protocolPaths.Count(); i++)
            {
                string[] splitPath = protocolPaths[i].Split(new char[] { '/', '.' });
                string shortName = splitPath[splitPath.Count() - 2];

                Protocols.Add(new KeyValuePair<string, string>(shortName, protocolPaths[i]));
                m_ProtocolFiles.Add(new ProvFile(protocolPaths[i]));
            }
        }
    }
}