using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{ 
    public abstract class Toolbar : MonoBehaviour
    {
        /// <summary>
        /// List of the tools of the toolbar
        /// </summary>
        protected List<Tools.Tool> m_Tools = new List<Tools.Tool>();

        /// <summary>
        /// Initialize the toolbar
        /// </summary>
        public void Initialize()
        {
            AddTools();
            AddListeners();
        }

        /// <summary>
        /// Link elements to the toolbar
        /// </summary>
        /// <param name="parent">Transform of the toolbar</param>
        protected abstract void AddTools();

        protected virtual void AddListeners()
        {
            foreach (Tools.Tool tool in m_Tools)
            {
                tool.Initialize();
            }
        }
    }
}
