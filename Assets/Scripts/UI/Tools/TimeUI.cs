using System;

using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public int TimeInSeconds
    {
        get
        {
            return (Hour * 3600) + (Min * 60) + Sec;
        }
    }
    public int Hour
    {
        get
        {
            return Convert.ToInt32(HourField.text);
        }
    }
    public int Min
    {
        get
        {
            return Convert.ToInt32(MinField.text);
        }
    }
    public int Sec
    {
        get
        {
            return Convert.ToInt32(SecField.text);
        }
    }

    [SerializeField]
    private InputField HourField = null;
	[SerializeField]
    private InputField MinField = null;
	[SerializeField]
    private InputField SecField = null;
}
