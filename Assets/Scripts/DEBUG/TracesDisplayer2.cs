using BTV.Data;
using BTV.Services.EegFileService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TracesDisplayer2 : MonoBehaviour
{
    [SerializeField]
    private BTVMedia media = null;
    [SerializeField]
    private LayoutElement m_ParentLayoutElement = null;
    [SerializeField]
    private RawImage m_RawImage = null;

    private BtvProgram FileHandle = null;
    private BtvChannel Channel = null;

    private Vector3[] m_dataArray = null;
    protected RectTransform m_rectTransform = null;

    private Texture2D m_CurrentTexture = null;
    private Color[] m_TextureColorData;

    private Color hardBlue = new Color(0.6117f, 0.7058f, 0.7960f, 1f);

    private void Awake()
    {
        m_rectTransform = gameObject.transform.GetComponent<RectTransform>();
        m_CurrentTexture = (Texture2D)m_RawImage.texture;
        m_TextureColorData = m_CurrentTexture.GetPixels();

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
            StartCoroutine(UpdateDraw(m_ParentLayoutElement.minHeight));
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
        //m_LineRenderer.positionCount = Channel.NumberOfSample;
        //m_LineRenderer.startWidth = 0.02f;
        //m_LineRenderer.endWidth = 0.02f;
        UpdateHorizontalScale();
    }

    private void UpdateHorizontalScale()
    {
        //if (m_rectTransform == null) return;
        //if (m_dataArray == null) return;

        //UnityEngine.Debug.Log("Update HorizontalScale");
        //float widthOfGameObject = m_rectTransform.rect.width;
        //float horizontalScale = widthOfGameObject / m_dataArray.Length;
        //for (int i = 0; i < m_dataArray.Length; i++)
        //{
        //    m_dataArray[i].x = ((-widthOfGameObject / 2) + 1) + i * horizontalScale;
        //}
        //m_LineRenderer.SetPositions(m_dataArray);
    }

    private IEnumerator UpdateDraw(float height)
    {
        float limitVal = height / 2;
        float[] data = Channel.Data;
        float min = data.Min();
        float max = data.Max();

        UnityEngine.Debug.Log(m_CurrentTexture.width);
        UnityEngine.Debug.Log(m_CurrentTexture.height);
        for (int i = 0; i < 50;i++)// data.Length; i++)
        {
            int x = Mathf.RoundToInt(((float)i / data.Length) * m_CurrentTexture.width);
            float y = (Channel.GetSample(i, true) - min) / (max - min);

            //m_TextureColorData[x + y] = Color.red;
            UnityEngine.Debug.Log(x + " et " + y);

        }

        yield return null;
    }

    private void ResetTextureValueOnTheFly()
    {
        int textureSize = m_TextureColorData.Length;
        for (int i = 0; i < textureSize; i++)
            m_TextureColorData[i] = hardBlue;
    }
}
