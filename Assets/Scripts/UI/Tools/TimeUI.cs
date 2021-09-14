using System;

using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public float TimeInSeconds
    {
        get
        {
            return (Hour * 3600) + (Min * 60) + Sec;
        }
        set
        {
            int h = Convert.ToInt32(value / 3600);
            int m = Convert.ToInt32((value / 60) % 60);
            int s = Convert.ToInt32(value % 60);

            HourField.text = h.ToString();
            MinField.text = m.ToString();
            SecField.text = s.ToString();
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
