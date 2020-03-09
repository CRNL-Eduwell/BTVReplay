using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadMedia : MonoBehaviour
{
    public Subject SubjectToReload { get; set; } = null;
    public int Id { get; set; } = -2;
    public string Path { get; set; } = "";
    public bool TriggerReload { get; set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (TriggerReload)
        {
            GameObject mediaGameObject = GameObject.Find("Canvas").transform.GetChild(1).gameObject;
            mediaGameObject.SetActive(true);
            mediaGameObject.SetActive(false);
            BTVMedia media = mediaGameObject.GetComponent<BTVMedia>();
            if (media != null)
            {
                PatientGUIManager patManager = media.transform.gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<PatientGUIManager>();
                patManager.SetSubjectToGUI(SubjectToReload);
                //==
                media.pm.LoadList(false, Path);
                media.pm.idCurrentPatientLoaded = Id;
                media.gameObject.SetActive(true);
                media.InstantiateDB();
                media.loadMedia(media.pm.Subjects[media.pm.idCurrentPatientLoaded]);
                Destroy(gameObject);
            }
        }
    }
}
