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
    }
}