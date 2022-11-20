using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    public class TFToolbar : Toolbar
    {
        [SerializeField] Toggle m_LockCursorsToggle = null;

        protected override void AddTools()
        {
            //m_Tools.Add(m_LockCursorsToggle);
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
        }
    }
}