using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data.Factory;

public class DebugFlorian : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.D))
        //    SpawnDatabaseUI();
    }

    private void SpawnDatabaseUI()
    {
        GameObject m_prefab = Resources.Load("Prefabs/DBUserPreferences", typeof(GameObject)) as GameObject;
        GameObject canvas = GameObject.Find("Windows");
        GameObject m_PopUpAddWindow = Instantiate(m_prefab, canvas.transform);
        //PatientBaseGUIManager manager = m_PopUpAddWindow.GetComponent<PatientBaseGUIManager>();
    }
}
