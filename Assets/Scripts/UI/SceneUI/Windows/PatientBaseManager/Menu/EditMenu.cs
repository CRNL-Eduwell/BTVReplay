using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI
{
    public class EditMenu : Menu
    {
        [SerializeField]
        private Button m_EditDatabaseName = null;
        [SerializeField]
        private Button m_DeleteDatabase = null;
        [SerializeField]
        private Button m_EditSubjectName = null;
        [SerializeField]
        private Button m_AddSubject = null;
        [SerializeField]
        private Button m_DeleteSubject = null;

        private void Start()
        {
            m_EditDatabaseName.onClick.AddListener(EditDabaseName);
            m_DeleteDatabase.onClick.AddListener(DeleteDatabase);
            m_EditSubjectName.onClick.AddListener(EditSubjectName);
            m_AddSubject.onClick.AddListener(AddSubject);
            m_DeleteSubject.onClick.AddListener(DeleteSubject);
        }

        private void OnDestroy()
        {
            m_EditDatabaseName.onClick.RemoveAllListeners();
            m_DeleteDatabase.onClick.RemoveAllListeners();
            m_EditSubjectName.onClick.RemoveAllListeners();
            m_AddSubject.onClick.RemoveAllListeners();
            m_DeleteSubject.onClick.RemoveAllListeners();
        }

        private void EditDabaseName()
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 0
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            Close();
        }

        private void DeleteDatabase()
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 1
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            Close();
        }

        private void EditSubjectName()
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 2
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            Close();
        }

        private void AddSubject()
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 3
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            Close();
        }

        private void DeleteSubject()
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 4
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            Close();
        }
    }
}