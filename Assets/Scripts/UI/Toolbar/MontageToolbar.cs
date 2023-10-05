using UnityEngine;

namespace BTV.UI.Module3D
{
    public class MontageToolbar : Toolbar
    {
        [SerializeField] Tools.MontageManager m_MontageManager;
        [SerializeField] Tools.MontageLoaderSaver m_MontageLoaderSaver;

        protected override void AddTools()
        {
            m_Tools.Add(m_MontageManager);
            m_Tools.Add(m_MontageLoaderSaver);
        }

        protected override void AddListeners()
        {
            base.AddListeners();
        }
    }
}