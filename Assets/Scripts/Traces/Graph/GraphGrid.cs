using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGrid : MonoBehaviour
{
    public bool IsOn
    {
        get
        {
            return m_display;
        }
        set
        {
            if (m_display != value)
            {
                m_display = value;
                displayTimeGrid(value);
            }
        }
    }
    [SerializeField] GameObject m_gridContainter = null;

    GameObject m_gridLineSec = null, m_gridLineMilliSec = null;
    bool m_display = false;

    public void init(int initialPeriod)
    {
        m_gridLineSec = Resources.Load("Prefabs/ImageGrid", typeof(GameObject)) as GameObject;
        m_gridLineMilliSec = Resources.Load("Prefabs/ImageGridMS", typeof(GameObject)) as GameObject;

        updateGridScale(initialPeriod);
        displayTimeGrid(m_display);
    }

    public void updateGridScale(int newPeriod)
    {
        for (int i = m_gridContainter.transform.childCount - 1; i >= 0; i--)
            Destroy(m_gridContainter.transform.GetChild(i).gameObject);

        for (int i = 0; i < newPeriod; i++)
        {
            GameObject newLine = Instantiate(m_gridLineSec);
            newLine.name = "line " + i;
            newLine.transform.SetParent(m_gridContainter.transform);
            newLine.transform.localScale = new Vector3(1, 1, 1);
            newLine.transform.localPosition = new Vector3(newLine.transform.localPosition.x, newLine.transform.localPosition.y, 0);
            newLine.SetActive(m_display);

            if (newPeriod <= 3)
            {
                for (int j = 0; j < 4; j++)
                {
                    GameObject newLineMS = Instantiate(m_gridLineMilliSec);
                    newLineMS.name = "lineMs " + i;
                    newLineMS.transform.SetParent(m_gridContainter.transform);
                    newLineMS.transform.localScale = new Vector3(1, 1, 1);
                    newLineMS.transform.localPosition = new Vector3(newLineMS.transform.localPosition.x, newLineMS.transform.localPosition.y, 0);
                    newLineMS.SetActive(m_display);
                }
            }
        }
    }

    public void displayTimeGrid(bool isGridOn)
    {
        for (int i = 0; i < m_gridContainter.transform.childCount; i++)
            m_gridContainter.transform.GetChild(i).gameObject.SetActive(isGridOn);
    }
}
