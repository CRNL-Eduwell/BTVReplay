using UnityEngine;

public class SphereSize : MonoBehaviour
{
    MainScript2 mainSc = null;
    TVCurve curve1 = null;
    GameObject electrode = null;
    public int elecID = 0;
    public int elecBipoleID = 0;
    public float currentValue = 0;
    public float scaleSize = 0;

    public void InitializeData(string goName)
    {
        mainSc = GameObject.Find("Launch Func GameObject").GetComponent<MainScript2>();
        curve1 = GameObject.Find("LRObject1").GetComponent<TVCurve>();
        electrode = GameObject.Find(goName.ToLower());

        elecID = mainSc.elecList.FindIndex(x => x.ToLower().Equals(electrode.name));
        if (elecID != -1)
        {
            elecBipoleID = elecID;
        }
        else
        {
            electrode.SetActive(false);
            //Destroy(this);
        }
    }

    void FixedUpdate()
    {
        if (mainSc.init && mainSc.vlcScript.player.IsPlaying)
        {
            UpdateSphereSize();
        }
    }

    void UpdateSphereSize()
    {
        int sampleToLook = (int)mainSc.vlcScript.time;// ((int)(mainSc.vlcScript.player.currentTime * 0.064));
        int posInArray = sampleToLook + (elecBipoleID * curve1.eHandle.nbSam);
        currentValue = curve1.eHandle.eegData[posInArray] / 100;
        scaleSize = 2 + (2 * currentValue);
        electrode.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
    }
}
