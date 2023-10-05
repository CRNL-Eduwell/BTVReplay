using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AnatomicalDataWidget : MonoBehaviour
{
    public bool IsInteractable
    {
        get
        {
            return _LeftHemi.IsInteractable;
        }
        set
        {
            _LeftHemi.IsInteractable = value;
            _RightHemi.IsInteractable = value;
            _Transform.IsInteractable = value;
            _Pts.IsInteractable = value;
            _Atlas.IsInteractable = value;
            _MeshConfiguration.interactable = value;
            _EegTechnology.interactable = value;
        }
    }

    [SerializeField] private Transform _HeaderTabs = null;
    [SerializeField] private BrowseWidget _LeftHemi = null;
    [SerializeField] private BrowseWidget _RightHemi = null;
    [SerializeField] private BrowseWidget _Transform = null;
    [SerializeField] private BrowseWidget _Pts = null;
    [SerializeField] private BrowseWidget _Atlas = null;
    [SerializeField] private Dropdown _MeshConfiguration = null;
    [SerializeField] private Dropdown _EegTechnology = null;

    private UnityEvent<string> OnTabClicked { get; } = new GenericEvent<string>();

    private GameObject m_ButtonPrefab = null;
    private List<ExtendedButton> m_Buttons = new List<ExtendedButton>();

    private Color m_normalColor = new Color(0.149019f, 0.149019f, 0.149019f, 1); //38
    private Color m_selectedColor = new Color(0.231372f, 0.478431f, 0.760784f, 1); //59 / 122 / 194

    private Subject m_Subject = null;
    private bool m_IsMni = false;
    private string m_Label = "";

    private void Awake()
    {
        m_ButtonPrefab = Resources.Load("Prefabs/ReferentialLabel", typeof(GameObject)) as GameObject;

        _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
        _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
        _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
        //===
        _LeftHemi.TextUpdated.AddListener((string str) => { GetDataFromUI(str, 0); });
        _RightHemi.TextUpdated.AddListener((string str) => { GetDataFromUI(str, 1); });
        _Transform.TextUpdated.AddListener((string str) => { GetDataFromUI(str, 2); });
        _Pts.TextUpdated.AddListener((string str) => { GetDataFromUI(str, 3); });
        _Atlas.TextUpdated.AddListener((string str) => { GetDataFromUI(str, 4); });
        _MeshConfiguration.onValueChanged.AddListener(OnMeshConfigurationValueChanged);
        _EegTechnology.onValueChanged.AddListener(OnEegTechnologyValueChanged);

        OnTabClicked.AddListener(OnTabClickedUpdate);
        //===
        AddTab("MNI");
        AddTab("PAT");
        m_Buttons[0].SetColor(m_selectedColor);
        m_Buttons[1].SetColor(m_normalColor);

        _Atlas.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnTabClicked.RemoveAllListeners();

        _LeftHemi.TextUpdated.RemoveAllListeners();
        _RightHemi.TextUpdated.RemoveAllListeners();
        _Transform.TextUpdated.RemoveAllListeners();
        _Pts.TextUpdated.RemoveAllListeners();
        _Atlas.TextUpdated.RemoveAllListeners();
        _MeshConfiguration.onValueChanged.RemoveAllListeners();
        _EegTechnology.onValueChanged.RemoveAllListeners();

        for (int i = m_Buttons.Count - 1; i >= 0; i--)
        {
            RemoveTab(i);
        }
    }

    public void SetDefault()
    {
        m_Subject = null;
        m_Label = "";
        m_IsMni = false;

        SetDataInUI(new BrainDataContainer());
        IsInteractable = false;
    }

    public void SetSubject(Subject subject)
    {
        m_Subject = subject;

        bool mniFound = m_Subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        if (mniFound)
        {
            m_Label = "MNI";
            m_IsMni = true;
            SetDataInUI(mniContainer);
        }
        IsInteractable = true;
    }

    private void AddTab(string name)
    {
        ExtendedButton addMe = Instantiate(m_ButtonPrefab, _HeaderTabs.transform).GetComponent<ExtendedButton>();
        addMe.SetColor(m_normalColor);
        addMe.SetTabText(name);

        m_Buttons.Add(addMe);
        ConnectButton(m_Buttons.Count - 1);
    }

    private void RemoveTab(int index)
    {
        ExtendedButton deleteMe = m_Buttons[index];
        deleteMe.OnSingleClick.RemoveAllListeners();
        deleteMe.OnDoubleClick.RemoveAllListeners();
        m_Buttons.Remove(deleteMe);

        Destroy(deleteMe.gameObject);
    }

    private void ConnectButton(int ID)
    {
        m_Buttons[ID].OnSingleClick.AddListener(() => { SwitchTo(ID); });
    }

    private void SwitchTo(int ID)
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            m_Buttons[i].SetColor(m_normalColor);
        }
        m_Buttons[ID].SetColor(m_selectedColor);

        OnTabClicked.Invoke(m_Buttons[ID].Text);
    }

    private void OnMeshConfigurationValueChanged(int value)
    {
        if (value == 0)
        {
            _LeftHemi.gameObject.SetActive(true);
            _LeftHemi.PlaceholderText = "LHemi File";
            _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
            //==
            _RightHemi.gameObject.SetActive(true);
            _RightHemi.PlaceholderText = "RHemi File";
            _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
            //==
            _Transform.gameObject.SetActive(true);
            _Transform.PlaceholderText = "Transformation File";
            _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
        }
        else
        {
            _LeftHemi.gameObject.SetActive(true);
            _LeftHemi.PlaceholderText = "Single File";
            _LeftHemi.Text = "";
            //==
            _RightHemi.gameObject.SetActive(false);
            _RightHemi.Text = "";
            //==
            _Transform.Text = "";
        }
    }

    private void OnEegTechnologyValueChanged(int value)
    {
        switch (value)
        {
            case 0:
                {
                    _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
                    _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
                    _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
                    _Pts.Text = "";
                }
                break;
            case 1:
                {
                    _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
                    _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
                    _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
                    _Pts.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_EEG.pts" : "";
                }
                break;
        }
    }

    private void SetDataInUI(BrainDataContainer container)
    {
        _MeshConfiguration.SetValueWithoutNotify((int)container.MeshConfiguration);
        _EegTechnology.SetValueWithoutNotify((int)container.EegTechnology);

        //will loop back to GetDataFromUI , see to maybe use a lock when doing that
        _LeftHemi.TextWithoutPopUp = container.LeftHemisphere;
        _RightHemi.TextWithoutPopUp = container.RightHemisphere;
        _Transform.TextWithoutPopUp = container.Transformation;
        _Pts.TextWithoutPopUp = container.Pts;
        if (!m_IsMni) _Atlas.TextWithoutPopUp = container.Atlas;
    }

    private void GetDataFromUI(string text, int data)
    {
        if (m_Subject == null) return;

        if (m_Subject.AnatomicalSpaces.ContainsKey(m_Label))
        {
            switch (data)
            {
                case 0:
                    {
                        m_Subject.AnatomicalSpaces[m_Label].LeftHemisphere = text;
                        break;
                    }
                case 1:
                    {
                        m_Subject.AnatomicalSpaces[m_Label].RightHemisphere = text;
                        break;
                    }
                case 2:
                    {
                        m_Subject.AnatomicalSpaces[m_Label].Transformation = text;
                        break;
                    }
                case 3:
                    {
                        m_Subject.AnatomicalSpaces[m_Label].Pts = text;
                        break;
                    }
                case 4:
                    {
                        m_Subject.AnatomicalSpaces[m_Label].Atlas = !m_IsMni ? text : "";
                        break;
                    }
            }
        }
    }

    private void OnTabClickedUpdate(string label)
    {
        //find new container and put data in UI
        m_Label = label;
        m_IsMni = m_Label == "MNI";
        if (m_Subject != null)
        {
            bool isFound = m_Subject.AnatomicalSpaces.TryGetValue(m_Label, out BrainDataContainer container);
            if (isFound)
            {
                _Atlas.gameObject.SetActive(!m_IsMni);
                SetDataInUI(container);
            }
        }
    }
}
