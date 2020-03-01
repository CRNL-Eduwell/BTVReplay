using BTV.Data;
using BTV.Services.CalculationService;
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
    [SerializeField]
    private SelectableLabel m_ElectrodeLabel = null;
    [SerializeField]
    private SelectableLabel m_GainLabel = null;
    [SerializeField]
    private SelectableLabel m_FileLabel = null;


    float MaxValue { get; set; } = 150;
    float MinValue { get; set; } = 50;

    private BtvProgram FileHandle = null;
    private BtvChannel Channel = null;

    private Vector3[] m_dataArray = null;
    protected RectTransform m_rectTransform = null;

    private int m_currentElectrodeID = 0;
    private float m_Gain = 1;
    private int m_ContainerId = 0;

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

        //zoom scroll mouse
        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (scrollDelta.y != 0)
        {
            if (m_ElectrodeLabel.HasFocus)
            {
                int newId = scrollDelta.y < 0 ? m_currentElectrodeID - 1 : m_currentElectrodeID + 1;
                UpdateElectrode(newId);
            }
            else if (m_GainLabel.HasFocus)
            {
                m_Gain = scrollDelta.y < 0 ? m_Gain - 0.25f : m_Gain + 0.25f;
                m_GainLabel.Text.text = m_Gain.ToString();
            }
            else if (m_FileLabel.HasFocus)
            {
                UpdateFile(scrollDelta.y);
            }

            UpdateDraw(m_ParentLayoutElement.minHeight);
        }

    }

    private void Init()
    {
        FileHandle = EegFileService.ReturnFirstValidContainer();

        m_currentElectrodeID = 0;
        Channel = FileHandle.Channels[m_currentElectrodeID];

        m_dataArray = new Vector3[Channel.NumberOfSample];
        m_LineRenderer.positionCount = Channel.NumberOfSample;
        m_LineRenderer.startWidth = 0.02f;
        m_LineRenderer.endWidth = 0.02f;

        UpdateElectrode(m_currentElectrodeID);
        //==
        m_GainLabel.Text.text = m_Gain.ToString();
        //==
        UpdateFile(-1);
        //==
        UpdateHorizontalScale();
        UpdateDraw(m_ParentLayoutElement.minHeight);
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

        for (int i = 0; i < m_dataArray.Length; i++)
        {
            float value = m_Gain * (Channel.GetSample(i, true) / (MaxValue - MinValue)) * limitVal;
            if (value >= -limitVal && value <= limitVal)
            {
                m_dataArray[i].y = value;
            }
            else
            {
                if (value >= 0)
                    m_dataArray[i].y = limitVal;
                else
                    m_dataArray[i].y = -limitVal;
            }
        }

        m_LineRenderer.SetPositions(m_dataArray);
    }

    private void UpdateElectrode(int Index)
    {
        if (Index > -1 && Index < FileHandle.NumberOfElectrodes)
        {
            m_currentElectrodeID = Index;
            Channel = FileHandle.Channels[m_currentElectrodeID];
            m_ElectrodeLabel.Text.text = Channel.Label;
        }
    }

    private void UpdateFile(float yDelta)
    {
        if (yDelta < 0)
        {
            if (m_ContainerId - 1 >= 0 && EegFileService.IsFileIdValid(m_ContainerId - 1))
            {
                m_ContainerId -= 1;
            }
        }
        else
        {
            if (EegFileService.IsFileIdValid(m_ContainerId + 1))
            {
                m_ContainerId += 1;
            }
        }
        FileHandle = EegFileService.ChangeContainerHandle(FileHandle, m_ContainerId);
        Channel = FileHandle.Channels[m_currentElectrodeID];
        m_FileLabel.Text.text = "File " + m_ContainerId.ToString();
    }
}