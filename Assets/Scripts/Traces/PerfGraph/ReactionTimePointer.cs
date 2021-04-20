using UnityEngine;
using UnityEngine.UI;

public class ReactionTimePointer : MonoBehaviour
{
    [SerializeField]
    private GameObject m_RootImageObject = null;
    [SerializeField]
    private Text m_CodeLabel = null;
    [SerializeField]
    private Text m_ReactionTimeLabel = null;

    private void Awake()
    {
        Messenger.Default.Register<ReactionTimePointerMessage>(this, OnReactionTimePointerMessage, MessageContext.ReactionTimePointerMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.ReactionTimePointerMessage);
    }

    void OnReactionTimePointerMessage(ReactionTimePointerMessage message)
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
                Debug.LogError("ReactionTimePointer.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void UpdatePointerInformation(ReactionTimePointerMessage message)
    {
        gameObject.transform.position = message.PointerPosition;
        m_RootImageObject.SetActive(message.ShowPointer);
        SetCodeLabel(message.Code);
        SetReactionTimeLabel(message.ReactionTimeMs);
    }

    private void SetCodeLabel(string label)
    {
        m_CodeLabel.text = "Code : " + label;
    }

    private void SetReactionTimeLabel(string label)
    {
        m_ReactionTimeLabel.text = "Reaction Time (ms) : " + label;
    }
}
