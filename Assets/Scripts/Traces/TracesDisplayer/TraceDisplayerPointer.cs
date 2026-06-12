using UnityEngine;
using UnityEngine.UI;

public class TraceDisplayerPointer : MonoBehaviour
{
    [SerializeField]
    private GameObject m_RootImageObject = null;
    [SerializeField]
    private Text m_CodeLabel = null;
    [SerializeField]
    private Text m_DescriptionLabel = null;
    [SerializeField]
    private Text m_DurationTimeLabel = null;

    private float m_TimeSinceAppear = 0.0f;

    private void Awake()
    {
        Messenger.Default.Register<TraceDisplayerPointerMessage>(this, OnTraceDisplayerPointerMessage, MessageContext.TraceDisplayerPointerMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.TraceDisplayerPointerMessage);
    }

    private void Update()
    {
        if (m_RootImageObject.activeSelf)
        {
            m_TimeSinceAppear += Time.deltaTime;
            if (m_TimeSinceAppear > 0.2f)
            {
                m_RootImageObject.SetActive(false);
            }
        }
    }

    void OnTraceDisplayerPointerMessage(TraceDisplayerPointerMessage message)
    {
        switch (message.TaskToExecute)
        {
            case TraceDisplayerPointerMessage.Task.ShowAndUpdate:
                m_TimeSinceAppear = 0.0f;
                UpdatePointerInformation(message);
                break;
            case TraceDisplayerPointerMessage.Task.Hide:
                m_RootImageObject.SetActive(message.ShowPointer);
                break;
            default:
                Debug.LogError("TraceDisplayerPointer.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void UpdatePointerInformation(TraceDisplayerPointerMessage message)
    {
        gameObject.transform.position = message.PointerPosition;
        m_RootImageObject.SetActive(message.ShowPointer);
        SetCodeLabel(message.Code);
        SetDescriptionLabel(message.Description);
        SetReactionTimeLabel(message.DurationTimeMs);
    }

    private void SetCodeLabel(string label)
    {
        m_CodeLabel.text = "Code : " + label;
    }

    private void SetDescriptionLabel(string label)
    {
        m_DescriptionLabel.text = "Description : " + label;
    }

    private void SetReactionTimeLabel(string label)
    {
        m_DurationTimeLabel.text = "Duration Time (ms) : " + label;
    }
}
