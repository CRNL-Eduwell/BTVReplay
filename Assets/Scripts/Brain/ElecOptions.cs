using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElecOptions : MonoBehaviour
{
    Button freezePlotButton = null;
    Button freezeElecButton = null;
    Button closeButton = null;

    GameObject parentElec = null;
    GameObject plotOfInterest = null;
    Site plotScript = null;
    bool isFrozen = true;

    public void init(GameObject clickedPlot)
    {
        plotOfInterest = clickedPlot;

        freezePlotButton = transform.GetChild(0).GetChild(0).GetComponent<Button>();
        freezeElecButton = transform.GetChild(0).GetChild(1).GetComponent<Button>();
        closeButton = transform.GetChild(0).GetChild(2).GetComponent<Button>();
        plotScript = plotOfInterest.GetComponent<Site>();

        if (plotScript.IsFrozen)
            freezePlotButton.transform.GetChild(0).GetComponent<Text>().text = "Unfreeze Plot";
        else
            freezePlotButton.transform.GetChild(0).GetComponent<Text>().text = "Freeze Plot";

        parentElec = plotOfInterest.transform.parent.gameObject;
        isFrozen = isElecFrozen(parentElec);
        if (isFrozen)
            freezeElecButton.transform.GetChild(0).GetComponent<Text>().text = "Unfreeze Full Electrode";
        else
            freezeElecButton.transform.GetChild(0).GetComponent<Text>().text = "Freeze Full Electrode";

        freezePlotButton.onClick.AddListener(freezePlot);

        freezeElecButton.onClick.AddListener(freezeElec);

        closeButton.onClick.AddListener(choiceClose);
    }

    void freezePlot()
    {
        if (plotScript.IsFrozen)
            plotScript.IsFrozen = false;
        else
            plotScript.IsFrozen = true;

        choiceClose();
    }

    void freezeElec()
    {
        for (int i = 0; i < parentElec.transform.childCount; i++)
        {
            Site currentPlot = parentElec.transform.GetChild(i).GetComponent<Site>();
            currentPlot.IsFrozen = !isFrozen;
        }

        choiceClose();
    }

    bool isElecFrozen(GameObject parentElec)
    {
        bool isTotalyFrozen = true;
        for (int i = 0; i < parentElec.transform.childCount; i++)
        {
            Site currentPlot = parentElec.transform.GetChild(i).GetComponent<Site>();
            isTotalyFrozen = isTotalyFrozen && currentPlot.IsFrozen;
        }
        return isTotalyFrozen;
    }

    void choiceClose()
    {
        freezePlotButton.onClick.RemoveAllListeners();
        freezeElecButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
