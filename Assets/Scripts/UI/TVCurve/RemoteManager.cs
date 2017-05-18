using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void gainChangedEventHandler(int oldVal, int newVal);
public delegate void idElecChangedEventHandler();
public delegate void idFileChangedEventHandler(int newIdHandle);
public delegate void toggleSonificationEventHandler();
public delegate void colorElecChangedEventHandler(string myElectrodeName, Color myColor);
public delegate void colorElecChangeForceEventHandler(string myElectrodeName, Color myColor);

public class RemoteManager : MonoBehaviour
{
    public int currentIdElec = 0;
    public int Gain = 1;
    public int PreviousGain = 1;
    public GameObject myColorPicker = null;

    public event gainChangedEventHandler gainHasChanged;
    public event idElecChangedEventHandler idElecHasChanged;
    public event idFileChangedEventHandler idFileHasChanged;
    public event toggleSonificationEventHandler sonifHasBeenToggled;
    public event colorElecChangedEventHandler colorElecHasChanged;
    public event colorElecChangeForceEventHandler colorElecForcedChange;

    //===
    private GameObject ElecPlot = null;
    private Transform elecContent = null;

    private Button closeRemoteButton = null;
    private Button sonificationButton = null;
    private Button colorPickButton = null;
    private Button gainMoreButton = null;
    private Button gainLessButton = null;
    private Text gainText = null;

    private Button sm0 = null;
    private Button sm250 = null;
    private Button sm500 = null;
    private Button sm1000 = null;
    private Button sm2500 = null;
    private Button sm5000 = null;
    //===
    private BTVMedia_New btvMedia = null;
    private ColorPickerManager colorPickManager = null;
    private SphereColor sphereColor = null;

    void Awake()
    {
        ElecPlot = Resources.Load("Prefabs/OptCurve/Elec", typeof(GameObject)) as GameObject;

        #region getObjectFromScene
        elecContent = gameObject.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        //===
        closeRemoteButton = gameObject.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        sonificationButton = gameObject.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Button>();
        colorPickButton = gameObject.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<Button>();
        //===
        gainMoreButton = gameObject.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Button>();
        gainLessButton = gameObject.transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<Button>();
        gainText = gameObject.transform.GetChild(1).GetChild(3).GetChild(3).GetComponent<Text>();
        //===
        sm0 = gameObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetComponent<Button>();
        sm250 = gameObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(1).GetComponent<Button>();
        sm500 = gameObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(2).GetComponent<Button>();
        sm1000 = gameObject.transform.GetChild(1).GetChild(2).GetChild(1).GetChild(0).GetComponent<Button>();
        sm2500 = gameObject.transform.GetChild(1).GetChild(2).GetChild(1).GetChild(1).GetComponent<Button>();
        sm5000 = gameObject.transform.GetChild(1).GetChild(2).GetChild(1).GetChild(2).GetComponent<Button>();
        //===
        btvMedia = GameObject.Find("Canvas").transform.GetChild(8).GetComponent<BTVMedia_New>();
        colorPickManager = myColorPicker.transform.GetComponent<ColorPickerManager>();
        sphereColor = gameObject.AddComponent<SphereColor>();
        #endregion

        #region addListener
        closeRemoteButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        sonificationButton.onClick.AddListener(() =>
        {
            sonifHasBeenToggled();
        });

        colorPickButton.onClick.AddListener(() =>
        {
            myColorPicker.SetActive(!myColorPicker.activeSelf);
        });

        gainMoreButton.onClick.AddListener(() =>
        {
            PreviousGain = Gain;
            Gain += 1;
            gainHasChanged(PreviousGain, Gain);
            gainText.text = Gain.ToString();
        });

        gainLessButton.onClick.AddListener(() =>
        {
            if (Gain - 1 > 0)
            {
                PreviousGain = Gain;
                Gain -= 1;
                gainHasChanged(PreviousGain, Gain);
                gainText.text = Gain.ToString();
            }
        });

        sm0.onClick.AddListener(() =>
        {
            if (checkHandle(0))
            {
                changeButtonHighlightAlpha(sm0, 255);
                idFileHasChanged(0);
            }
            else
            {
                changeButtonHighlightAlpha(sm0, 0);
            }
        });

        sm250.onClick.AddListener(() =>
        {
            if (checkHandle(1))
            {
                changeButtonHighlightAlpha(sm250, 255);
                idFileHasChanged(1);
            }
            else
            {
                changeButtonHighlightAlpha(sm250, 0);
            }
        });

        sm500.onClick.AddListener(() =>
        {
            if (checkHandle(2))
            {
                changeButtonHighlightAlpha(sm500, 255);
                idFileHasChanged(2);
            }
            else
            {
                changeButtonHighlightAlpha(sm500, 0);
            }
        });

        sm1000.onClick.AddListener(() =>
        {
            if (checkHandle(3))
            {
                changeButtonHighlightAlpha(sm1000, 255);
                idFileHasChanged(3);
            }
            else
            {
                changeButtonHighlightAlpha(sm1000, 0);
            }
        });

        sm2500.onClick.AddListener(() =>
        {
            if (checkHandle(4))
            {
                changeButtonHighlightAlpha(sm2500, 255);
                idFileHasChanged(4);
            }
            else
            {
                changeButtonHighlightAlpha(sm2500, 0);
            }
        });

        sm5000.onClick.AddListener(() =>
        {
            if (checkHandle(5))
            {
                changeButtonHighlightAlpha(sm5000, 255);
                idFileHasChanged(5);
            }
            else
            {
                changeButtonHighlightAlpha(sm5000, 0);
            }
        });
        #endregion

        #region subscribeEvent
        colorPickManager.colorElecChanged += new ColorElecChangedEventHandler((color) => {
            colorPickButton.GetComponent<Image>().color = color;
            string name = elecContent.GetChild(currentIdElec).gameObject.name;
            colorElecForcedChange(name, color);
        });
        #endregion
    }

    void OnDestroy()
    {
        #region removeListener
        closeRemoteButton.onClick.RemoveAllListeners();
        sonificationButton.onClick.RemoveAllListeners();
        colorPickButton.onClick.RemoveAllListeners();
        gainMoreButton.onClick.RemoveAllListeners();
        gainLessButton.onClick.RemoveAllListeners();
        sm0.onClick.RemoveAllListeners();
        sm250.onClick.RemoveAllListeners();
        sm500.onClick.RemoveAllListeners();
        sm1000.onClick.RemoveAllListeners();
        sm2500.onClick.RemoveAllListeners();
        sm5000.onClick.RemoveAllListeners();
        #endregion

        colorPickManager.colorElecChanged -= new ColorElecChangedEventHandler((color) => {
            colorPickButton.GetComponent<Image>().color = color;
            string name = elecContent.GetChild(currentIdElec).gameObject.name;
            colorElecForcedChange(name, color);
        });

        for (int i = 0; i < elecContent.childCount; i++)
        {
            elecContent.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(elecContent.GetChild(i).gameObject);
        }
    }

    public void loadElectrodeInPanel(List<string> electrodeList)
    {
        if (elecContent.childCount > 0)
        {
            for (int i = 0; i < elecContent.childCount; i++)
            {
                elecContent.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(elecContent.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < electrodeList.Count; i++)
        {
            GameObject currentElectrode = Instantiate(ElecPlot);
            Button currentElecButton = currentElectrode.GetComponent<Button>();
            currentElecButton.onClick.AddListener(() =>
            {
                for (int j = 0; j < elecContent.childCount; j++)
                {
                    if (elecContent.GetChild(j).name == currentElecButton.name)
                    {
                        if (sphereColor.isValidForChange(currentElecButton.name))
                        {
                            changeIdElectrode(j);
                            changeColorElec();
                            break;
                        }
                    }
                }
            });

            Text currentElecText = currentElectrode.transform.GetChild(0).GetComponent<Text>();
            currentElecText.text = electrodeList[i];
            currentElectrode.name = electrodeList[i];
            currentElectrode.transform.SetParent(elecContent);
            currentElectrode.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void changeIdElectrode(int newID)
    {
        currentIdElec = newID;
        idElecHasChanged();
    }

    public void changeColorElec()
    {
        Color currentColor = colorPickButton.GetComponent<Image>().color;
        string name = elecContent.GetChild(currentIdElec).gameObject.name;
        colorElecHasChanged(name, currentColor);
    }

    bool checkHandle(int newID)
    {
        switch (newID)
        {
            case 0:
                if (btvMedia.e0 != null)
                    return true;
                else
                    return false;
            case 1:
                if (btvMedia.e250 != null)
                    return true;
                else
                    return false;
            case 2:
                if (btvMedia.e500 != null)
                    return true;
                else
                    return false;
            case 3:
                if (btvMedia.e1000 != null)
                    return true;
                else
                    return false;
            case 4:
                if (btvMedia.e2500 != null)
                    return true;
                else
                    return false;
            case 5:
                if (btvMedia.e5000 != null)
                    return true;
                else
                    return false;
            default:
                Debug.Log("Problem when choosing elecfile handle");
                return false;
        }
    }

    void changeButtonHighlightAlpha(Button myClickedButton, int alphaValue)
    {
        ColorBlock currentColor = myClickedButton.colors;
        currentColor.highlightedColor = new Color(currentColor.normalColor.r, currentColor.normalColor.g, currentColor.normalColor.b, alphaValue);
        myClickedButton.colors = currentColor;
    }
}
