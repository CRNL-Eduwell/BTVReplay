using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Factory
{
    public interface IElectrodesContext
    {
        List<object> Electrodes
        {
            get;
        }
        void LoadElectrodes(string pathPts);
        void LoadAtlasData(string pathAtlasCsv);
        void LoadElectrodesOnBrain(GameObject parent);
        void LoadDefaultPearl();
        void UpdateElectrodesPosition(GameObject parent);
        void UpdateElectrodesPearl(GameObject parent);
    }
}
