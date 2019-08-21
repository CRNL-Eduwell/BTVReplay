using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class EventsModifier : Tool
    {
        public GenericEvent<bool> AddEvents = new GenericEvent<bool>();
        public GenericEvent<bool> ShowEvents = new GenericEvent<bool>();
        public UnityEvent DeleteEvents = new UnityEvent();

        [SerializeField]
        private Toggle m_AddEvents = null;
        [SerializeField]
        private Toggle m_ShowEvents = null;
        [SerializeField]
        private Button m_DeleteEvents = null;

        public override void Initialize()
        {
            m_AddEvents.onValueChanged.AddListener((IsAddOn) =>
            {
                AddEvents.Invoke(IsAddOn);
            });
            m_ShowEvents.onValueChanged.AddListener((IsShowOn) =>
            {
                AddEvents.Invoke(IsShowOn);
            });
            m_DeleteEvents.onClick.AddListener(() =>
            {
                DeleteEvents.Invoke();
            });
        }
    }
}
