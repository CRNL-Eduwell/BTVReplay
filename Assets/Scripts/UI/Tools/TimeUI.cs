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
    // Safe parse: the fields are user-editable, so empty/non-numeric input must yield 0 rather
    // than throwing FormatException (which callers like RecordVideoWindow did not handle).
    public int Hour
    {
        get
        {
            int.TryParse(HourField.text, out int value);
            return value;
        }
    }
    public int Min
    {
        get
        {
            int.TryParse(MinField.text, out int value);
            return value;
        }
    }
    public int Sec
    {
        get
        {
            int.TryParse(SecField.text, out int value);
            return value;
        }
    }

    [SerializeField]
    private InputField HourField = null;
	[SerializeField]
    private InputField MinField = null;
	[SerializeField]
    private InputField SecField = null;
}
