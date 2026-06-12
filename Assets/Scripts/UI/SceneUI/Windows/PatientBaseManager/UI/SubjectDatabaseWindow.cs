using SFB;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using BTV.UI;
using BTV.Services.DatabaseService;
using UnityEngine;
using UnityEngine.UI;

public class SubjectDatabaseWindow : MonoBehaviour
{
    [SerializeField] Button _CloseWindow = null;
    [SerializeField] ResizableGrid _ResizableGrid = null;
    [SerializeField] DatabaseList _DatabaseList = null;
    [SerializeField] SubjectList _SubjectList = null;
    [SerializeField] SubjectWidget _SubjectWidget = null;
    [SerializeField] Button _LoadSubject = null;

    private SubjectRepository m_LastSelectedRepository = null;
    private bool m_dbSwitch = false;

    private void Start()
    {
        _CloseWindow.onClick.AddListener(() => { Destroy(gameObject); });
        Messenger.Default.Register<FileMenuMessage>(this, OnFileMenuMessage, MessageContext.FileMenuMessage);
        Messenger.Default.Register<EditMenuMessage>(this, OnEditMenuMessage, MessageContext.EditMenuMessage);

        //Add element in service in they exist and replug listener for update
        DatabaseService.Databases.CollectionChanged += UpdateDatabaseCollection;
        foreach (var item in DatabaseService.Databases)
        {
            ((INotifyCollectionChanged)item.Subjects).CollectionChanged += UpdateSubjectCollection;
            _DatabaseList.AddElement(item);
        }

        ((ISelectionCountable)_DatabaseList).OnSelectionChanged.AddListener(UpdateShownDatabase);
        ((ISelectionCountable)_SubjectList).OnSelectionChanged.AddListener(OnSubjectSelectionChanged);
        _LoadSubject.onClick.AddListener(LoadSelectedSubject);

        StartCoroutine(InitDisplayWhenRendered());
    }

    private void OnDestroy()
    {
        _LoadSubject.onClick.RemoveAllListeners();
        ((ISelectionCountable)_SubjectList).OnSelectionChanged.RemoveAllListeners();
        ((ISelectionCountable)_DatabaseList).OnSelectionChanged.RemoveAllListeners();

        //unplug listener for update, otherwise weird fucking error
        foreach (var item in DatabaseService.Databases)
        {
            ((INotifyCollectionChanged)item.Subjects).CollectionChanged -= UpdateSubjectCollection;
        }
        DatabaseService.Databases.CollectionChanged -= UpdateDatabaseCollection;

        Messenger.Default.Unregister(this, MessageContext.FileMenuMessage);
        Messenger.Default.Unregister(this, MessageContext.EditMenuMessage);
        _CloseWindow.onClick.RemoveAllListeners();
    }

    // Init once the grid has a valid (non-zero) rect, then stop. Replaces a per-frame
    // InitDone poll in Update() that kept running for the whole lifetime of the window.
    private IEnumerator InitDisplayWhenRendered()
    {
        yield return new WaitUntil(() =>
            _ResizableGrid.RectTransform.rect.width > 0 && _ResizableGrid.RectTransform.rect.height > 0);
        if (_ResizableGrid.InitDone == false)
            InitDisplay();
    }

    //Warning : Good placement is due to the fact that PatientBaseGUIManager Start is called after the default
    //start and therefore the Start of VerticalHandler.cs is already executed , see to maybe change that
    //and init everything with init functions in all necessary classes
    private void InitDisplay()
    {
        BtvLog.Log("Init");
        _ResizableGrid.Init();

        _ResizableGrid.VerticalHandlers[0].MagneticPosition = 0.25f;
        _ResizableGrid.VerticalHandlers[1].MagneticPosition = 0.5f;

        _ResizableGrid.VerticalHandlers[0].Position = 0.25f;
        _ResizableGrid.VerticalHandlers[1].Position = 0.5f;

        _ResizableGrid.SetVerticalHandlersPosition(0);
        _ResizableGrid.SetVerticalHandlersPosition(1);
        //
        _ResizableGrid.UpdateAnchors();
        _ResizableGrid.InitDone = true;
    }

    #region Menu
    private void OnFileMenuMessage(FileMenuMessage message)
    {
        switch (message.TaskToExecute)
        {
            case FileMenuMessage.Task.NewDatabase:
                {
                    DatabaseService.CreateNewDatabase(message.FilePath);
                    break;
                }
            case FileMenuMessage.Task.OpenDatabase:
                {
                    DatabaseService.OpenDatabase(message.FilePath);
                    break;
                }
            case FileMenuMessage.Task.Save:
                {
                    SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
                    foreach (var element in SelectedElements)
                    {
                        element.Save();
                    }
                    break;
                }
            case FileMenuMessage.Task.SaveAs:
                {
                    SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
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
            case FileMenuMessage.Task.Exit:
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
            case EditMenuMessage.Task.RenameDatabase:
                {
                    RenameDatabase();
                    break;
                }
            case EditMenuMessage.Task.CloseDatabase:
                {
                    CloseDatabase();
                    break;
                }
            case EditMenuMessage.Task.RenameSubject:
                {
                    RenameSubject();
                    break;
                }
            case EditMenuMessage.Task.AddSubject:
                {
                    AddSubjectToDatabase();
                    break;
                }
            case EditMenuMessage.Task.DeleteSubject:
                {
                    RemoveSubjectFromDatabase();
                    break;
                }
            case EditMenuMessage.Task.MoveSubjects:
                {
                    MoveSubjectsToDatabase(message.DestinationDatabase);
                    break;
                }
            case EditMenuMessage.Task.CopySubjects:
                {
                    CopySubjectsToDatabase(message.DestinationDatabase);
                    break;
                }
        }
    }
    #endregion

    #region EditMenu
    private void UpdateShownDatabase()
    {
        m_dbSwitch = true;
        SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
        _SubjectList.RemoveAllElements();
        if (SelectedElements.Length > 0)
        {
            _SubjectList.AddElements(SelectedElements[0].Subjects.ToList());
        }
        OnDatabaseSelectionChanged();
        m_dbSwitch = false;
    }

    public void OnDatabaseSelectionChanged()
    {
        SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
        m_LastSelectedRepository = (SelectedElements.Length > 0) ? SelectedElements[0] : null;
    }

    private void RenameDatabase()
    {
        SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            FileInfo fileinfo = new FileInfo(SelectedElements[0].FilePath);
            string name = fileinfo.Name.Replace(".dbtv2", "");

            InputFieldWindow window = ApplicationState.SpawnFrequencyChoiceWindow();
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
        SubjectRepository[] SelectedElements = _DatabaseList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            ApplicationState.displayConfirmation("Database Closure", "You are going to close this database, are you sure ?", () => { DatabaseService.DeleteDatabase(SelectedElements[0]); }, () => { });
        }
    }

    private void RenameSubject()
    {
        SubjectRepository[] SelectedDB = _DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = _SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                InputFieldWindow window = ApplicationState.SpawnFrequencyChoiceWindow();
                window.Initialize("Subject Name", "Choose a new name for your Subject",
                    () =>
                    {
                        DatabaseService.EditSubjectName(SelectedDB[0], SelectedSubjects[0], window.StringValue);
                        _SubjectWidget.Subject.PatientName = window.StringValue;
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
        SubjectRepository[] SelectedDB = _DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            DatabaseService.AddSubjectToDatabase(SelectedDB[0]);
        }
    }

    private void RemoveSubjectFromDatabase()
    {
        SubjectRepository[] SelectedDB = _DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = _SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                ApplicationState.displayConfirmation("Subject Deletion", "You are going to erase this subject, are you sure ?", () => { DatabaseService.RemoveSubjectFromDatabase(SelectedDB[0], SelectedSubjects[0]); }, () => { });
            }
        }
    }

    private void MoveSubjectsToDatabase(string database)
    {
        SubjectRepository[] SelectedDB = _DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = _SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                SubjectRepository destinationDb = _DatabaseList.Objects.First(x => x.FilePath.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv2", "") == database);
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
        SubjectRepository[] SelectedDB = _DatabaseList.ObjectsSelected;
        if (SelectedDB.Length > 0)
        {
            Subject[] SelectedSubjects = _SubjectList.ObjectsSelected;
            if (SelectedSubjects.Length > 0)
            {
                SubjectRepository destinationDb = _DatabaseList.Objects.First(x => x.FilePath.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv2", "") == database);
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
    #endregion

    private void UpdateDatabaseCollection(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                BtvLog.Log("Adding a Database element : ");
                SubjectRepository itemToAdd = (SubjectRepository)e.NewItems[0]; //list of new items, only one at a time normally
                ((INotifyCollectionChanged)itemToAdd.Subjects).CollectionChanged += UpdateSubjectCollection;
                _DatabaseList.AddElement(itemToAdd);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                BtvLog.Log("Moving a Database element : ");
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                BtvLog.Log("Removing a Database element : ");
                SubjectRepository itemToRemove = (SubjectRepository)e.OldItems[0];
                ((INotifyCollectionChanged)itemToRemove.Subjects).CollectionChanged -= UpdateSubjectCollection;
                _DatabaseList.RemoveElement(itemToRemove);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                BtvLog.Log("Replacing a Database element : ");
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                BtvLog.Log("Reseting a Database element : ");
                break;
        }
    }

    private void UpdateSubjectCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    BtvLog.Log("Adding a Subject element : ");
                    Subject itemToAdd = (Subject)e.NewItems[0]; //list of new items, only one at a time normally
                    _SubjectList.AddElement(itemToAdd);
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                {
                    BtvLog.Log("Moving a Subject element : ");
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                {
                    BtvLog.Log("Removing a Subject element : ");
                    Subject itemToRemove = (Subject)e.OldItems[0];
                    _SubjectList.RemoveElement(itemToRemove);
                    break;
                }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                {
                    //We only need to update the element in the graphical object in case of 
                    //a subject to subject switch, if it's a change of db we only need the 
                    //underlying collection to be updated
                    if (!m_dbSwitch)
                    {
                        BtvLog.Log("Replacing a Subject element : ");
                        Subject itemToRemove = (Subject)e.OldItems[0];
                        Subject itemToAdd = (Subject)e.NewItems[0];
                        _SubjectList.ReplaceElement(itemToRemove, itemToAdd);
                    }
                }
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                {
                    BtvLog.Log("Reseting a Subject element : ");
                    break;
                }
        }
    }

    public void OnSubjectSelectionChanged()
    {
        if (m_LastSelectedRepository != null && _SubjectWidget.Subject != null)
        {
            int repoIndex = DatabaseService.Databases.IndexOf(m_LastSelectedRepository);
            Subject updated = new Subject(_SubjectWidget.Subject);
            Subject outdated = new Subject(_SubjectWidget.MemorySubject);
            if (updated != outdated)
            {
                ApplicationState.displayConfirmation("Keep Modifications ?", "There seems to have been some modifications, do you want to save them ?",
                    () =>
                    {
                        DatabaseService.UpdateSubjectFromDatabase(repoIndex, outdated, updated);
                        DatabaseService.Databases[repoIndex].Save();
                    },
                    () => { });
            }
        }

        Subject[] SelectedElements = _SubjectList.ObjectsSelected;
        if (SelectedElements.Length > 0)
        {
            _SubjectWidget.SetSubject(SelectedElements[0]);
        }
        else
        {
            _SubjectWidget.SetDefault();
        }
    }

    private void LoadSelectedSubject()
    {
        Subject updated = new Subject(_SubjectWidget.Subject);
        Subject outdated = new Subject(_SubjectWidget.MemorySubject);
        string experimentlabel = updated.Experiments[_SubjectWidget.ExperimentIndex].Label;
        if (updated != outdated)
        {
            ApplicationState.displayConfirmation("Keep Modifications ?", "There seems to have been some modifications, do you want to save them ?",
            () =>
            {
                m_dbSwitch = true;
                int repoIndex = DatabaseService.Databases.IndexOf(m_LastSelectedRepository);
                DatabaseService.UpdateSubjectFromDatabase(repoIndex, outdated, updated);
                DatabaseService.Databases[repoIndex].Save();
                m_dbSwitch = false;
                LoadSubject(updated, experimentlabel);
            },
            () =>
            {
                LoadSubject(updated, experimentlabel);
            });
        }
        else
        {
            LoadSubject(updated, experimentlabel);
        }
    }

    private void LoadSubject(Subject updated, string experimentLabel)
    {
        if (updated.IsLoadable)
        {
            LoadSubjectMessage message = new LoadSubjectMessage
            {
                subject = new Subject(updated),
                label = experimentLabel
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
