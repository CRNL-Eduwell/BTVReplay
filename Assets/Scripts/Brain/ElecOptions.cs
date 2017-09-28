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
    ElecPlotSize plotScript = null;
    bool isFrozen = true;

    public void init(GameObject clickedPlot)
    {
        plotOfInterest = clickedPlot;

        freezePlotButton = transform.GetChild(0).GetChild(0).GetComponent<Button>();
        freezeElecButton = transform.GetChild(0).GetChild(1).GetComponent<Button>();
        closeButton = transform.GetChild(0).GetChild(2).GetComponent<Button>();
        plotScript = plotOfInterest.GetComponent<ElecPlotSize>();

        if (plotScript.isFrozen)
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
        if (plotScript.isFrozen)
        {
            plotScript.isFrozen = false;
        }
        else
        {
            plotScript.isFrozen = true;
            plotScript.fixSize();
        }

        choiceClose();
    }

    void freezeElec()
    {
        for (int i = 0; i < parentElec.transform.childCount; i++)
        {
            ElecPlotSize currentPlot = parentElec.transform.GetChild(i).GetComponent<ElecPlotSize>();
            currentPlot.isFrozen = !isFrozen;
            if(currentPlot.isFrozen)
                currentPlot.fixSize();
        }

        choiceClose();
    }

    bool isElecFrozen(GameObject parentElec)
    {
        bool isTotalyFrozen = true;
        for (int i = 0; i < parentElec.transform.childCount; i++)
        {
            ElecPlotSize currentPlot = parentElec.transform.GetChild(i).GetComponent<ElecPlotSize>();
            isTotalyFrozen = isTotalyFrozen && currentPlot.isFrozen;
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
