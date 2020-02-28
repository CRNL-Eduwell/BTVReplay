using BTV.Data;
using BTV.Services.EegFileService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TracesDisplayer : MonoBehaviour
{
    [SerializeField]
    private BTVMedia media = null;
    [SerializeField]
    private LayoutElement m_ParentLayoutElement = null;
    [SerializeField]
    private LineRenderer m_LineRenderer = null;






    private BtvProgram FileHandle = null;
    private BtvChannel Channel = null;

    private Vector3[] m_dataArray = null;
    protected RectTransform m_rectTransform = null;

    private void Awake()
    {
        m_rectTransform = gameObject.transform.GetComponent<RectTransform>();

        media.loadTrace += new initTrace(Init);
    }

    private void OnDestroy()
    {
        media.loadTrace -= new initTrace(Init);
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateHorizontalScale();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateDraw(m_ParentLayoutElement.minHeight);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            m_ParentLayoutElement.minHeight = 120;
            UpdateDraw(m_ParentLayoutElement.minHeight);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            m_ParentLayoutElement.minHeight = 30;
            UpdateDraw(m_ParentLayoutElement.minHeight);
        }
    }

    private void Init()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();
        Channel = FileHandle.Channels[0];

        m_dataArray = new Vector3[Channel.NumberOfSample];
        m_LineRenderer.positionCount = Channel.NumberOfSample;
        m_LineRenderer.startWidth = 0.02f;
        m_LineRenderer.endWidth = 0.02f;
        UpdateHorizontalScale();
    }

    private void UpdateHorizontalScale()
    {
        if (m_rectTransform == null) return;
        if (m_dataArray == null) return;

        UnityEngine.Debug.Log("Update HorizontalScale");
        float widthOfGameObject = m_rectTransform.rect.width;
        float horizontalScale = widthOfGameObject / m_dataArray.Length;
        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        }
        m_LineRenderer.SetPositions(m_dataArray);
    }

    private void UpdateDraw(float height)
    {
        float limitVal = height / 2;
        float[] data = Channel.Data;
        float min = data.Min();
        float max = data.Max();


        for (int i = 0; i < m_dataArray.Length; i++)
        {
            m_dataArray[i].y = ((Channel.GetSample(i, true) - min) / (max-min)) * limitVal;
        }
        m_LineRenderer.SetPositions(m_dataArray);
    }
}
