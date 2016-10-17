using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TVCurvePerf : MonoBehaviour
{
    public GameObject linePerfObject = null;
    public VLCSharp vlcScript = null;
    public BTVMedia btvMedia = null;

    GameObject perfLRPrefab = null;
    List<GameObject> perfLine = new List<GameObject>();
    RectTransform linePerfRectTransObj = null;
    float widthOfGameObject = 0;
    float heightOfGameObject = 0;

    float horizontalScale = 0;
    float verticalScale = 0;

    int numberPoint = 64 * 30;
    int maxim = 0;

    public void init()
    {
        //instantiate object vertical line 
        perfLRPrefab = Resources.Load("Prefabs/perfLR", typeof(GameObject)) as GameObject;

        linePerfRectTransObj = linePerfObject.GetComponent<RectTransform>();
        widthOfGameObject = linePerfRectTransObj.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;

        heightOfGameObject = linePerfRectTransObj.rect.height;
        maxim = btvMedia.posF.reactionTimeMs.Max();
        verticalScale = heightOfGameObject / maxim;

        for (int i = 0; i < btvMedia.posF.sampleEventTrimmed.Count; i++)
        {        
            GameObject aa = Instantiate(perfLRPrefab);
            aa.transform.SetParent(linePerfObject.transform);
            aa.transform.localPosition = new Vector3(0, 0, 0);
            aa.name = "Perf" + (perfLine.Count);

            aa.transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(i, 0, 1));
            aa.transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(i, 100, 1));
            aa.transform.localScale = new Vector3(1, 1, 1);
            perfLine.Add(aa);

            aa.SetActive(false);
        }
    }

	void Update ()
    {
        if (btvMedia.perfOk == true)
        {
            if (linePerfRectTransObj != null)
            {
                if (linePerfRectTransObj.rect.width != widthOfGameObject)
                {
                    updateLineRendererHorizontalScale();
                }
            }

            if (vlcScript.videoPaused == false)
            {
                UpdateSpawn();
            }
        }
    }

    void updateLineRendererHorizontalScale()
    {
        widthOfGameObject = linePerfRectTransObj.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;
    }

    void updateLineRendererVerticalScale()
    {
        heightOfGameObject = linePerfRectTransObj.rect.height;
        verticalScale = heightOfGameObject / maxim;
    }

    void UpdateSpawn()
    {
        int leftTime = (int)(vlcScript.totalTimeMSec * 0.064) - numberPoint;
        int rightTime = (int)(vlcScript.totalTimeMSec * 0.064);

        List<int> currentIndex = btvMedia.posF.sampleEventTrimmed.Select((x, i) => new { x, i } )
                                                                 .Where(xx => xx.x >= leftTime && xx.x <= rightTime)
                                                                 .Select(xx => xx.i)
                                                                 .ToList();

        if (currentIndex.Count != 0)
        {
            for (int i = 0; i < perfLine.Count; i++)
            {
                perfLine[i].SetActive(false);
            }

            for (int i = 0; i < currentIndex.Count; i++)
            {
                perfLine[currentIndex[i]].SetActive(true);

                float v = ((leftTime - btvMedia.posF.sampleEventTrimmed[currentIndex[i]]) * -horizontalScale);
                perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(v, 0, 1));

                float value = verticalScale * btvMedia.posF.reactionTimeMs[currentIndex[i]];
                perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(v, value, 1));
            }
        }
    }

}
