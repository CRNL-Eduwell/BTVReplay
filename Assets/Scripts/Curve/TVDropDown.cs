using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TVDropDown : MonoBehaviour
{
    public GameObject telecommande = null;
    public Dropdown dropDown = null;
    public Text dropDownText = null;

    public void loadElectrodeListInDropDown(string[] electrodeList, int sizeArray)
    {
        dropDown.options.Clear();

        for (int i = 0; i < sizeArray; i++)
        {
            dropDown.options.Add(new Dropdown.OptionData(electrodeList[i]));
        }

        dropDownText.text = dropDown.options[dropDown.value].text;
    }

    public void programPlus()
    {
        dropDown.value += 1;
    }

    public void programMinus()
    {
        dropDown.value -= 1;
    }

    public void showMe(bool yesOrNo)
    {
        telecommande.SetActive(yesOrNo);
    }
}
