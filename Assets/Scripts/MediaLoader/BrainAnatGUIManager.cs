using UnityEngine;
using UnityEngine.UI;

public class BrainAnatGUIManager : MonoBehaviour
{
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

        _LeftHemi._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
        _RightHemi._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
        _Transform._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
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
            _LeftHemi._InputField.placeholder.GetComponent<Text>().text = "LHemi File";
            _LeftHemi._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri" : "";
            //==
            _RightHemi.gameObject.SetActive(true);
            _RightHemi._InputField.placeholder.GetComponent<Text>().text = "RHemi File";
            _RightHemi._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri" : "";
            //==
            _Transform.gameObject.SetActive(true);
            _Transform._InputField.placeholder.GetComponent<Text>().text = "Transformation File";
            _Transform._InputField.text = m_IsMni ? Application.dataPath + "/Config/Data/MNI/transfo_mni.trm" : "";
        }
        else
        {
            _LeftHemi.gameObject.SetActive(true);
            _LeftHemi._InputField.placeholder.GetComponent<Text>().text = "Single File";
            _LeftHemi._InputField.text = "";
            //==
            _RightHemi.gameObject.SetActive(false);
            _RightHemi._InputField.text = "";
            //==
            _Transform._InputField.text = "";
        }
    }

    public BrainDataContainer GetDataContainer()
    {
        BrainDataContainer container = new BrainDataContainer();
        container.LeftHemisphere = _LeftHemi._InputField.text;
        container.RightHemisphere = _RightHemi._InputField.text;
        container.Transformation = _Transform._InputField.text;
        container.Pts = _Pts._InputField.text;
        container.Atlas = _Atlas != null ? _Atlas._InputField.text : "";
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
        _LeftHemi._InputField.text = container.LeftHemisphere;
        _RightHemi._InputField.text = container.RightHemisphere;
        _Transform._InputField.text = container.Transformation;
        _Pts._InputField.text = container.Pts;
        if (_Atlas != null) _Atlas._InputField.text = container.Atlas;
    }
}
