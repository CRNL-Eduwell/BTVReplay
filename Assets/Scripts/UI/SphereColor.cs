using UnityEngine;
using System.Text.RegularExpressions;

public class SphereColor : MonoBehaviour
{
    GameObject handlePreviousElec = null;
    GameObject handleNewElec = null;
    RemoteManager myRemote = null;
    selectRing myRing = null;

    void Awake()
    {
        myRemote = gameObject.GetComponent<RemoteManager>();
        myRing = GameObject.Find("RingGO").GetComponent<selectRing>();
        myRemote.colorElecHasChanged += new colorElecChangedEventHandler(changeElectrodesColor);
        myRemote.colorElecForcedChange += new colorElecChangeForceEventHandler(forceChangeElectrodesColor);
    }

    void OnDestroy()
    {
        myRemote.colorElecHasChanged -= new colorElecChangedEventHandler(changeElectrodesColor);
        myRemote.colorElecForcedChange -= new colorElecChangeForceEventHandler(forceChangeElectrodesColor);
    }

    void changeElectrodesColor(string newElectrodeName, Color newColor)
    {
        GameObject newElectrode = getElecFromName(newElectrodeName);
        assignHandles(newElectrode);

        if (isValidForChange(newElectrode))
        {
            myRing.setSelectedPlot(newElectrode);
            handlePreviousElec.transform.GetComponent<MeshRenderer>().materials[0].color = Color.white;
            handleNewElec.transform.GetComponent<MeshRenderer>().materials[0].color = newColor;
        }
    }

    void forceChangeElectrodesColor(string newElectrodeName, Color newColor)
    {
        GameObject newElectrode = getElecFromName(newElectrodeName);
        assignHandles(newElectrode);
        myRing.setSelectedPlot(newElectrode);
        handleNewElec.transform.GetComponent<MeshRenderer>().materials[0].color = newColor;
    }

    GameObject getElecFromName(string name)
    {
        string elecToLook = "", rootToLook = "";
        Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
        Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

        Match resultLeft = ReLeft.Match(name);
        Match resultRight = ReRight.Match(name);

        if (resultLeft.Groups[1].Length == 1)
        {
            rootToLook = resultLeft.Groups[1].Value.ToLower();
            elecToLook = resultLeft.Groups[0].Value.ToLower();
        }
        else if (resultRight.Groups[1].Length == 1)
        {
            rootToLook = (resultRight.Groups[1] + "\'").ToLower();
            elecToLook = (resultRight.Groups[1] + "\'" + resultRight.Groups[3]).ToLower();
        }

        return GameObject.Find("GameObject").transform.FindChild("Electrodes").transform.FindChild(rootToLook).transform.FindChild(elecToLook).gameObject;
    }

    public bool isValidForChange(string name)
    {
        GameObject electrode = getElecFromName(name);
        if (electrode.transform.GetComponent<MeshRenderer>().materials[0].color == Color.white)
            return true;
        else
            return false;
    }

    public bool isValidForChange(GameObject clickedElec)
    {
        if (clickedElec.transform.GetComponent<MeshRenderer>().materials[0].color == Color.white)
            return true;
        else
            return false;
    }

    void assignHandles(GameObject newElectrode)
    {
        if (handlePreviousElec == null)
        {
            handlePreviousElec = newElectrode;
            handleNewElec = newElectrode;
        }
        else
        {
            handlePreviousElec = handleNewElec;
            handleNewElec = newElectrode;
        }
    }
}
