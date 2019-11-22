using UnityEngine;
using System.Collections;

public class ApplicationManager : MonoBehaviour
{
    private void Awake()
    {
        ApplicationState.Module3D = FindObjectOfType<BTV3DModule>();
        //get User preference here when implementing
    }
    private void OnDestroy()
    {
        //clean data used
    }
}
