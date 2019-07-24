using UnityEngine;

namespace BTV.UI.Module3D.Tools
{
    public abstract class Tool : MonoBehaviour
    {
        /// <summary>
        /// Add the listener to this tool
        /// </summary>
        public abstract void Initialize();
    }
}
