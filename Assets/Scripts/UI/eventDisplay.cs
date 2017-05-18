using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Collections.Generic;

public class eventDisplay : MonoBehaviour
{
    public VLCSharp.VLCSharp vlcScript = null;
    public BTVMedia_New media = null;
    public MainScript2 main = null;

    GameObject picGO = null;
    RawImage picDisplayer = null;
    List<int> mainCodes = new List<int>();
    List<Texture2D> picEvent = new List<Texture2D>();

    public void init()
    {
        picGO = gameObject.transform.GetChild(0).gameObject;
        picDisplayer = gameObject.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();

        for (int i = 0; i < media.provF.blocs.Count; i++)
        {
            if (File.Exists(media.provF.blocs[i].dispBloc.path))
            {
                Texture2D current = new Texture2D(256, 256);
                current.LoadImage(File.ReadAllBytes(media.provF.blocs[i].dispBloc.path));

                picEvent.Add(current);
            }

            mainCodes.Add(media.provF.blocs[i].mainEvent.code);
        }
    }

    void Update()
    {
        if (media.perfOk == true)
        {
            if (main.init && vlcScript.player.IsPlaying)
            {
                int found = media.posF1.Triggers.FindIndex(x => x.trigger.sample >= vlcScript.time - 16 && x.trigger.sample < vlcScript.time + 16);

                if (found != -1 && mainCodes.Contains(media.posF1.Triggers[found].trigger.code))
                {
                    if (picGO.activeSelf == false)
                    {
                        picGO.SetActive(true);
                        picDisplayer.texture = picEvent[mainCodes.IndexOf(media.posF1.Triggers[found].trigger.code)];
                    }
                }
                else
                {
                    if (picGO.activeSelf == true)
                        picGO.SetActive(false);
                }
            }
        }
    }
}
