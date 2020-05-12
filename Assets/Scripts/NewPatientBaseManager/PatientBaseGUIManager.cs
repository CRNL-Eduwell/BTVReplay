using Assets.Scripts.Data.Factory;
using BTV.Services.DatabaseService;
using BTV.UI;
using CielaSpike;
using SFB;
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

    private void Start()
    {
        //Add element in service in they exist
        foreach (var item in DatabaseService.Databases)
        {
            m_DatabaseList.AddElement(item);
        }

        DatabaseService.Databases.CollectionChanged += UpdateDatabaseCollection;
        Messenger.Default.Register<FileMenuMessage>(this, OnFileMenuMessage, MessageContext.FileMenuMessage);
        Messenger.Default.Register<EditMenuMessage>(this, OnEditMenuMessage, MessageContext.EditMenuMessage);
        m_Close.onClick.AddListener(() => { Destroy(gameObject); });
        m_DatabaseList.OnSelectionChanged.AddListener(UpdateShownDatabase);
        m_SubjectList.OnSelectionChanged.AddListener(UpdateShownSubject);
        m_LoadSubject.onClick.AddListener(LoadSelectedPatient);

        //TODO
        m_DatabaseList.OnSelectionChanged.AddListener(OnDatabaseSelectionChanged);
        m_SubjectList.OnSelectionChanged.AddListener(OnSubjectSelectionChanged);

    }

    public void OnDatabaseSelectionChanged()
    {
        SubjectRepository[] list = m_DatabaseList.ObjectsSelected;
        if (list.Length > 0)
        {
            m_LastSelectedRepository = list[0];
        }
        else
        {
            m_LastSelectedRepository = null;
        }
    }

    public void OnSubjectSelectionChanged()
    {
        if (m_LastSelectedSUbject != null)
        {
            m_PatientManager.LastSubject.PatientName = m_LastSelectedSUbject.PatientName;
            if (m_LastSelectedSUbject != m_PatientManager.LastSubject)
            {
                Subject old = new Subject(m_LastSelectedSUbject); //make a copy of the previously selected object before the selection change items
                UnityEngine.Debug.Log("Need to check if " + m_LastSelectedSUbject.PatientName + " has been modified");
                ApplicationState.displayConfirmation("Keep Modifications ?", "There seems to have been some modifications, do you want to save them ?",
                    () =>
                    {
                        Subject replacement = new Subject(m_PatientManager.LastSubject);
                        DatabaseService.UpdateSubjectFromDatabase(m_LastSelectedRepository, old, replacement);
                    },
                    () => { });
            }
        }

        Subject[] list = m_SubjectList.ObjectsSelected;
        if (list.Length > 0)
        {
            m_LastSelectedSUbject = list[0];
        }
        else
        {
            m_LastSelectedSUbject = null;
        }
    }

    private void OnDestroy()
    {
        DatabaseService.Databases.CollectionChanged -= UpdateDatabaseCollection;
        Messenger.Default.Unregister(this, MessageContext.FileMenuMessage);
        Messenger.Default.Unregister(this, MessageContext.EditMenuMessage);
        m_Close.onClick.RemoveAllListeners();
        m_DatabaseList.OnSelectionChanged.RemoveAllListeners();
        m_SubjectList.OnSelectionChanged.RemoveAllListeners();
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
                    UnityEngine.Debug.Log("Replacing a Subject element : ");
                    Subject itemToRemove = (Subject)e.OldItems[0];
                    Subject itemToAdd = (Subject)e.NewItems[0];
                    m_SubjectList.ReplaceElement(itemToRemove, itemToAdd);
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
                        var extensionList = new[] { new ExtensionFilter("BrainTV Database Files", "dbtv") };
                        FileInfo file = new FileInfo(SelectedElements[0].FilePath);
                        string filePath = FileBrowser.GetSavedFileName(extensionList, "Save Database To", file.FullName, file.Name);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            SelectedElements[0].Save(filePath);
                        }
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
                    EditDatabaseName();
                    break;
                }
            case 1:
                {
                    DeleteDatabase();
                    break;
                }
            case 2:
                {
                    EditSubjectName();
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
        }
    }

    private void UpdateShownDatabase()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        m_SubjectList.RemoveAllElements();
        if (SelectedElements.Length > 0)
        {
            m_SubjectList.AddElements(SelectedElements[0].Subjects.ToList());
        }
    }

    private void UpdateShownSubject()
    {
        Subject[] SelectedElements = m_SubjectList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            m_PatientManager.SetSubjectToGUI(SelectedElements[0]);
        }
        else
        {
            m_PatientManager.SetToDefault();
        }
    }

    private void EditDatabaseName()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            FileInfo fileinfo = new FileInfo(SelectedElements[0].FilePath);
            string name = fileinfo.Name.Replace(".dbtv", "");

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
        }
    }

    private void DeleteDatabase()
    {
        SubjectRepository[] SelectedElements = m_DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            ApplicationState.displayConfirmation("Database Deletion", "You are going to erase this database, are you sure ?", () => { DatabaseService.DeleteDatabase(SelectedElements[0]); }, () => { });
        }
    }

    private void EditSubjectName()
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

    private void LoadSelectedPatient()
    {
        SubjectRepository[] SelectedDB = m_DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = m_SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                LoadSubjectMessage message = new LoadSubjectMessage
                {
                    subject = new Subject(SelectedSubjects[0])
                };
                Messenger.Default.Send(message, MessageContext.LoadSubjectMessage);
                Destroy(gameObject);
            }
        }
    }
}
