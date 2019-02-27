using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MessageWindow : MonoBehaviour
{
    [SerializeField] Text headerMessage = null;
    [SerializeField] Image displayPic = null;
    [SerializeField] Text longMessage = null;
    [SerializeField] Button validate = null;
    [SerializeField] Button cancel = null;

    Sprite[] m_picInfo = new Sprite[3];

    private void Awake()
    {
        m_picInfo[0] = Resources.Load("Pictures/DisplayWin/OkFinish", typeof(Sprite)) as Sprite;
        m_picInfo[1] = Resources.Load("Pictures/DisplayWin/InfoFinish", typeof(Sprite)) as Sprite;
        m_picInfo[2] = Resources.Load("Pictures/DisplayWin/NokFinish", typeof(Sprite)) as Sprite;
    }

    private void OnDestroy()
    {
        m_picInfo = null;
    }

    public void display(string header, string infoPic, string detailedMessage)
    {
        validate.onClick.RemoveAllListeners();
        validate.onClick.AddListener(() =>
        {
            showMe(false);
        });

        showMe(true);
        headerMessage.text = header;

        switch (infoPic)
        {
            case "OK":
                displayPic.sprite = m_picInfo[0];
                break;
            case "INFO":
                displayPic.sprite = m_picInfo[1];
                break;
            case "NOK":
                displayPic.sprite = m_picInfo[2];
                break;
        }

        longMessage.text = detailedMessage;

        cancel.interactable = false;
        cancel.gameObject.SetActive(false);
    }

    public void displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        showMe(true);
        headerMessage.text = HeaderMessage;
        longMessage.text = DetailledMessage;
        displayPic.sprite = m_picInfo[1];
        cancel.interactable = true;
        cancel.gameObject.SetActive(true);

        validate.onClick.RemoveAllListeners();
        validate.onClick.AddListener(yesAction);
        validate.onClick.AddListener(() =>
         {
             showMe(false);
         });
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(cancelAction);
        cancel.onClick.AddListener(() =>
        {
            showMe(false);
        });
    }

    void showMe(bool show)
    {
        transform.parent.gameObject.SetActive(show);
    }
}
