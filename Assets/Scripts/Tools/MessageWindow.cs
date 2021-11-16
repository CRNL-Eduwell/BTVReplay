using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MessageWindow : MonoBehaviour
{
    [SerializeField] private Button _HeaderClose = null;
    [SerializeField] private Text _HeaderLabel = null;
    [SerializeField] private Image _ImageInformation = null;
    [SerializeField] private Text _LongMessage = null;
    [SerializeField] private Button _Validate = null;
    [SerializeField] private Button _Cancel = null;

    private Sprite[] m_picInfo = new Sprite[3];

    private void Awake()
    {
        _HeaderClose.onClick.AddListener(() => { showMe(false); });

        m_picInfo[0] = Resources.Load("Pictures/DisplayWin/OkFinish", typeof(Sprite)) as Sprite;
        m_picInfo[1] = Resources.Load("Pictures/DisplayWin/InfoFinish", typeof(Sprite)) as Sprite;
        m_picInfo[2] = Resources.Load("Pictures/DisplayWin/NokFinish", typeof(Sprite)) as Sprite;
    }

    private void OnDestroy()
    {
        _HeaderClose.onClick.RemoveAllListeners();

        m_picInfo = null;
    }

    public void display(string header, string infoPic, string detailedMessage)
    {
        _Validate.onClick.RemoveAllListeners();
        _Validate.onClick.AddListener(() =>
        {
            showMe(false);
        });

        showMe(true);
        _HeaderLabel.text = header;

        switch (infoPic)
        {
            case "OK":
                _ImageInformation.sprite = m_picInfo[0];
                break;
            case "INFO":
                _ImageInformation.sprite = m_picInfo[1];
                break;
            case "NOK":
                _ImageInformation.sprite = m_picInfo[2];
                break;
        }

        _LongMessage.text = detailedMessage;

        _Cancel.interactable = false;
        _Cancel.gameObject.SetActive(false);
    }

    public void displayConfirmation(string HeaderMessage, string DetailledMessage, UnityAction yesAction, UnityAction cancelAction)
    {
        showMe(true);
        _HeaderLabel.text = HeaderMessage;
        _LongMessage.text = DetailledMessage;
        _ImageInformation.sprite = m_picInfo[1];
        _Cancel.interactable = true;
        _Cancel.gameObject.SetActive(true);

        _Validate.onClick.RemoveAllListeners();
        _Validate.onClick.AddListener(yesAction);
        _Validate.onClick.AddListener(() =>
         {
             showMe(false);
         });
        _Cancel.onClick.RemoveAllListeners();
        _Cancel.onClick.AddListener(cancelAction);
        _Cancel.onClick.AddListener(() =>
        {
            showMe(false);
        });
    }

    void showMe(bool show)
    {
        transform.parent.gameObject.SetActive(show);
    }
}
