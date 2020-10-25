using BTV.Services.DatabaseService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField]
        private Button m_MoveSubject = null;
        [SerializeField]
        private DbSubMenu m_MoveSubjectsSubMenu = null;
        [SerializeField]
        private Button m_CopySubject = null;
        [SerializeField]
        private DbSubMenu m_CopySubjectsSubMenu = null;

        private void Start()
        {
            m_EditDatabaseName.onClick.AddListener(EditDabaseName);
            m_DeleteDatabase.onClick.AddListener(DeleteDatabase);
            m_EditSubjectName.onClick.AddListener(EditSubjectName);
            m_AddSubject.onClick.AddListener(AddSubject);
            m_DeleteSubject.onClick.AddListener(DeleteSubject);
            m_MoveSubject.onClick.AddListener(()=> 
            {
                m_MoveSubjectsSubMenu.Show = !m_MoveSubjectsSubMenu.Show;
                m_CopySubjectsSubMenu.Show = false;
            });
            m_MoveSubjectsSubMenu.ItemClicked.AddListener(MoveSubjectsToDatabase);
            m_CopySubject.onClick.AddListener(() => 
            {
                m_CopySubjectsSubMenu.Show = !m_CopySubjectsSubMenu.Show;
                m_MoveSubjectsSubMenu.Show = false;
            });
            m_CopySubjectsSubMenu.ItemClicked.AddListener(CopySubjectsToDatabase);

            DatabaseService.Databases.CollectionChanged += UpdateDatabaseCollection;
        }

        private void OnDestroy()
        {
            m_EditDatabaseName.onClick.RemoveAllListeners();
            m_DeleteDatabase.onClick.RemoveAllListeners();
            m_EditSubjectName.onClick.RemoveAllListeners();
            m_AddSubject.onClick.RemoveAllListeners();
            m_DeleteSubject.onClick.RemoveAllListeners();
            m_MoveSubject.onClick.RemoveAllListeners();
            m_MoveSubjectsSubMenu.ItemClicked.RemoveAllListeners();
            m_CopySubject.onClick.RemoveAllListeners();
            m_CopySubjectsSubMenu.ItemClicked.RemoveAllListeners();

            DatabaseService.Databases.CollectionChanged -= UpdateDatabaseCollection;
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

        private void MoveSubjectsToDatabase(string databaseName)
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 5,
                DestinationDatabase = databaseName
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            m_MoveSubjectsSubMenu.Show = false;
            Close();
        }

        private void CopySubjectsToDatabase(string databaseName)
        {
            EditMenuMessage message = new EditMenuMessage
            {
                TaskToExecute = 6,
                DestinationDatabase = databaseName
            };
            Messenger.Default.Send(message, MessageContext.EditMenuMessage);
            m_CopySubjectsSubMenu.Show = false;
            Close();
        }

        private void UpdateDatabaseCollection(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        UnityEngine.Debug.Log("Adding a Database element : ");
                        SubjectRepository itemToAdd = (SubjectRepository)e.NewItems[0]; //list of new items, only one at a time normally
                        m_MoveSubjectsSubMenu.AddSubMenuItem(itemToAdd);
                        m_CopySubjectsSubMenu.AddSubMenuItem(itemToAdd);
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        UnityEngine.Debug.Log("Moving a Database element : ");
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        UnityEngine.Debug.Log("Removing a Database element : ");
                        SubjectRepository itemToRemove = (SubjectRepository)e.OldItems[0];
                        m_MoveSubjectsSubMenu.RemoveSubMenuItem(itemToRemove);
                        m_CopySubjectsSubMenu.RemoveSubMenuItem(itemToRemove);
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        UnityEngine.Debug.Log("Replacing a Database element : ");
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        UnityEngine.Debug.Log("Reseting a Database element : ");
                        break;
                    }
            }
        }
    }
}