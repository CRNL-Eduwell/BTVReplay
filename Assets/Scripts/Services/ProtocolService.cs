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
        //protocol change code old
        //protocol change code new
        //protocol blocs (List)

        public static List<Protocol> ProtocolFiles = null;

        public static void Reset()
        {
            ProtocolFiles = new List<Protocol>();
        }
        public static void LoadAllProtocols()
        {
            string[] protocolPaths = Directory.GetFiles(Application.dataPath + @"/Config/Prov/", "*.prov");
            ProtocolFiles = LoadProtocols(protocolPaths);
        }

        /// <summary>
        /// The protocols that load, in path order. A file that cannot be read is logged as an error
        /// naming it and left out of the list (the Protocol events dropdown is built from it), rather
        /// than offered half-read.
        /// </summary>
        public static List<Protocol> LoadProtocols(IEnumerable<string> protocolPaths)
        {
            List<Protocol> protocols = new List<Protocol>();
            foreach (string path in protocolPaths)
            {
                try
                {
                    protocols.Add(new Protocol(path));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return protocols;
        }
    }
}