using Assets.Scripts.Data.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BTV.Services.AnatomicalDataService
{
    public static class AnatomicalDataService
    {
        private static Dictionary<string, List<AnatomicalSite>> m_SitesPerReferential = new Dictionary<string, List<AnatomicalSite>>();
        private static MarsAtlas m_Atlas = null;

        public static void Reset()
        {
            m_SitesPerReferential = new Dictionary<string, List<AnatomicalSite>>();
        }

        public static IEnumerator c_Load(string referentialName, BrainDataContainer brainToLoad)
        {
            IAnatomicalSiteContext anatomicalSiteContext = AnatomicalSiteFactory.GetAnatomicalSiteContext(brainToLoad.Pts);
            try
            {
                m_SitesPerReferential.Add(referentialName, new List<AnatomicalSite>(anatomicalSiteContext.Electrodes));
            }
            catch (ArgumentException ae)
            {
                UnityEngine.Debug.LogError("AnatomicalDataService.c_load error : " + ae.Message);
            }
            yield return null;
        }

        public static IEnumerator c_LoadDefaultElectrodes(EegTechnology eeg)
        {
            Data.BtvProgram container = null;
            for (int i = 0; i < 6; i++)
            {
                container = EegFileService.EegFileService.ChangeContainerHandle(container, i);
                if (container != null)
                {
                    switch (eeg)
                    {
                        case EegTechnology.Intra:
                            {
                                List<AnatomicalSite> electrodes = new List<AnatomicalSite>();

                                string memPlot = "";
                                int nbIntraElec = 0, nbIntraPlot = 0;
                                foreach (Data.BtvChannel channel in container.Channels)
                                {
                                    Tuple<string, int> NameAndId = GetIntraPlotInformation(channel.Label);
                                    if (memPlot != NameAndId.Item1)
                                    {
                                        nbIntraElec += 5;
                                        nbIntraPlot = -5;
                                        memPlot = NameAndId.Item1;
                                    }
                                    else if (memPlot == NameAndId.Item1)
                                    {
                                        nbIntraPlot -= 5;
                                    }

                                    electrodes.Add(new AnatomicalSite(channel.Label, new Vector3(nbIntraElec, 0, nbIntraPlot)));
                                }

                                string referentialLabel = "ELEC_" + i;
                                m_SitesPerReferential.Add(referentialLabel, new List<AnatomicalSite>(electrodes));
                                break;
                            }
                        case EegTechnology.Scalp:
                            {
                                List<AnatomicalSite> electrodes = new List<AnatomicalSite>();

                                int count = 0;
                                foreach (var channel in container.Channels)
                                {
                                    string plotName = channel.Label.ToLower();
                                    electrodes.Add(new AnatomicalSite(plotName, new Vector3(5 * count, 0, 0)));
                                    count++;
                                }

                                string referentialLabel = "ELEC_" + i;
                                m_SitesPerReferential.Add(referentialLabel, new List<AnatomicalSite>(electrodes));
                                break;
                            }
                        case EegTechnology.Unknown:
                            {
                                throw new ArgumentException("Anatomical Data Service -> c_LoadDefaultElectrodes : eeg technology value is Unknown, it should not");
                            }
                        default:
                            {
                                throw new ArgumentException("Anatomical Data Service -> c_LoadDefaultElectrodes : eeg technology value is default, it should not");
                            }
                    }
                }
            }            
            yield return null;
        }

        //TODO : link atlas information in visualisation , right now atlas data is loaded but that's all
        public static IEnumerator c_LoadAtlas(string filePath)
        {
            if (File.Exists(filePath))
            {
                m_Atlas = new MarsAtlas(Application.dataPath);
                m_Atlas.loadPatientAtlas(filePath);
                //m_Atlas.findElectrodesWithAtlas(Electrodes);
            }

            yield return null;
        }

        public static bool IsReferentialValid(string referential)
        {
            switch (referential)
            {
                case "MNI":
                    {
                        return m_SitesPerReferential.ContainsKey("MNI");
                    }
                case "PAT":
                    {
                        return m_SitesPerReferential.ContainsKey("PAT");
                    }
                case "ELEC":
                    {
                        return m_SitesPerReferential.ContainsKey("ELEC_0") || m_SitesPerReferential.ContainsKey("ELEC_1") || m_SitesPerReferential.ContainsKey("ELEC_2") ||
                               m_SitesPerReferential.ContainsKey("ELEC_3") || m_SitesPerReferential.ContainsKey("ELEC_4") || m_SitesPerReferential.ContainsKey("ELEC_5");
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static KeyValuePair<string, List<AnatomicalSite>> ReturnFirstValidSitesList()
        {
            if (m_SitesPerReferential.ContainsKey("MNI")) return new KeyValuePair<string, List<AnatomicalSite>>("MNI", m_SitesPerReferential["MNI"]);
            else if (m_SitesPerReferential.ContainsKey("PAT")) return new KeyValuePair<string, List<AnatomicalSite>>("PAT", m_SitesPerReferential["PAT"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_0")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_0"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_1")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_1"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_2")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_2"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_3")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_3"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_4")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_4"]);
            else if (m_SitesPerReferential.ContainsKey("ELEC_5")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", m_SitesPerReferential["ELEC_5"]);
            else return default;
        }

        public static List<AnatomicalSite> GetSitesListFrom(string referential, int fileId = -1)
        {
            if (referential == "MNI" && m_SitesPerReferential.ContainsKey("MNI"))
            {
                return new List<AnatomicalSite>(m_SitesPerReferential["MNI"]);
            }
            else if (referential == "PAT" && m_SitesPerReferential.ContainsKey("PAT"))
            {
                return new List<AnatomicalSite>(m_SitesPerReferential["PAT"]);
            }
            else if (referential.StartsWith("ELEC_") && fileId != -1)
            {
                string key = "ELEC_" + fileId.ToString();
                bool hasKeyData = m_SitesPerReferential.ContainsKey(key);
                return hasKeyData ? new List<AnatomicalSite>(m_SitesPerReferential[key]) : default;
            }
            else 
            {
                return default;
            }
        }

        private static Tuple<string, int> GetIntraPlotInformation(string rawName)
        {
            Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
            Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

            Match resultLeft = ReLeft.Match(rawName);
            Match resultRight = ReRight.Match(rawName);

            string plotName = "";
            int plotID = -1;

            if (resultLeft.Groups[1].Length == 1)
            {
                plotName = resultLeft.Groups[1].Value;
                plotID = int.Parse(resultLeft.Groups[2].Value.ToString());
            }
            else if (resultRight.Groups[1].Length == 1)
            {
                plotName = resultRight.Groups[1].Value + resultRight.Groups[2].Value;
                plotID = int.Parse(resultRight.Groups[3].Value.ToString());
            }

            return new Tuple<string, int>(plotName, plotID);
        }
    }
}