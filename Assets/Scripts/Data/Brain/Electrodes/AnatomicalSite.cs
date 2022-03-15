using UnityEngine;

public class AnatomicalSite
{
    public string Label { get; set; }
    public Vector3 Coordinates { get; set; }

    public AnatomicalSite(string label, Vector3 coordinates)
    {
        Label = label;
        Coordinates = coordinates;
    }

    public void Display()
    {
        Debug.Log("Anatomical Site");
        Debug.Log("Name : " + Label);
        Debug.Log("Coordinates : " + Coordinates.ToString());
    }
}
