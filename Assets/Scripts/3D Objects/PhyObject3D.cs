using UnityEngine;
using System.Collections;

public class PhyObject3D : MonoBehaviour
{
    public bool isLoaded = false;
    //====
    GameObject LHBrain = null;
    GameObject RHBrain = null;
    GameObject Electrodes = null;
    Electrodes ElectrodesScript = null;
    PhyObjCamera CameraScript = null;
    GameObject Object3DHandle = null;


    // Use this for initialization
    void Start ()
    {
        Object3DHandle = GameObject.Find("GameObject");

        CameraScript = GameObject.Find("CameraBrain").GetComponent<PhyObjCamera>();
    }

    public void loadBrainAndElectrodes(string LHtri, string RHtri, string PTS)
    {
        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(BrainHemi) });
        LHBrain.transform.parent = Object3DHandle.transform;
        LHBrain.layer = Object3DHandle.layer;
        LHBrain.GetComponent<BrainHemi>().InitializeData(LHtri);

        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(BrainHemi) });
        RHBrain.transform.parent = Object3DHandle.transform;
        RHBrain.layer = Object3DHandle.layer;
        RHBrain.GetComponent<BrainHemi>().InitializeData(RHtri);

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = Object3DHandle.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadPtsFile(PTS);
        ElectrodesScript.loadElecOnBrain();

        CameraScript.initCameraPosition();
    }

    public void loadElectrodes(string PTS)
    {
        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = Object3DHandle.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadPtsFile(PTS);
        ElectrodesScript.loadElecOnBrain();

        CameraScript.initElecCameraPosition();
    }
}
