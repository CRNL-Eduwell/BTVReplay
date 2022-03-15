using Assets.Scripts.Data.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTV.Services.AnatomicalDataService
{
    public static class AnatomicalDataService
    {
        private static Dictionary<string, List<AnatomicalSite>> m_SitesPerReferential = new Dictionary<string, List<AnatomicalSite>>();

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

        public static KeyValuePair<string, List<AnatomicalSite>> ReturnFirstValidSitesList()
        {
            if (m_SitesPerReferential.ContainsKey("MNI")) return new KeyValuePair<string, List<AnatomicalSite>>("MNI", m_SitesPerReferential["MNI"]);
            else if (m_SitesPerReferential.ContainsKey("PAT")) return new KeyValuePair<string, List<AnatomicalSite>>("PAT", m_SitesPerReferential["PAT"]);

            return default;
        }
    }
}