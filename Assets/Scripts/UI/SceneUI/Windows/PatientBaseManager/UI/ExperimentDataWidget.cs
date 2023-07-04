using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BTV.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExperimentDataWidget : MonoBehaviour
{
    public int ExperimentIndex { get { return m_ExperimentID; } }

    [SerializeField] private Transform _HeaderTabs = null;
    [SerializeField] private Button _AddTab = null;
    [SerializeField] private Button _RemoveTab = null;
    [SerializeField] private EegInfoGUIManager[] _EegFiles = new EegInfoGUIManager[6] { null, null, null, null, null, null, };
    [SerializeField] private BrowseWidget _Video = null;

    private GameObject m_ButtonPrefab = null;
    private List<ExtendedButton> m_Buttons = new List<ExtendedButton>();
    private Color m_normalColor = new Color(0.149019f, 0.149019f, 0.149019f, 1); //38
    private Color m_selectedColor = new Color(0.231372f, 0.478431f, 0.760784f, 1); //59 / 122 / 194

    private Subject m_Subject = null;
    private int m_ExperimentID = 0;
    private bool m_LockFeedback = false;

    private void Awake()
    {
        m_ButtonPrefab = Resources.Load("Prefabs/ExamLabel", typeof(GameObject)) as GameObject;

        _AddTab.onClick.AddListener(AddTab);
        _RemoveTab.onClick.AddListener(RemoveSelectedTab);
        foreach (var eeg in _EegFiles)
        {
            eeg.TextUpdated.AddListener(GetDataFromUI);
            eeg.onEndEditKey.AddListener((oldstr, str) => { IsKeyOk(oldstr, str, eeg); });
        }

        _Video.TextUpdated.AddListener((str) =>
        {
            if (m_Subject == null) return;
            if (m_ExperimentID >= m_Subject.Experiments.Count) return;
            if (m_LockFeedback) return;
          
            m_Subject.Experiments[m_ExperimentID].Video = str;
        });
        UpdateInteractability();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            m_Buttons[i].OnSingleClick.RemoveAllListeners();
            m_Buttons[i].OnDoubleClick.RemoveAllListeners();
        }

        _Video.TextUpdated.RemoveAllListeners();

        foreach (var eeg in _EegFiles)
        {
            eeg.TextUpdated.RemoveAllListeners();
            eeg.onEndEditKey.RemoveAllListeners();
        }

        _AddTab.onClick.RemoveAllListeners();
        _RemoveTab.onClick.RemoveAllListeners();
    }

    public void SetDefault()
    {
        m_Subject = null;
        m_ExperimentID = 0;

        m_LockFeedback = true;

        RemoveAllTabs();
        for (int i = 0; i < _EegFiles.Length; i++)
        {
            _EegFiles[i].SetEegFileInfoToGUI("", "");
        }
        _Video.Text = "";

        UpdateInteractability();
        m_LockFeedback = false;
    }

    public void SetSubject(Subject subject)
    {
        m_Subject = subject;
        m_ExperimentID = 0;

        m_LockFeedback = true;

        //Add all corresponding tabs
        RemoveAllTabs();
        for (int i = 0; i < m_Subject.Experiments.Count; i++)
        {
            AddTabInGui(m_Subject.Experiments[i].Label);
        }

        if (m_ExperimentID < m_Subject.Experiments.Count)
        {
            m_Buttons[m_ExperimentID].SetColor(m_selectedColor);

            //input tab data
            for (int i = 0; i < _EegFiles.Length; i++)
            {
                KeyValuePair<string, IEegFileInfo> kvp = m_Subject.Experiments[m_ExperimentID].Files.ElementAtOrDefault(i);
                bool isDefaultValue = kvp.Equals(default(KeyValuePair<string, IEegFileInfo>));
                string key = isDefaultValue ? "" : kvp.Key;
                string filePath = isDefaultValue ? "" : kvp.Value.Files[0];

                _EegFiles[i].SetEegFileInfoToGUI(key, filePath);
            }
            _Video.Text = m_Subject.Experiments[m_ExperimentID].Video;
        }
        UpdateInteractability();
        m_LockFeedback = false;
    }

    private void AddTab()
    {
        string name = "TASK";
        int count = 0;
        while (true)
        {
            string temp = string.Format("{0}({1})", name, ++count);
            if (m_Subject.Experiments.FindAll(x => x.Label == temp).Count == 0)
            {
                name = temp;
                break;
            }
        }
        AddTabInGui(name);
        m_Subject.Experiments.Insert(m_Buttons.Count - 1, new Experiment(name, new Dictionary<string, IEegFileInfo>(), ""));
        SwitchTo(m_Buttons[m_Buttons.Count - 1]);
    }

    private void AddTabInGui(string name)
    {
        ExtendedButton addMe = Instantiate(m_ButtonPrefab, _HeaderTabs.transform).GetComponent<ExtendedButton>();
        addMe.SetColor(m_normalColor);
        addMe.SetTabText(name);

        addMe.OnSingleClick.AddListener(() => { SwitchTo(addMe); });
        addMe.OnDoubleClick.AddListener(() =>
        {
            InputFieldWindow window = ApplicationState.SpawFrequencyChoiceWindow();
            window.Initialize("Label", "Choose a new label for you tab",
                () =>
                {
                    RenameTab(addMe, window.StringValue);
                    window.Close();
                }, () =>
                {
                    window.Close();
                });
            window.StringValue = addMe.Text;
        });

        m_Buttons.Add(addMe);
    }

    private void RemoveSelectedTab()
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            if (m_Buttons[i].Color == m_selectedColor)
            {
                RemoveTab(i, true);
            }
        }
    }

    private void RemoveAllTabs()
    {
        for (int i = m_Buttons.Count - 1; i >= 0; i--)
        {
            RemoveTab(i);
        }
    }

    private void RemoveTab(int tabIndex, bool sendEvent = false)
    {
        if (m_Subject != null)
        {
            if (sendEvent)
            {
                m_Subject.Experiments.RemoveAt(tabIndex);
            }
        }

        ExtendedButton deleteMe = m_Buttons[tabIndex];
        deleteMe.OnSingleClick.RemoveAllListeners();
        deleteMe.OnDoubleClick.RemoveAllListeners();
        m_Buttons.Remove(deleteMe);

        Destroy(deleteMe.gameObject);

        if (m_Buttons.Count >= 1)
        {
            m_Buttons[0].SetColor(m_selectedColor);
            if (sendEvent)
                ClickOntTab(0);
        }
        else
        {
            m_LockFeedback = true;

            //TODO need to set not interactable
            for (int i = 0; i < _EegFiles.Length; i++)
            {
                _EegFiles[i].SetEegFileInfoToGUI("", "");
            }
            _Video.Text = "";

            m_LockFeedback = false;
        }

        UpdateInteractability();
    }

    private void SwitchTo(ExtendedButton button)
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            m_Buttons[i].SetColor(m_normalColor);
        }
        button.SetColor(m_selectedColor);

        int index = m_Buttons.FindIndex(x => x == button);
        if (index >= 0)
        {
            ClickOntTab(index);
        }
        else
        {
            UnityEngine.Debug.LogError("Error fctn SwitchTo, index is : " + index);
        }
    }

    private void GetDataFromUI(string key, string text)
    {
        if (m_LockFeedback) return;
        if (string.IsNullOrEmpty(key)) return;
        if (string.IsNullOrEmpty(text))
        {
            m_Subject.Experiments[m_ExperimentID].Files.Remove(key);
            return;
        }

        if (m_Subject.Experiments[m_ExperimentID].Files.ContainsKey(key))
        {
            UnityEngine.Debug.Log("Update coming from " + key + " new value is " + text);
            FileInfo fileInfo = new FileInfo(text);
            if (fileInfo.Extension == ".TRC")
            {
                m_Subject.Experiments[m_ExperimentID].Files[key] = new MicromedFileInfo(text);
            }
            else if (fileInfo.Extension == ".eeg")
            {
                m_Subject.Experiments[m_ExperimentID].Files[key] = new ElanFileInfo(text);
            }
            else if (fileInfo.Extension == ".vhdr")
            {
                m_Subject.Experiments[m_ExperimentID].Files[key] = new BrainvisionFileInfo(text);
            }
            else if (fileInfo.Extension == ".edf")
            {
                m_Subject.Experiments[m_ExperimentID].Files[key] = new EdfFileInfo(text);
            }
            else
            {
                return;
            }
        }
        else
        {
            UnityEngine.Debug.Log("Adding key " + key + " new value is " + text);
            FileInfo fileInfo = new FileInfo(text);
            if (fileInfo.Extension == ".TRC")
            {
                m_Subject.Experiments[m_ExperimentID].Files.Add(key, new MicromedFileInfo(text));
            }
            else if (fileInfo.Extension == ".eeg")
            {
                m_Subject.Experiments[m_ExperimentID].Files.Add(key, new ElanFileInfo(text));
            }
            else if (fileInfo.Extension == ".vhdr")
            {
                m_Subject.Experiments[m_ExperimentID].Files.Add(key, new BrainvisionFileInfo(text));
            }
            else if (fileInfo.Extension == ".edf")
            {
                m_Subject.Experiments[m_ExperimentID].Files.Add(key, new EdfFileInfo(text));
            }
            else
            {
                return;
            }
        }
    }

    private void IsKeyOk(string oldstr, string str, EegInfoGUIManager eeg)
    { 
        List<string> keys = new List<string>();
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            if (_EegFiles[i] == eeg) continue; //if this is the one modified , we don't want to take it into account
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            keys.Add(kvp.Key);
        }

        if (keys.Contains(str))
        {
            ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
            eeg.RevertKeyField();
            return;
        }
        else
        {
            if (!string.IsNullOrEmpty(oldstr) && string.IsNullOrEmpty(str))
            {
                if (m_Subject.Experiments[m_ExperimentID].Files.ContainsKey(oldstr))
                {
                    m_Subject.Experiments[m_ExperimentID].Files.Remove(oldstr);
                }
            }
            else
            {
                KeyValuePair<string, IEegFileInfo> kvp = eeg.GetEegFileInfoFromGUI();
                GetDataFromUI(kvp.Key, kvp.Value.Files[0]);
            }
        }

        UpdateInteractability();
    }

    private void ClickOntTab(int Id)
    {
        //find container and put data in UI
        m_ExperimentID = Id;

        if (m_Subject == null) return;

        m_LockFeedback = true;
        //input tab data
        for (int i = 0; i < _EegFiles.Length; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = m_Subject.Experiments[m_ExperimentID].Files.ElementAtOrDefault(i);
            bool isDefaultValue = kvp.Equals(default(KeyValuePair<string, IEegFileInfo>));
            string key = isDefaultValue ? "" : kvp.Key;
            string filePath = isDefaultValue ? "" : kvp.Value.Files[0];

            _EegFiles[i].SetEegFileInfoToGUI(key, filePath);
        }
        _Video.Text = m_Subject.Experiments[m_ExperimentID].Video;
        UpdateInteractability();

        m_LockFeedback = false;
    }

    private void RenameTab(ExtendedButton button, string label)
    {
        int index = m_Buttons.FindIndex(x => x == button);
        if (index >= 0)
        {
            m_Subject.Experiments[index].Label = label;
            m_Buttons[index].SetTabText(label);
        }
        else
        {
            UnityEngine.Debug.LogError("TabRenammed error, index is " + index);
        }
    }

    private void UpdateInteractability()
    {
        bool isInteractable = m_Buttons.Count > 0;
        foreach (var eeg in _EegFiles)
            eeg.IsInteractable = isInteractable;
        _Video.IsInteractable = isInteractable;
    }
}
