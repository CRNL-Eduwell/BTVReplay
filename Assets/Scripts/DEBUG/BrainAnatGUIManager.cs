using UnityEngine;
using UnityEngine.UI;

public class BrainAnatGUIManager : MonoBehaviour
{
    [SerializeField] Text _Label = null;
    [SerializeField] browseButton _LeftHemi = null;
    [SerializeField] browseButton _RightHemi = null;
    [SerializeField] browseButton _Transform = null;
    [SerializeField] browseButton _Pts = null;
    [SerializeField] browseButton _Atlas = null;
    [SerializeField] Dropdown _MeshConfiguration = null;
    [SerializeField] Dropdown _EegTechnology = null;

    private bool m_IsMni = false;

    private void Awake()
    {
        m_IsMni = _Label != null && _Label.text.Contains("MNI");

        _LeftHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri";
        _RightHemi.inputfield.text = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri";
        _MeshConfiguration.onValueChanged.AddListener(OnMeshConfigurationValueChanged);
    }

    private void OnDestroy()
    {
        _MeshConfiguration.onValueChanged.RemoveAllListeners();
    }

    private void OnMeshConfigurationValueChanged(int value)
    {
        if (value == 0)
        {
            _LeftHemi.gameObject.SetActive(true);
            _LeftHemi.inputfield.placeholder.GetComponent<Text>().text = "LHemi File";
            _LeftHemi.inputfield.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
            //==
            _RightHemi.gameObject.SetActive(true);
            _RightHemi.inputfield.placeholder.GetComponent<Text>().text = "RHemi File";
            _RightHemi.inputfield.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
        }
        else
        {
            _LeftHemi.gameObject.SetActive(true);
            _LeftHemi.inputfield.placeholder.GetComponent<Text>().text = "Single File";
            _LeftHemi.inputfield.text = "";
            //==
            _RightHemi.gameObject.SetActive(false);
            _RightHemi.inputfield.text = "";
        }
    }

    public BrainDataContainer GetDataContainer()
    {
        BrainDataContainer container = new BrainDataContainer();
        container.LeftHemisphere = _LeftHemi.inputfield.text;
        container.RightHemisphere = _RightHemi.inputfield.text;
        container.Transformation = _Transform != null ? _Transform.inputfield.text : "";
        container.Pts = _Pts.inputfield.text;
        container.Atlas = _Atlas != null ? _Atlas.inputfield.text : "";
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
        _LeftHemi.inputfield.text = container.LeftHemisphere;
        _RightHemi.inputfield.text = container.RightHemisphere;
        if(_Transform != null) _Transform.inputfield.text = container.Transformation;
        _Pts.inputfield.text = container.Pts;
        if (_Atlas != null) _Atlas.inputfield.text = container.Atlas;
    }
}
