using Assets.Scripts.Data.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BTV.Services.AnatomicalDataService
{
    public static class AnatomicalDataService
    {
        private static Dictionary<string, List<AnatomicalSite>> SitesPerReferential { get { return Session.Current.SitesPerReferential; } }
        private static MarsAtlas Atlas
        {
            get { return Session.Current.Atlas; }
            set { Session.Current.Atlas = value; }
        }

        public static void Reset()
        {
            Session.Current.SitesPerReferential = new Dictionary<string, List<AnatomicalSite>>();
            Atlas = null;
        }

        public static IEnumerator c_Load(string referentialName, BrainDataContainer brainToLoad)
        {
            IAnatomicalSiteContext anatomicalSiteContext = AnatomicalSiteFactory.GetAnatomicalSiteContext(brainToLoad.Pts);
            try
            {
                SitesPerReferential.Add(referentialName, new List<AnatomicalSite>(anatomicalSiteContext.Electrodes));
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
            for (int i = 0; i < BTV.Data.EegSlots.Count; i++)
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
                                SitesPerReferential.Add(referentialLabel, new List<AnatomicalSite>(electrodes));
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
                                SitesPerReferential.Add(referentialLabel, new List<AnatomicalSite>(electrodes));
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
                Atlas = new MarsAtlas(Application.dataPath);
                Atlas.loadPatientAtlas(filePath);
                LinkAtlasData(Atlas.electrodes_Atlas);
            }

            yield return null;
        }

        private static void LinkAtlasData(MarsAtlas_plot[] atlas_Plots)
        {
            int elementCount = SitesPerReferential.Count;
            for (int i = 0; i < elementCount; i++)
            {
                KeyValuePair<string, List<AnatomicalSite>> kvp_sites = SitesPerReferential.ElementAt(i);
                List<AnatomicalSite> sites = kvp_sites.Value;
                for (int j = 0; j < sites.Count; j++)
                {
                    string siteLabel = sites[j].Label;
                    string IdString = new string(siteLabel.Where(char.IsDigit).ToArray());
                    if (int.TryParse(IdString, out int siteID))
                    {
                        string siteIDFormated = siteID.ToString("00");
                        siteLabel = CorrectPlotName(siteLabel.Replace(siteID.ToString(), siteIDFormated));
                    }

                    List<int> idFound = atlas_Plots.Select((item, index) => new { Item = item, Index = index })
                                            .Where(x => (x.Item.plotName.ToLower() == siteLabel))
                                            .Select(x => x.Index)
                                            .ToList();

                    if (idFound.Count > 0)
                    {
                        sites[j].MarsAtlas = atlas_Plots[idFound[0]].nameFull;
                        sites[j].Broadmann = atlas_Plots[idFound[0]].broadman;
                    }
                }
            }
        }

        private static string CorrectPlotName(string plot)
        {
            //== Correct if elec is named Pp1 (P'1)
            List<int> nbP = plot.ToLower().Select((v, ii) => new { v, ii })
            .Where(c => c.v.Equals('p'))
            .Select(c => c.ii).ToList();

            if (nbP.Count > 1)
            {
                var stringBuilder = new StringBuilder(plot);
                stringBuilder[nbP.Count - 1] = '\'';
                plot = stringBuilder.ToString();
            }
            //=========
            if (plot[0] == 'p' || plot[0] == 'P') //if electrode is p then just to lower case else change p for ' and to lower
                plot = plot.ToLower();
            else
                plot = plot.ToLower().Replace('p', '\'');

            string[] tempPlot = plot.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", tempPlot);
        }

        public static bool IsReferentialValid(string referential)
        {
            switch (referential)
            {
                case "MNI":
                    {
                        return SitesPerReferential.ContainsKey("MNI");
                    }
                case "PAT":
                    {
                        return SitesPerReferential.ContainsKey("PAT");
                    }
                case "ELEC":
                    {
                        return SitesPerReferential.ContainsKey("ELEC_0") || SitesPerReferential.ContainsKey("ELEC_1") || SitesPerReferential.ContainsKey("ELEC_2") ||
                               SitesPerReferential.ContainsKey("ELEC_3") || SitesPerReferential.ContainsKey("ELEC_4") || SitesPerReferential.ContainsKey("ELEC_5");
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static KeyValuePair<string, List<AnatomicalSite>> ReturnFirstValidSitesList()
        {
            return ReturnFirstValidSitesList(Session.Current);
        }

        public static KeyValuePair<string, List<AnatomicalSite>> ReturnFirstValidSitesList(Session session)
        {
            Dictionary<string, List<AnatomicalSite>> sites = session.SitesPerReferential;
            if (sites.ContainsKey("MNI")) return new KeyValuePair<string, List<AnatomicalSite>>("MNI", sites["MNI"]);
            else if (sites.ContainsKey("PAT")) return new KeyValuePair<string, List<AnatomicalSite>>("PAT", sites["PAT"]);
            else if (sites.ContainsKey("ELEC_0")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_0"]);
            else if (sites.ContainsKey("ELEC_1")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_1"]);
            else if (sites.ContainsKey("ELEC_2")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_2"]);
            else if (sites.ContainsKey("ELEC_3")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_3"]);
            else if (sites.ContainsKey("ELEC_4")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_4"]);
            else if (sites.ContainsKey("ELEC_5")) return new KeyValuePair<string, List<AnatomicalSite>>("ELEC", sites["ELEC_5"]);
            else return default;
        }

        public static List<AnatomicalSite> GetSitesListFrom(string referential, int fileId = -1)
        {
            return GetSitesListFrom(Session.Current, referential, fileId);
        }

        public static List<AnatomicalSite> GetSitesListFrom(Session session, string referential, int fileId = -1)
        {
            Dictionary<string, List<AnatomicalSite>> sites = session.SitesPerReferential;
            if (referential == "MNI" && sites.ContainsKey("MNI"))
            {
                return new List<AnatomicalSite>(sites["MNI"]);
            }
            else if (referential == "PAT" && sites.ContainsKey("PAT"))
            {
                return new List<AnatomicalSite>(sites["PAT"]);
            }
            else if (referential == "ELEC" && fileId != -1)
            {
                string key = "ELEC_" + fileId.ToString();
                bool hasKeyData = sites.ContainsKey(key);
                return hasKeyData ? new List<AnatomicalSite>(sites[key]) : default;
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
