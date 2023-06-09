using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    public class ToolbarMenu : MonoBehaviour
    {
        /// <summary>
        /// Currently used toolbar
        /// </summary>
        public Toolbar CurrentToolbar { get; set; }

        /// <summary>
        /// </summary>
        public BrainToolbar BrainToolBar
        {
            get { return m_BrainToolbar; }
        }
        [SerializeField] BrainToolbar m_BrainToolbar = null;
        /// <summary>
        /// </summary>
        public EegSignalToolbar EegSignal1ToolBar
        {
            get { return m_EegSignal1Toolbar; }
        }
        [SerializeField] EegSignalToolbar m_EegSignal1Toolbar = null;
        /// <summary>
        /// </summary>
        public EegSignalToolbar EegSignal2ToolBar
        {
            get { return m_EegSignal2Toolbar; }
        }
        [SerializeField] EegSignalToolbar m_EegSignal2Toolbar = null;
        /// <summary>
        /// </summary>
        public CompPerformanceToolbar PerformanceToolBar
        {
            get { return m_PerformanceToolbar; }
        }
        [SerializeField] CompPerformanceToolbar m_PerformanceToolbar = null;
        /// <summary>
        /// </summary>
        public VideoToolbar VideoToolBar
        {
            get { return m_VideoToolbar; }
        }
        [SerializeField] VideoToolbar m_VideoToolbar = null;
        /// <summary>
        /// </summary>
        public EventsToolbar EventsToolBar
        {
            get { return m_EventsToolbar; }
        }
        [SerializeField] EventsToolbar m_EventsToolbar = null;
        /// <summary>
        /// </summary>
        public TFToolbar TimeFrequencyToolBar
        {
            get { return m_TfToolbar; }
        }
        [SerializeField] TFToolbar m_TfToolbar = null;
        /// <summary>
        /// </summary>
        public LayoutsToolbar LayoutsToolbar
        {
            get { return m_LayoutsToolbar; }
        }
        [SerializeField] LayoutsToolbar m_LayoutsToolbar = null;
        public MontageToolbar MontageToolbar
        {
            get { return m_MontageToolbar; }
        }
        [SerializeField] MontageToolbar m_MontageToolbar = null;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the toolbar menu
        /// </summary>
        private void Initialize()
        {
            m_BrainToolbar.Initialize();
            m_EegSignal1Toolbar.Initialize();
            m_EegSignal2Toolbar.Initialize();
            m_PerformanceToolbar.Initialize();
            m_VideoToolbar.Initialize();
            m_EventsToolbar.Initialize();
            m_TfToolbar.Initialize();
            m_LayoutsToolbar.Initialize();
            m_MontageToolbar.Initialize();

            CurrentToolbar = null;

            m_BrainToolbar.gameObject.SetActive(false);
            m_EegSignal1Toolbar.gameObject.SetActive(false);
            m_EegSignal2Toolbar.gameObject.SetActive(false);
            m_PerformanceToolbar.gameObject.SetActive(false);
            m_VideoToolbar.gameObject.SetActive(false);
            m_EventsToolbar.gameObject.SetActive(false);
            m_TfToolbar.gameObject.SetActive(false);
            m_LayoutsToolbar.gameObject.SetActive(false);
            m_MontageToolbar.gameObject.SetActive(false);
        }
    }
}