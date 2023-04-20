using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTfValueDisplay : MonoBehaviour
{
    [SerializeField] private GameObject m_RootImageObject = null;
    [SerializeField] private Text m_FrequencyLabel = null;
    [SerializeField] private Text m_PowerTimeLabel = null;

    private const float DELAY = 0.2f;
    private float m_Timer = 0.0f;

    private void OnGUI()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= 1)
        {
            Show(false);
            m_Timer = 0.0f;
        }
    }

    public void Show(bool isVisible)
    {
        m_RootImageObject.SetActive(isVisible);
    }

    public void UpdateDisplayInformation(float frequency, float value)
    {
        m_FrequencyLabel.text = frequency.ToString();
        m_PowerTimeLabel.text = value.ToString();
    }
}
