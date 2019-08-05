using Assets.Scripts.Data.Factory;
using CielaSpike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    public class EventsToolbar2 : Toolbar
    {
        [SerializeField]
        Tools.EventsLoader m_EventsLoader = null;
        [SerializeField]
        Tools.EventsSaver m_EventsSaver = null;
        [SerializeField]
        Tools.EventsModifier m_EventsModifier = null;

        protected override void AddTools()
        {
            m_Tools.Add(m_EventsLoader);
            m_Tools.Add(m_EventsSaver);
            m_Tools.Add(m_EventsModifier);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            m_EventsLoader.LoadFile.AddListener(LoadFile);
            m_EventsSaver.SaveFile.AddListener(SaveFile);
            m_EventsModifier.AddEvents.AddListener(ToggleAddEvents);
            m_EventsModifier.ShowEvents.AddListener(ToggleShowEvents);
            m_EventsModifier.DeleteEvents.AddListener(DeleteEvents);
        }

        private void LoadFile(string filePath)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = 0,
                FilePathToLoad = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void SaveFile(string filePath)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = 1,
                FilePathToSave = filePath
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void ToggleAddEvents(bool isAddOn)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = 2,
                IsAddEventsOn = isAddOn
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }


        private void ToggleShowEvents(bool isShowOn)
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = 3,
                IsShowEventsOn = isShowOn
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }

        private void DeleteEvents()
        {
            UiToEventsMessage message = new UiToEventsMessage
            {
                TaskToExecute = 4
            };
            Messenger.Default.Send(message, MessageContext.UiToEvents);
        }
    }
}