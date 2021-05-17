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

            ProtocolFiles = new List<Protocol>();
            for (int i = 0; i < protocolPaths.Count(); i++)
            {
                ProtocolFiles.Add(new Protocol(protocolPaths[i]));
            }
        }
    }
}