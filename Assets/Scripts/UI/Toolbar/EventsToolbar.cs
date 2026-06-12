using System.Collections.Generic;
using UnityEngine;

namespace BTV.UI.Module3D
{
    public class EventsToolbar : Toolbar
    {
        [SerializeField]
        Tools.EventsOfEegFileLoader m_EventsOfFileLoader = null;
        [SerializeField]
        Tools.EventsLoader m_EventsLoader = null;
        [SerializeField]
        Tools.EventsSaver m_EventsSaver = null;
        [SerializeField]
        Tools.EventsModifier m_EventsModifier = null;
        [SerializeField]
        Tools.EventsMatching m_EventMatching = null;

        protected override void AddTools()
        {
            m_Tools.Add(m_EventsOfFileLoader);
            m_Tools.Add(m_EventsLoader);
            m_Tools.Add(m_EventsSaver);
            m_Tools.Add(m_EventsModifier);
            m_Tools.Add(m_EventMatching);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_EventsOfFileLoader.LoadEvents.AddListener(LoadEventsList);
            m_EventsLoader.LoadFile.AddListener(LoadFile);
            m_EventsSaver.SaveFile.AddListener(SaveFile);
            m_EventsModifier.AddEvents.AddListener(ToggleAddEvents);
            m_EventsModifier.ShowEvents.AddListener(ToggleShowEvents);
            m_EventsModifier.DeleteEvents.AddListener(DeleteEvents);
            m_EventMatching.LoadFile.AddListener(LoadCodeMatchingFile);
        }

        private void LoadEventsList(List<Data.BtvEvent> btvEvents)
        {
            foreach (Data.BtvEvent _event in btvEvents)
            {
                EventsModificationMessage hackMessage = new EventsModificationMessage
                {
                    TaskToExecute = EventsModificationMessage.Task.AddEvent,
                    Event = _event
                };
                Messenger.Default.Send(hackMessage, MessageContext.EventsModificationMessage);
            }
        }

        private void LoadFile(string filePath)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.LoadEventsFile,
                FilePathToLoad = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void SaveFile(string filePath)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.SaveEventsFile,
                FilePathToSave = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void ToggleAddEvents(bool isAddOn)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.ToggleAddEvents,
                IsAddEventsOn = isAddOn
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void ToggleShowEvents(bool isShowOn)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.ToggleShowEvents,
                IsShowEventsOn = isShowOn
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void DeleteEvents()
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.DeleteSelectedEvents
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void LoadCodeMatchingFile(string filePath)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = UiToEventsMessage.Task.LoadCodeMatchingFile,
                FilePathToLoad = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }
    }
}