using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    public class TFToolbar : Toolbar
    {
        [SerializeField] Toggle m_LockCursorsToggle = null;
        [SerializeField] Tools.TfTraceToggler _TraceToggler = null;
        [SerializeField] Slider _AlphaSlider = null;
        [SerializeField] Slider _FrequencySlider = null;

        private int m_CurrentTraceID = 0;
        private TfTraceOption m_Options = null;

        protected override void AddTools()
        {
            m_Tools.Add(_TraceToggler);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_LockCursorsToggle.onValueChanged.AddListener((IsOn) =>
            {
                UnityEngine.Debug.Log("Toggle TF Cursors Lock");
                //Send Message
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 0,
                    IsSlaved = IsOn
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });

            _TraceToggler.SelectedTrace.AddListener((traceID) =>
            {
                m_CurrentTraceID = traceID;
                m_Options = TimeFrequencyService.GetOptionsFor(m_CurrentTraceID);
                _AlphaSlider.SetValueWithoutNotify(m_Options.Alpha);
                _FrequencySlider.SetValueWithoutNotify(m_Options.FrequencySlider);
            });

            _AlphaSlider.onValueChanged.AddListener((sliderValue) =>
            {
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 1,
                    ParentWindowIndex = m_CurrentTraceID,
                    Alpha = sliderValue
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });
            
            _FrequencySlider.onValueChanged.AddListener((sliderValue) =>
            {
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 2,
                    ParentWindowIndex = m_CurrentTraceID,
                    FrequencySlider = sliderValue
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });
        }
    }
}