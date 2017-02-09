using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteManager : MonoBehaviour
{
    //=== Close Button
    public Button closeRemote;
    //=== Electrode List
    public Transform elecContent;
    GameObject ElecPlot;
    //=== Sonification
    public Button sonificationButton;
    public Sonification sonification;

    //Script : Curve Manager


    void Awake()
    {
        closeRemote.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        //Sonification
        sonificationButton.onClick.AddListener(() =>
        {
            sonification.toggleSonification();
        });

    }

    void Start()
    {
        ElecPlot = Resources.Load("Prefabs/OptCurve/Elec", typeof(GameObject)) as GameObject;
    }

    public void loadElectrodeInPanel(List<string> electrodeList)
    {
        if (elecContent.childCount > 0)
        {
            for (int i = 0; i < elecContent.childCount; i++)
                Destroy(elecContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < electrodeList.Count; i++)
        {
            GameObject currentElectrode = Instantiate(ElecPlot);
            Text currentElecText = currentElectrode.transform.GetChild(0).GetComponent<Text>();

            currentElecText.text = electrodeList[i];
            currentElectrode.name = electrodeList[i];
            currentElectrode.transform.SetParent(elecContent);
            currentElectrode.transform.localScale = new Vector3(1, 1, 1);
        }
    }

}
