using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadMedia : MonoBehaviour
{
    public Subject SubjectToReload { get; set; } = null;
    public string ExperimentName { get; set; } = "";
    public bool TriggerReload { get; set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (TriggerReload)
        {
            ApplicationState.ResetAllServices();
            LoadSubjectMessage message = new LoadSubjectMessage
            {
                subject = new Subject(SubjectToReload),
                label = ExperimentName
            };
            Messenger.Default.Send(message, MessageContext.LoadSubjectMessage);
            Destroy(gameObject);
        }
    }
}
