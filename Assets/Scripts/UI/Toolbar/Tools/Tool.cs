using System;
using BTV.Services;
using UnityEngine;

namespace BTV.UI.Module3D.Tools
{
    public abstract class Tool : MonoBehaviour
    {
        public Session PatientSession { get; private set; }

        /// <summary>
        /// Bind this tool to the patient lifetime owned by its toolbar, then install listeners.
        /// </summary>
        public void Initialize(Session session)
        {
            PatientSession = session ?? throw new ArgumentNullException(nameof(session));
            OnInitialize();
        }

        /// <summary>
        /// Add the listeners for this tool after its patient session has been bound.
        /// </summary>
        protected abstract void OnInitialize();
    }
}
