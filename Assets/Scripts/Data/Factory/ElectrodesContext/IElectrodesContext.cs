using System.Collections.Generic;
using UnityEngine;
using BTV.Services;

namespace Assets.Scripts.Data.Factory
{
    public interface IElectrodesContext
    {
        void LoadElectrodesOnBrain(Session patientSession, GameObject parent, List<AnatomicalSite> sites);
    }
}
