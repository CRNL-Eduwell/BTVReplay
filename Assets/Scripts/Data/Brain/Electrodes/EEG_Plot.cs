using UnityEngine;

public class EEG_Plot
{
    public string Label
    {
        get;
        set;
    }
    public Vector3 Coordinates
    {
        get;
        set;
    }

    public EEG_Plot(string label, Vector3 coordinates)
    {
        Label = label;
        Coordinates = coordinates;
    }

    public void display()
    {
        Debug.Log("Name : " + Label);
        Debug.Log("Coordinates : " + Coordinates.ToString());
    }
}
