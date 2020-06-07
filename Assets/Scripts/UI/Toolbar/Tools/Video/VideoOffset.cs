using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BrainTV.Tools.NumberExtensions;

namespace BTV.UI.Module3D.Tools
{
    public delegate void offsetVideoChangedEventHandler(float newVal);

    public class VideoOffset : Tool
    {  
        public event offsetVideoChangedEventHandler offsetVideoHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_OffsetMinutes = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_OffsetSeconds = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_OffsetMilliSeconds = null;
        /// <summary>
        /// </summary>
        private float m_OffsetMemory = 0;

        public override void Initialize()
        {
            m_OffsetMinutes.text = "00";
            m_OffsetSeconds.text = "00";
            m_OffsetMilliSeconds.text = "00";

            m_OffsetMinutes.onEndEdit.AddListener(CheckOffsetInput);
            m_OffsetSeconds.onEndEdit.AddListener(CheckOffsetInput);
            m_OffsetMilliSeconds.onEndEdit.AddListener(CheckOffsetInput);
        }

        private void OnDestroy()
        {
            m_OffsetMinutes.onEndEdit.RemoveAllListeners();
            m_OffsetSeconds.onEndEdit.RemoveAllListeners();
            m_OffsetMilliSeconds.onEndEdit.RemoveAllListeners();
        }

        private void CheckOffsetInput(string str)
        {
            if (string.IsNullOrEmpty(str)) ConvertTotextValues(m_OffsetMemory);
            bool isParsed = str.TryParseInt(out int result);
            if (!isParsed) ConvertTotextValues(m_OffsetMemory);

            m_OffsetMemory = GetMillisecondsValue();
            offsetVideoHasChanged(m_OffsetMemory);
        }

        private void ConvertTotextValues(float milliSeconds)
        {
            int m = ((int)milliSeconds / 1000) / 60;
            m_OffsetMinutes.text = m.ToString();
            int s = ((int)milliSeconds / 1000) % 60;
            m_OffsetSeconds.text = s.ToString();
            int ms = (int)milliSeconds - (((int)milliSeconds / 1000) * 1000);
            m_OffsetMilliSeconds.text = ms.ToString();
        }

        private int GetMillisecondsValue()
        {
            bool isMinOk = m_OffsetMinutes.text.TryParseInt(out int m);
            bool isSecOk = m_OffsetSeconds.text.TryParseInt(out int s);
            bool isMsOk = m_OffsetMilliSeconds.text.TryParseInt(out int ms);

            if (isMinOk && isSecOk && isMsOk)
            {
                return (m * 60 * 1000) + (s * 1000) + ms;
            }
            else
            {
                UnityEngine.Debug.LogError("offset value not correct, reseting to default 00mn:00sec:00ms");
                return 0;
            }
        }
    }
}
