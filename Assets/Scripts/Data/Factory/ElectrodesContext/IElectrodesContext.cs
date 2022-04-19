using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Factory
{
    public interface IElectrodesContext
    {
        void LoadElectrodesOnBrain(GameObject parent, List<AnatomicalSite> sites);
    }
}
