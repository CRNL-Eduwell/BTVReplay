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
        [SerializeField] Tools.TfWindow _TfWindow = null;
        [SerializeField] Tools.TfAmplitude _TfAmplitude = null;

        private int m_CurrentTraceID = 0;
        private TfTraceOption m_Options = null;

        protected override void AddTools()
        {
            m_Tools.Add(_TraceToggler);
            m_Tools.Add(_TfWindow);
            m_Tools.Add(_TfAmplitude);
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
                _TfWindow.SetFrequencyBandWithoutNotify((int)m_Options.LowFrequency, (int)m_Options.HighFrequency);
                _TfWindow.SetTimePeriodWithoutNotify((int)m_Options.WindowInMilliseconds);
                _TfAmplitude.SetAmplitudeWithoutNotify(m_Options.MinValueFactor, m_Options.MaxValueFactor);
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
            
            _TfWindow.UpdateFrequencyBand.AddListener((lowValue, highValue) =>
            {
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 2,
                    ParentWindowIndex = m_CurrentTraceID,
                    LowFrequency = lowValue,
                    HighFrequency = highValue
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });

            _TfWindow.UpdateTfWindow.AddListener((windowValue) =>
            {
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 3,
                    ParentWindowIndex = m_CurrentTraceID,
                    WindowInMs = windowValue
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });

            _TfAmplitude.UpdateAmplitude.AddListener((min, max) =>
            {
                UiToTFEventsMessage message = new UiToTFEventsMessage
                {
                    TaskToExecute = 4,
                    ParentWindowIndex = m_CurrentTraceID,
                    MinValueFactor = min,
                    MaxValueFactor = max
                };
                Messenger.Default.Send(message, MessageContext.UiToTFEvents);
            });
        }
    }
}