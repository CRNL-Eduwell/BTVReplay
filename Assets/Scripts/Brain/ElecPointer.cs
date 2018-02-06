using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElecPointer : MonoBehaviour
{
    [SerializeField] Text elecLabel = null;
    [SerializeField] Text marsAtlasLabel = null;
    [SerializeField] Text broadmanLabel = null;
    [SerializeField] Text corrdinatesLabel = null;

    public void show(bool showMe)
    {
        gameObject.SetActive(showMe);
    }

    public void moveTo(float x, float y, float z)
    {
        gameObject.transform.parent.transform.position = new Vector3(x, y, z);
    }

    public void setElecLabel(string label)
    {
        elecLabel.text = "Electrode name: " + label;
    }

    public void setMarsAtlasLabel(string label)
    {
        marsAtlasLabel.text = "Mars Atlas Parcel : " + label;
    }

    public void setBroadmanLabel(string label)
    {
        broadmanLabel.text = "Broadman Area : " + label;
    }

    public void setCorrdinatesLabel(Vector3 position)
    {
        corrdinatesLabel.text = "Coordinates : " + position.x + " " + position.y + " " + position.z;
    }
}
