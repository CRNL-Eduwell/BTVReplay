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
            Initialize();
        }

        /// <summary>
        /// Add the listeners for this tool. Kept as the derived-class hook while callers migrate
        /// to the session-aware overload.
        /// </summary>
        public abstract void Initialize();
    }
}
