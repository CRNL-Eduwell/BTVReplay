using UnityEngine;
using UnityEngine.UI;

using System.Text.RegularExpressions;

public class SphereColor : MonoBehaviour
{
    public GameObject brain3D = null;
    public GameObject sphereObject = null;
    public GameObject colorPicker = null;
    public Image colorPreviewPicker = null;
    public Dropdown dropDownScript = null;
    public selectRing ringScript = null;

    Color sphereColor, previousColor, defaultColor = Color.white;

    bool pickerCalled = false;
    bool firstInit = false;
    int previsousValue = 0;
    GameObject handlePreviousElec = null;
    GameObject handleNewElec = null;

    void Start ()
    {
        sphereColor = sphereObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color;
    }

    void Update ()
    {
        if (Input.GetMouseButtonUp(0))
        {
            checkSphereClicked();
        }

        checkChangeColor();
    }

    public void changeColorElec()
    {
        changeColorElecInBrain(sphereColor);
    }

    void checkSphereClicked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Casts the ray and goes through only if the sphere is hit
        if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.name == "Sphere")
        {
            colorPicker.SetActive(true);
            pickerCalled = true;
        }
    }

    void checkChangeColor()
    {
        if ((colorPicker.activeSelf == false) && (pickerCalled == true))
        {
            sphereColor = colorPreviewPicker.color;
            previousColor = sphereObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color;
            if (sphereColor != previousColor)
            {
                sphereObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = sphereColor;
                changeColorElecInBrain(sphereColor);
            }
            pickerCalled = false;
        }
    }

    void changeColorElecInBrain(Color newElectrodeColor)
    {
        string elecToLook = "", rootToLook = "";
        Regex ReLeft = new Regex(@"([a-zA-Z]+)(\d+)");
        Regex ReRight = new Regex(@"([a-zA-Z]+)(\')(\d+)");

        string electrode = dropDownScript.options[dropDownScript.value].text;

        Match resultLeft = ReLeft.Match(electrode);
        Match resultRight = ReRight.Match(electrode);

        if (resultLeft.Groups[1].Length == 1)
        {
            rootToLook = resultLeft.Groups[1].Value.ToLower();
            elecToLook = resultLeft.Groups[0].Value.ToLower();
        }
        else if (resultRight.Groups[1].Length == 1)
        {
            rootToLook = (resultRight.Groups[1] + "p").ToLower();
            elecToLook = (resultRight.Groups[1] + "p" + resultRight.Groups[3]).ToLower();
        }

        if (firstInit == false)
        {
            previsousValue = dropDownScript.value;
            handleNewElec = brain3D.transform.FindChild("Electrodes").transform.FindChild(rootToLook).transform.FindChild(elecToLook).gameObject;
            handlePreviousElec = handleNewElec;
            handlePreviousElec.transform.GetComponent<MeshRenderer>().materials[0].color = newElectrodeColor;
            firstInit = true;
            ringScript.setSelectedPlot(handleNewElec);
        }
        else
        {
            handleNewElec = brain3D.transform.FindChild("Electrodes").transform.FindChild(rootToLook).transform.FindChild(elecToLook).gameObject;
            ringScript.setSelectedPlot(handleNewElec);

            handlePreviousElec.transform.GetComponent<MeshRenderer>().materials[0].color = defaultColor;
            if (handleNewElec.transform.GetComponent<MeshRenderer>().materials[0].color == defaultColor)
            {
                handleNewElec.transform.GetComponent<MeshRenderer>().materials[0].color = newElectrodeColor;
                previsousValue = dropDownScript.value;
                handlePreviousElec = handleNewElec;
            }
            else
            {
                if (previsousValue < dropDownScript.value)
                {
                    dropDownScript.value += 1;
                }
                else
                {
                    dropDownScript.value -= 1;
                }
            }
        }

        colorPicker.SetActive(false);
    }
}
