using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AnatomicalDataWidget : MonoBehaviour
{
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

    private Color m_normalColor = new Color(0.203921f, 0.203921f, 0.203921f, 1); //52
    private Color m_selectedColor = new Color(0.125490f, 0.125490f, 0.125490f, 1); //32

    private Subject m_Subject = null;
    private bool m_IsMni = false;
    private string m_Label = "";

    private void Awake()
    {
        m_ButtonPrefab = Resources.Load("Prefabs/ReferentialLabel", typeof(GameObject)) as GameObject;

        _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
        _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
        _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";

        _MeshConfiguration.onValueChanged.AddListener(OnMeshConfigurationValueChanged);
        _EegTechnology.onValueChanged.AddListener(OnEegTechnologyValueChanged);
        OnTabClicked.AddListener(OnTabClickedUpdate);

        AddTab("MNI");
        AddTab("PAT");
        m_Buttons[0].SetColor(m_selectedColor);
        m_Buttons[1].SetColor(m_normalColor);

        _Atlas.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnTabClicked.RemoveAllListeners();

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

        SetDataInUI(new BrainDataContainer());
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

        //bool patFound = m_Subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);
        //if (patFound)
        //{
        //    m_Label = "PAT";
        //    m_IsMni = false;
        //    SetDataInUI(patContainer);
        //}
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
        _MeshConfiguration.value = (int)container.MeshConfiguration;
        _MeshConfiguration.onValueChanged.Invoke((int)container.MeshConfiguration);
        //check that on value changed is not called two times

        _EegTechnology.value = (int)container.EegTechnology;
        _LeftHemi.TextWithoutPopUp = container.LeftHemisphere;
        _RightHemi.TextWithoutPopUp = container.RightHemisphere;
        _Transform.TextWithoutPopUp = container.Transformation;
        _Pts.TextWithoutPopUp = container.Pts;
        if (!m_IsMni) _Atlas.TextWithoutPopUp = container.Atlas;
    }

    private void OnTabClickedUpdate(string label)
    {
        if (m_Subject != null)
        {
            //Get ui data and put it in old container
            bool isFound = m_Subject.AnatomicalSpaces.TryGetValue(m_Label, out BrainDataContainer container);
            if (isFound)
            {
                container.LeftHemisphere = _LeftHemi.Text;
                container.RightHemisphere = _RightHemi.Text;
                container.Transformation = _Transform.Text;
                container.Pts = _Pts.Text;
                container.Atlas = !m_IsMni ? _Atlas.Text : "";
                container.SetMeshConfigurationFromString(_MeshConfiguration.captionText.text);
                container.SetEegTechnologyFromString(_EegTechnology.captionText.text);
            }
        }

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
