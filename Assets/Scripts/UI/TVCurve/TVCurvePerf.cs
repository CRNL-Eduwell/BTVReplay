using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class TVCurvePerf : MonoBehaviour
{
    public GameObject linePerfObject = null;
    public VLCSharp.VLCSharp vlcScript = null;
    public BTVMedia_New btvMedia = null;

    GameObject perfLRPrefab = null;
    List<GameObject> perfLine = new List<GameObject>();
    RectTransform linePerfRectTransObj = null;
    public float widthOfGameObject = 0;
    float heightOfGameObject = 0;

    float horizontalScale = 0;
    float verticalScale = 0;

    int numberPoint = 64 * 10;  //sampling frequency * numb of line renderer max in space
    int maxim = 0;

    public int leftTime = 0;
    public int rightTime = 0;

    void OnDestroy()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i));
        }
    }

    public void init()
    {
        //instantiate object vertical line 
        perfLRPrefab = Resources.Load("Prefabs/perfLR", typeof(GameObject)) as GameObject;

        linePerfRectTransObj = linePerfObject.GetComponent<RectTransform>();
        widthOfGameObject = linePerfRectTransObj.rect.width;
        horizontalScale = widthOfGameObject / numberPoint;

        heightOfGameObject = linePerfRectTransObj.rect.height;
        maxim = btvMedia.posF1.rtMsMax;
        verticalScale = heightOfGameObject / maxim;

        for (int i = 0; i < btvMedia.posF1.Triggers.Count; i++)
        {
            GameObject aa = Instantiate(perfLRPrefab);
            aa.transform.SetParent(linePerfObject.transform);
            aa.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            aa.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            aa.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            aa.transform.localPosition = new Vector3(0, 0, 0);
            aa.name = "Perf" + (perfLine.Count);

            aa.transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(i, 0, 1));
            aa.transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(i, 100, 1));
            aa.transform.localScale = new Vector3(1, 1, 1);

            aa.SetActive(false);
            perfLine.Add(aa);
        }
    }

    void Update()
    {
        if (btvMedia.perfOk == true && btvMedia.loaded == true)
        {
            if (vlcScript.player != null && vlcScript.player.IsPlaying)
            {
                UpdateSpawn();
            }
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (linePerfRectTransObj != null)
        {
            if (linePerfRectTransObj.rect.width != widthOfGameObject)
            {
                updateLineRendererHorizontalScale();
                updateLineRendererVerticalScale();
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
        leftTime = ((int)(vlcScript.time) - numberPoint);
        rightTime = (int)(vlcScript.time);

        List<int> currentIndex = btvMedia.posF1.Triggers.Select((item, index) => new { Item = item, Index = index })
                                                         .Where(x => x.Item.trigger.sample > leftTime && x.Item.trigger.sample < rightTime)
                                                         .Select(x => x.Index)
                                                         .ToList();

        if (currentIndex.Count != 0)
        {
            for (int i = currentIndex[0] - 1; i >= 0; i--)
            {
                if (perfLine[i].activeSelf == true)
                {
                    perfLine[i].SetActive(false);
                }
            }


            for (int i = currentIndex[currentIndex.Count - 1] + 1; i < perfLine.Count; i++)
            {
                if (perfLine[i].activeSelf == true)
                {
                    perfLine[i].SetActive(false);
                }
            }

            for (int i = 0; i < currentIndex.Count; i++)
            {
                float sampleEventPlusResp = (btvMedia.posF1.Triggers[currentIndex[i]].trigger.sample + btvMedia.posF1.Triggers[currentIndex[i]].rtSample);
                float positionInsideRect = (leftTime - sampleEventPlusResp) * -horizontalScale;

                if (sampleEventPlusResp <= rightTime)
                {
                    perfLine[currentIndex[i]].SetActive(true);
                    perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(0, new Vector3(positionInsideRect, 0, 1));

                    float value = verticalScale * (btvMedia.posF1.Triggers[currentIndex[i]].rtMs - 750);
                    perfLine[currentIndex[i]].transform.GetComponent<LineRenderer>().SetPosition(1, new Vector3(positionInsideRect, value, 1));
                }
            }
        }
        else //No New obj, we clean if there is some left
        {
            List<GameObject> activeObj = perfLine.FindAll(x => x.activeSelf == true);
            if (activeObj.Count > 0)
            {
                for (int i = 0; i < activeObj.Count; i++)
                {
                    activeObj[i].SetActive(false);
                }
            }
        }
    }
}
