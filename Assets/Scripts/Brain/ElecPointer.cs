using UnityEngine;
using UnityEngine.UI;

public class ElecPointer : MonoBehaviour
{
    [SerializeField]
    private GameObject m_RootImageObject = null;
    [SerializeField]
    private Text m_ElectrodeLabel = null;
    [SerializeField]
    private Text m_MarsAtlasLabel = null;
    [SerializeField]
    private Text m_BroadmanLabel = null;
    [SerializeField]
    private Text m_CoordinatesLabel = null;

    private void Awake()
    {
        Messenger.Default.Register<BrainWardenToElectrodePointerMessage>(this, OnTraceParametersMessage, MessageContext.BrainWardenToElectrodePointerMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.BrainWardenToElectrodePointerMessage);
    }

    void OnTraceParametersMessage(BrainWardenToElectrodePointerMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                UpdatePointerInformation(message);
                break;
            case 1:
                m_RootImageObject.SetActive(message.ShowPointer);
                break;
            default:
                Debug.LogError("ElecPointer.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void UpdatePointerInformation(BrainWardenToElectrodePointerMessage message)
    {
        gameObject.transform.position = message.PointerPosition;
        m_RootImageObject.SetActive(message.ShowPointer);
        SetElectrodeLabel(message.ElectrodeLabel.ToUpper());
        Site hitPlot = GameObject.Find(message.ElectrodeLabel).GetComponent<Site>();
        if (hitPlot != null)
        {
            SetMarsAtlasLabel(hitPlot.MarsAtlasName);
            SetBroadmanLabel(hitPlot.BroadmanName);
            SetCorrdinatesLabel(hitPlot.Coordinates);
        }
    }

    private void SetElectrodeLabel(string label)
    {
        m_ElectrodeLabel.text = "Electrode name: " + label;
    }

    private void SetMarsAtlasLabel(string label)
    {
        m_MarsAtlasLabel.text = "Mars Atlas Parcel : " + label;
    }

    private void SetBroadmanLabel(string label)
    {
        m_BroadmanLabel.text = "Broadman Area : " + label;
    }

    private void SetCorrdinatesLabel(Vector3 position)
    {
        m_CoordinatesLabel.text = "Coordinates : " + position.x + " " + position.y + " " + position.z;
    }
}
