using UnityEngine;

public class AnatomicalSite
{
    public string Label { get; set; } = "";
    public Vector3 Coordinates { get; set; }
    public string MarsAtlas { get; set; } = "";
    public string Broadmann { get; set; } = "";

    public AnatomicalSite(AnatomicalSite site)
    {
        Label = site.Label;
        Coordinates = site.Coordinates;
        MarsAtlas = site.MarsAtlas;
        Broadmann = site.Broadmann;
    }

    public AnatomicalSite(string label, Vector3 coordinates, string marsAtlas = "", string broadmann = "")
    {
        Label = label;
        Coordinates = coordinates;
        MarsAtlas = marsAtlas;
        Broadmann = broadmann;
    }

    public void Display()
    {
        BtvLog.Log("Anatomical Site");
        BtvLog.Log("Name : " + Label);
        BtvLog.Log("Coordinates : " + Coordinates.ToString());
    }
}
