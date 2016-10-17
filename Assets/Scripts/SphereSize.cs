using UnityEngine;
using System.Collections;

public class SphereSize : MonoBehaviour
{
    MainScript2 mainSc = null;

    GameObject electrode = null;
    public int elecID = 0;
    public int elecBipoleID = 0;
    public int indexToLook = 0;
    public float v = 0;
    public void InitializeData(string goName)
    {
        mainSc = GameObject.Find("Launch Func GameObject").GetComponent<MainScript2>();

        electrode = GameObject.Find(goName);

        elecID = mainSc.eHandle.nameElectrode.FindIndex(x => x.ToLower().Contains(electrode.name.Replace('p', '\'')));
        if (elecID != -1)
        {
            elecBipoleID = elecID;
        }
    }

    void Update()
    {
        if (mainSc.init == true && mainSc.vlcScript.videoPaused == false)
        {
            UpdateSphereSize();
        }
    }

    void UpdateSphereSize()
    {
        float scaleSize = 0;
        indexToLook = ((int)(mainSc.vlcScript.totalTimeMSec * 0.064));

        v = (float)mainSc.eHandle.eegData[elecBipoleID][indexToLook];
        scaleSize = 2 + (2 * ((float)mainSc.eHandle.eegData[elecBipoleID][indexToLook] * 5));
        electrode.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
    }
}
