using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Factory
{
    public interface IAnatomicalSiteContext
    {
        public List<AnatomicalSite> Electrodes { get; }
        public string FilePath { get; set; }
    }
}