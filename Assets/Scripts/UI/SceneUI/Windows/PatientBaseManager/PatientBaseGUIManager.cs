using Assets.Scripts.Data.Factory;
using BTV.Services.DatabaseService;
using BTV.UI;
using CielaSpike;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PatientBaseGUIManager : MonoBehaviour
{
    [SerializeField] Button m_Close = null;
    [SerializeField] ResizableGrid m_grid = null;
    [SerializeField] DatabaseList m_DatabaseList = null;
    [SerializeField] SubjectList m_SubjectList = null;
    [SerializeField] PatientGUIManager m_PatientManager = null;
    [SerializeField] Button m_LoadSubject = null;

    private SubjectRepository m_LastSelectedRepository = null;
    private Subject m_LastSelectedSUbject = null;
    private bool m_dbSwitch = false;

    private void Start()
    {
        m_PatientManager.IsInteractable = false;

        //Add element in service in they exist and replug listener for update
        foreach (var item in DatabaseService.Databases)
        {
            ((INotifyCollectionChanged)item.Subjects).CollectionChanged += UpdateSubjectCollection;
            m_DatabaseList.AddElement(item);
        }

        DatabaseService.Databases.CollectionChanged += UpdateDatabaseCollection;
        Messenger.Default.Register<FileMenuMessage>(this, OnFileMenuMessage, MessageContext.FileMenuMessage);
        Messenger.Default.Register<EditMenuMessage>(this, OnEditMenuMessage, MessageContext.EditMenuMessage);
        m_Close.onClick.AddListener(() => { Destroy(gameObject); });
        ((ISelectionCountable)m_DatabaseList).OnSelectionChanged.AddListener(UpdateShownDatabase);
        ((ISelectionCountable)m_SubjectList).OnSelectionChanged.AddListener(UpdateShownSubject);
        m_LoadSubject.onClick.AddListener(LoadSelectedSubject);

        ((ISelectionCountable)m_SubjectList).OnSelectionChanged.AddListener(OnSubjectSelectionChanged);
    }

    private void OnDestroy()
    {
        //unplug listener for update, otherwise weird fucking error
        foreach (var item in DatabaseService.Databases)
        {
            ((INotifyCollectionChanged)item.Subjects).CollectionChanged -= UpdateSubjectCollection;
        }

        DatabaseService.Databases.CollectionChanged -= UpdateDatabaseCollection;
        Messenger.Default.Unregister(this, MessageContext.FileMenuMessage);
        Messenger.Default.Unregister(this, MessageContext.EditMenuMessage);
        m_Close.onClick.RemoveAllListeners();
        ((ISelectionCountable)m_DatabaseList).OnSelectionChanged.RemoveAllListeners();
        ((ISelectionCountable)m_SubjectList).OnSelectionChanged.RemoveAllListeners();
        m_LoadSubject.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (m_grid.InitDone == false)
            InitDisplay();
    }

    //Warning : Good placement is due to the fact that PatientBaseGUIManager Start is called after the default
    //start and therefore the Start of VerticalHandler.cs is already executed , see to maybe change that
    //and init everything with init functions in all necessary classes
    private void InitDisplay()
    {
        UnityEngine.Debug.Log("Init");
        m_grid.Init();

        m_grid.VerticalHandlers[0].MagneticPosition = 0.25f;
        m_grid.VerticalHandlers[1].MagneticPosition = 0.5f;

        m_grid.VerticalHandlers[0].Position = 0.25f;
        m_grid.VerticalHandlers[1].Position = 0.5f;

        m_grid.SetVerticalHandlersPosition(0);
        m_grid.SetVerticalHandlersPosition(1);
        //
        m_grid.UpdateAnchors();
        m_grid.InitDone = true;
    }

    private void UpdateDatabaseCollection(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                UnityEngine.Debug.Log("Adding a Database element : ");
                SubjectRepository itemToAdd = (SubjectRepository)e.NewItems[0]; //list of new items, only one at a time normally
                ((INotifyCollectionChanged)itemToAdd.Subjects).CollectionChanged += UpdateSubjectCollection;
                m_DatabaseList.AddElement(itemToAdd);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                UnityEngine.Debug.Log("Moving a Database element : ");
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                UnityEngine.Debug.Log("Removing a Database element : ");
                SubjectRepository itemToRemove = (SubjectRepository)e.OldItems[0];
                ((INotifyCollectionChanged)itemToRemove.Subjects).CollectionChanged -= UpdateSubjectCollection;
                m_DatabaseList.RemoveElement(itemToRemove);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                UnityEngine.Debug.Log("Replacing a Database element : ");
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                UnityEngine.Debug.Log("Reseting a Database element : ");
                break;
        }
    }

    private void UpdateSubjectCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    UnityEngine.Debug.Log("Adding a Subject element : ");
                    Subject itemToAdd = (Subject)e.NewItems[0]; //list of new items, only one at a time normally
                    m_SubjectList.AddElement(itemToAdd);
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                {
                    UnityEngine.Debug.Log("Moving a Subject element : ");
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                {
                    UnityEngine.Debug.Log("Removing a Subject element : ");
                    Subject itemToRemove = (Subject)e.OldItems[0];
                    m_SubjectList.RemoveElement(itemToRemove);
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                {
                    //We only need to update the element in the graphical object in case of 
                    //a subject to subject switch, if it's a change of db we only need the 
                    //underlying collection to be updated
                    if (!m_dbSwitch)
                    {
                        UnityEngine.Debug.Log("Replacing a Subject element : ");
                        Subject itemToRemove = (Subject)e.OldItems[0];
                        Subject itemToAdd = (Subject)e.NewItems[0];
                        m_SubjectList.ReplaceElement(itemToRemove, itemToAdd);
                    }
                }
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                {
                    UnityEngine.Debug.Log("Reseting a Subject element : ");
                    break;
                }
        }
    }

    private void OnFileMenuMessage(FileMenuMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                {
                    DatabaseService.CreateNewDatabase(message.FilePath);
                    break;
                }
            case 1:
                {
                    DatabaseService.OpenDatabase(message.FilePath);
                    break;
                }
            case 2:
                {
                    SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
                    foreach (var element in SelectedElements)
                    {
                        element.Save();
                    }
                    break;
                }
            case 3:
                {
                    SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
                    if (SelectedElements.Length > 0)
                    {
                        var extensionList = new[] { new ExtensionFilter("BrainTV Database Files", "dbtv2") };
                        FileInfo file = new FileInfo(SelectedElements[0].FilePath);
#if UNITY_STANDALONE_OSX
                        FileBrowser.GetSavedFileNameAsync((str) =>
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                SelectedElements[0].Save(str);
                            }
                        }, extensionList, "Save Database To", file.FullName, file.Name);
#else
                        string filePath = FileBrowser.GetSavedFileName(extensionList, "Save Database To", file.FullName, file.Name);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            SelectedElements[0].Save(filePath);
                        }
#endif
                    }
                    break;
                }
            case 4:
                {
                    Destroy(gameObject);
                    break;
                }
        }
    }

    private void OnEditMenuMessage(EditMenuMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                {
                    RenameDatabase();
                    break;
                }
            case 1:
                {
                    CloseDatabase();
                    break;
                }
            case 2:
                {
                    RenameSubject();
                    break;
                }
            case 3:
                {
                    AddSubjectToDatabase();
                    break;
                }
            case 4:
                {
                    RemoveSubjectFromDatabase();
                    break;
                }
            case 5:
                {
                    MoveSubjectsToDatabase(message.DestinationDatabase);
                    break;
                }
            case 6:
                {
                    CopySubjectsToDatabase(message.DestinationDatabase);
                    break;
                }
        }
    }

    private void UpdateShownDatabase()
    {
        m_dbSwitch = true;
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        m_SubjectList.RemoveAllElements();
        if (SelectedElements.Length > 0)
        {
            m_SubjectList.AddElements(SelectedElements[0].Subjects.ToList());
        }
        OnDatabaseSelectionChanged();
        m_dbSwitch = false;
    }

    private void UpdateShownSubject()
    {
        Subject[] SelectedElements = m_SubjectList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            m_PatientManager.IsInteractable = true;
            m_PatientManager.SetSubjectToGUI(SelectedElements[0]);
        }
        else
        {
            m_PatientManager.IsInteractable = false;
            m_PatientManager.SetToDefault();
        }
    }

    public void OnDatabaseSelectionChanged()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        m_LastSelectedRepository = (SelectedElements.Length > 0) ? SelectedElements[0] : null;
    }

    public void OnSubjectSelectionChanged()
    {
        if (m_LastSelectedRepository != null && m_LastSelectedSUbject != null)
        {
            if (m_LastSelectedSUbject != m_PatientManager.LastSubject)
            {
                int repoIndex = DatabaseService.Databases.IndexOf(m_LastSelectedRepository);
                Subject updated = new Subject(m_PatientManager.LastSubject);
                Subject outdated = new Subject(m_LastSelectedSUbject);
                ApplicationState.displayConfirmation("Keep Modifications ?", "There seems to have been some modifications, do you want to save them ?",
                    () =>
                    {
                        DatabaseService.UpdateSubjectFromDatabase(repoIndex, outdated, updated);
                        DatabaseService.Databases[repoIndex].Save();
                    },
                    () => { });
            }
        }

        Subject[] SelectedElements = m_SubjectList.ObjectsSelected;
        m_LastSelectedSUbject = (SelectedElements.Length > 0) ? SelectedElements[0] : null;
    }

    private void RenameDatabase()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            FileInfo fileinfo = new FileInfo(SelectedElements[0].FilePath);
            string name = fileinfo.Name.Replace(".dbtv2", "");

            InputFieldWindow window = ApplicationState.SpawFrequencyChoiceWindow();
            window.Initialize("Database Name", "Choose a new name for your Database",
                () =>
            {
                DatabaseService.UpdateDatabaseName(SelectedElements[0], name, window.StringValue);
                window.Close();
            }, () =>
            {
                window.Close();
            });
            window.StringValue = name;
        }
    }

    private void CloseDatabase()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            ApplicationState.displayConfirmation("Database Closure", "You are going to close this database, are you sure ?", () => { DatabaseService.DeleteDatabase(SelectedElements[0]); }, () => { });
        }
    }

    private void RenameSubject()
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = m_SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                InputFieldWindow window = ApplicationState.SpawFrequencyChoiceWindow();
                window.Initialize("Subject Name", "Choose a new name for your Subject",
                    () =>
                    {
                        DatabaseService.EditSubjectName(SelectedDB[0], SelectedSubjects[0], window.StringValue);
                        m_PatientManager.UpdateSubjectName(window.StringValue);
                        window.Close();
                    }, () =>
                    {
                        window.Close();
                    });
                window.StringValue = SelectedSubjects[0].PatientName;
            }
        }
    }

    private void AddSubjectToDatabase()
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            DatabaseService.AddSubjectToDatabase(SelectedDB[0]);
        }
    }

    private void RemoveSubjectFromDatabase()
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = m_SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                ApplicationState.displayConfirmation("Subject Deletion", "You are going to erase this subject, are you sure ?", () => { DatabaseService.RemoveSubjectFromDatabase(SelectedDB[0], SelectedSubjects[0]); }, () => { });
            }
        }
    }

    private void MoveSubjectsToDatabase(string database)
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = m_SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                SubjectRepository destinationDb = m_DatabaseList.Objects.First(x => x.FilePath.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv2", "") == database);
                foreach (var subject in SelectedSubjects)
                {
                    bool added = DatabaseService.AddSubjectToDatabase(destinationDb, subject);
                    if (added)
                    {
                        DatabaseService.RemoveSubjectFromDatabase(SelectedDB[0], subject);
                    }
                    else
                    {
                        ApplicationState.displayMessage("Patient was not moved", "INFO", "Patient already exists in destination database");
                    }
                }
            }
        }
    }

    private void CopySubjectsToDatabase(string database)
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = m_SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                SubjectRepository destinationDb = m_DatabaseList.Objects.First(x => x.FilePath.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv2", "") == database);
                foreach (var subject in SelectedSubjects)
                {
                    bool added = DatabaseService.AddSubjectToDatabase(destinationDb, subject);
                    if (!added)
                    {
                        ApplicationState.displayMessage("Patient was not copied", "INFO", "Patient already exists in destination database");
                    }
                }
            }
        }
    }

    private void LoadSelectedSubject()
    {
        int repoIndex = DatabaseService.Databases.IndexOf(m_LastSelectedRepository);
        Subject updated = new Subject(m_PatientManager.GetSubjectsFromGUI());
        Subject outdated = new Subject(m_LastSelectedSUbject);

        if (updated != outdated)
        {
            ApplicationState.displayConfirmation("Keep Modifications ?", "There seems to have been some modifications, do you want to save them ?",
            () =>
            {
                m_dbSwitch = true;
                DatabaseService.UpdateSubjectFromDatabase(repoIndex, outdated, updated);
                DatabaseService.Databases[repoIndex].Save();
                m_dbSwitch = false;
                LoadSubject(updated);
            },
            () =>
            {
                LoadSubject(updated);
            });
        }
        else
        {
            LoadSubject(updated);
        }
    }

    private void LoadSubject(Subject updated)
    {
        if (updated.IsLoadable)
        {
            LoadSubjectMessage message = new LoadSubjectMessage
            {
                subject = new Subject(updated),
                label = m_PatientManager.GetCurrentExperimentName()
            };
            Messenger.Default.Send(message, MessageContext.LoadSubjectMessage);
            Destroy(gameObject);
        }
        else
        {
            ApplicationState.displayMessage("Can not load Subject", "NOK", "Please check that all your eeg files exists at the given path and have the correct extensions");
        }
    }
}
