using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VersionWindow : MonoBehaviour
{
    [SerializeField] Button m_Close = null;
    [SerializeField] Text m_CurrentText = null;
    [SerializeField] Text m_LatestText = null;
    [SerializeField] Button m_GithubButton = null;
    [SerializeField] Button m_Submit = null;
    [SerializeField] Button m_Cancel = null;

    // The repository moved to the CRNL-Eduwell organisation. While it is private the API
    // answers 404 without credentials, so the check shows "Unknown" and the button opens the repo.
    private const string k_RepoUrl = "https://github.com/CRNL-Eduwell/BTVReplay";
    private const string k_LatestReleaseApi = "https://api.github.com/repos/CRNL-Eduwell/BTVReplay/releases/latest";

    private void Start()
    {
        m_Close.onClick.AddListener(() => { Destroy(gameObject); });
        m_Submit.onClick.AddListener(() => { Destroy(gameObject); });
        m_Cancel.onClick.AddListener(() => { Destroy(gameObject); });

        m_CurrentText.text = Application.version;
        m_LatestText.text = "Checking...";
        // Async via coroutine: the old synchronous WebClient.DownloadString froze the UI thread
        // until GitHub responded (or timed out).
        StartCoroutine(FetchLatestVersion());
    }

    private void OnDestroy()
    {
        m_Close.onClick.RemoveAllListeners();
        m_Submit.onClick.RemoveAllListeners();
        m_Cancel.onClick.RemoveAllListeners();
        m_GithubButton.onClick.RemoveAllListeners();
    }

    private IEnumerator FetchLatestVersion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(k_LatestReleaseApi))
        {
            request.SetRequestHeader("User-Agent", "BTVReplay");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var versionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(request.downloadHandler.text);
                    m_LatestText.text = versionInfo.VersionNumber;
                    m_GithubButton.onClick.RemoveAllListeners();
                    m_GithubButton.onClick.AddListener(() => Application.OpenURL(versionInfo.URL));
                    yield break;
                }
                catch (Exception e)
                {
                    // A failed background version check is not worth a bug report.
                    BtvLog.Handled("Version check: could not read the release information", e);
                }
            }
            else
            {
                Debug.LogWarning("Version check failed: " + request.error);
            }

            m_LatestText.text = "Unknown";
            m_GithubButton.onClick.RemoveAllListeners();
            m_GithubButton.onClick.AddListener(() => Application.OpenURL(k_RepoUrl));
        }
    }
}
