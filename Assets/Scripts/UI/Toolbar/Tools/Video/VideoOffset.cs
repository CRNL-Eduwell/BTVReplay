using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class VideoOffset : Tool
    {
        public event offsetVideoChangedEventHandler offsetVideoHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Scrollbar m_ChangeOffset = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Text m_Label = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_AddOffset = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_RemoveOffset = null;
        /// <summary>
        /// </summary>
        private float m_Offset = 1;
        private EventTrigger m_EventTrigger = null;

        public override void Initialize()
        {
            m_AddOffset.onClick.AddListener(AddOffset);
            m_RemoveOffset.onClick.AddListener(RemoveOffset);
            
            //== Scrollbar callback
            m_EventTrigger = m_ChangeOffset.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) => { UpdateOffsetScrollbarCallback(); });
            m_EventTrigger.triggers.Add(entry);

            m_Label.text = "Offset : 00 m: 00 s: 00ms";
        }

        private void OnDestroy()
        {
            m_AddOffset.onClick.RemoveAllListeners();
            m_RemoveOffset.onClick.RemoveAllListeners();

            for (int i = 0; i < m_EventTrigger.triggers.Count; i++)
                m_EventTrigger.triggers[i].callback.RemoveAllListeners();
        }

        private void AddOffset()
        {
            if (m_Offset - 10 >= -60000)
            {
                m_Offset -= 10;
                m_ChangeOffset.value = ((m_Offset / 1000) / 120) + 0.5f;
                UpdateOffset(m_Offset);
            }
        }

        private void RemoveOffset()
        {
            if (m_Offset + 10 <= 60000)
            {
                m_Offset += 10;
                m_ChangeOffset.value = ((m_Offset / 1000) / 120) + 0.5f;
                UpdateOffset(m_Offset);
            }
        }

        private void UpdateOffset(float offset)
        {
            UpdateLabel(offset);
            offsetVideoHasChanged(offset);
        }

        void UpdateOffsetScrollbarCallback()
        {
            float offsetBar = m_ChangeOffset.value - 0.5f;
            m_Offset = (int)(offsetBar * 120) * 1000;
            UpdateOffset(m_Offset);
        }

        void UpdateLabel(float milliSec)
        {
            int m = ((int)milliSec / 1000) / 60;
            int s = ((int)milliSec / 1000) % 60;
            int ms = (int)milliSec - (((int)milliSec / 1000) * 1000);
            m_Label.text = "Offset : " + m + "m: " + s + "s:" + ms + "ms";
        }

    }
}
