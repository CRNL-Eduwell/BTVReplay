using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class VersionWindow : MonoBehaviour
{
    [SerializeField] Button m_Close = null;
    [SerializeField] Text m_CurrentText = null;
    [SerializeField] Text m_LatestText = null;
    [SerializeField] Button m_GithubButton = null;
    [SerializeField] Button m_Submit = null;
    [SerializeField] Button m_Cancel = null;

    private void Start()
    {
        m_Close.onClick.AddListener(() => { Destroy(gameObject); });
        m_Submit.onClick.AddListener(() => { Destroy(gameObject); });
        m_Cancel.onClick.AddListener(() => { Destroy(gameObject); });

        SetFields();
    }

    private void OnDestroy()
    {
        m_Close.onClick.RemoveAllListeners();
        m_Submit.onClick.RemoveAllListeners();
        m_Cancel.onClick.RemoveAllListeners();
    }

    private void SetFields()
    {
        m_CurrentText.text = Application.version;
        using (WebClient wc = new WebClient())
        {
            try
            {
                wc.Headers.Add("User-Agent: Other");
                string jsonString = wc.DownloadString("https://api.github.com/repos/floriansipp/BTVReplay/releases/latest");
                var versionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(jsonString);
                m_LatestText.text = versionInfo.VersionNumber;
                m_GithubButton.onClick.AddListener(() => Application.OpenURL(versionInfo.URL));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_LatestText.text = "Unknown";
                m_GithubButton.onClick.RemoveAllListeners();
                m_GithubButton.onClick.AddListener(() => Application.OpenURL("https://github.com/floriansipp/BTVReplay"));
            }
        }
    }
}