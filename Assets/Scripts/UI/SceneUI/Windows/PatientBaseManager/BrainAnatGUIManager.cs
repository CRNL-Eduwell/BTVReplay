using UnityEngine;
using UnityEngine.UI;

public class BrainAnatGUIManager : MonoBehaviour
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
            if(_Atlas != null) _Atlas.IsInteractable = value;
            _MeshConfiguration.interactable = value;
            _EegTechnology.interactable = value;
        }
    }

    [SerializeField] Text _Label = null;
    [SerializeField] BrowseWidget _LeftHemi = null;
    [SerializeField] BrowseWidget _RightHemi = null;
    [SerializeField] BrowseWidget _Transform = null;
    [SerializeField] BrowseWidget _Pts = null;
    [SerializeField] BrowseWidget _Atlas = null;
    [SerializeField] Dropdown _MeshConfiguration = null;
    [SerializeField] Dropdown _EegTechnology = null;

    private bool m_IsMni = false;

    private void Awake()
    {
        m_IsMni = _Label != null && _Label.text.Contains("MNI");

        _LeftHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
        _RightHemi.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
        _Transform.Text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
        _MeshConfiguration.onValueChanged.AddListener(OnMeshConfigurationValueChanged);
        _EegTechnology.onValueChanged.AddListener(OnEegTechnologyValueChanged);
    }

    private void OnDestroy()
    {
        _MeshConfiguration.onValueChanged.RemoveAllListeners();
        _EegTechnology.onValueChanged.RemoveAllListeners();
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

    public BrainDataContainer GetDataContainer()
    {
        BrainDataContainer container = new BrainDataContainer();
        container.LeftHemisphere = _LeftHemi.Text;
        container.RightHemisphere = _RightHemi.Text;
        container.Transformation = _Transform.Text;
        container.Pts = _Pts.Text;
        container.Atlas = _Atlas != null ? _Atlas.Text : "";
        container.SetMeshConfigurationFromString(_MeshConfiguration.captionText.text);
        container.SetEegTechnologyFromString(_EegTechnology.captionText.text);
        return container;
    }

    public void SetDataConainerInUI(BrainDataContainer container)
    {
        _MeshConfiguration.value = (int)container.MeshConfiguration;
        _MeshConfiguration.onValueChanged.Invoke((int)container.MeshConfiguration);
        //check that on value changed is not called two times

        _EegTechnology.value = (int)container.EegTechnology;
        _LeftHemi.TextWithoutPopUp = container.LeftHemisphere;
        _RightHemi.TextWithoutPopUp = container.RightHemisphere;
        _Transform.TextWithoutPopUp = container.Transformation;
        _Pts.TextWithoutPopUp = container.Pts;
        if (_Atlas != null) _Atlas.TextWithoutPopUp = container.Atlas;
    }
}
