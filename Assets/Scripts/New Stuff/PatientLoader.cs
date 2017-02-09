using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class PatientLoader : MonoBehaviour
{
    public List<Pat> currentPats = new List<Pat>();
    string pathFile = @"D:\Users\Florian\Desktop\PatientReplay.txt";

    public void SaveList()
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(pathFile))
            {
                for (int i = 0; i < currentPats.Count; i++)
                {
                    sw.WriteLine("LH : " + currentPats[i].lhemi);
                    sw.WriteLine("RH : " + currentPats[i].rhemi);
                    sw.WriteLine("PTS : " + currentPats[i].pts);
                    sw.WriteLine("POS : " + currentPats[i].pos);
                    sw.WriteLine("SM0 : " + currentPats[i].sm0);
                    sw.WriteLine("SM250 : " + currentPats[i].sm250);
                    sw.WriteLine("SM500 : " + currentPats[i].sm500);
                    sw.WriteLine("SM1000 : " + currentPats[i].sm1000);
                    sw.WriteLine("SM2500 : " + currentPats[i].sm2500);
                    sw.WriteLine("SM5000 : " + currentPats[i].sm5000);
                    sw.WriteLine("PROV : " + currentPats[i].prov);
                    sw.WriteLine("VID : " + currentPats[i].video);
                    sw.WriteLine("[----------]");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error writing file", e.ToString());
        }
    }

    public void LoadList()
    {
        try
        {
            if (currentPats.Count > 0)
                currentPats = new List<Pat>();

            using (StreamReader sr = new StreamReader(pathFile))
            {
                string[] fileSplited = sr.ReadToEnd().Split(new string[] { "[----------]" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fileSplited.Length - 1; i++) // -1 because of last line jump
                {
                    Pat currentPat = new Pat();
                    string[] currentPatSplit = fileSplited[i].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < currentPatSplit.Length; j++)
                    {
                        string[] splitPath = currentPatSplit[j].Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                        currentPat.loadValue(j, splitPath);
                    }

                    currentPats.Add(currentPat);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Reading file", e.ToString());
        }
    }

    public void addPat(Pat thisPatient)
    {
        currentPats.Add(new Pat(thisPatient));
    }

    public void removePatAt(int index)
    {
        if (currentPats.Count > 0)
        {
            currentPats.Remove(currentPats[index]);
        }
    }
}

public class Pat
{
    public string lhemi = "";
    public string rhemi = "";
    public string pts = "";

    public string pos = "";
    public string sm0 = "";
    public string sm250 = "";
    public string sm500 = "";
    public string sm1000 = "";
    public string sm2500 = "";
    public string sm5000 = "";

    public string prov = "";
    public string video = "";

    public Pat()
    {

    }

    public Pat(Pat thisPat)
    {
        lhemi = thisPat.lhemi;
        rhemi = thisPat.rhemi;
        pts = thisPat.pts;

        pos = thisPat.pos;
        sm0 = thisPat.sm0;
        sm250 = thisPat.sm250;
        sm500 = thisPat.sm500;
        sm1000 = thisPat.sm1000;
        sm2500 = thisPat.sm2500;
        sm5000 = thisPat.sm5000;

        prov = thisPat.prov;
        video = thisPat.video ;
    }

    public void loadValue(int val, string[] data)
    {
        switch (val)
        {
            case 0: if (data.Length > 1) lhemi = data[1]; break;
            case 1: if (data.Length > 1) rhemi = data[1]; break;
            case 2: if (data.Length > 1) pts = data[1]; break;
            case 3: if (data.Length > 1) pos = data[1]; break;
            case 4: if (data.Length > 1) sm0 = data[1]; break;
            case 5: if (data.Length > 1) sm250 = data[1]; break;
            case 6: if (data.Length > 1) sm500 = data[1]; break;
            case 7: if (data.Length > 1) sm1000 = data[1]; break;
            case 8: if (data.Length > 1) sm2500 = data[1]; break;
            case 9: if (data.Length > 1) sm5000 = data[1]; break;
            case 10: if (data.Length > 1) prov = data[1]; break;
            case 11: if (data.Length > 1) video = data[1]; break;
            default: Debug.LogError("Problem with patients file"); break;
        }
    }
}