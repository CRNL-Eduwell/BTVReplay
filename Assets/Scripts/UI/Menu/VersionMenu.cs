using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class VersionMenu : MonoBehaviour
{
    [SerializeField] Text _VersionLabel = null;
    [SerializeField] Image m_Image = null;

    private void Awake()
    {
        _VersionLabel.text = string.Format("{0} {1}", Application.productName, Application.version);
        //using (WebClient wc = new WebClient())
        //{
        //    try
        //    {
        //        wc.Headers.Add("User-Agent: Other");
        //        string jsonString = wc.DownloadString("https://api.github.com/repos/CRNL-Eduwell/BTVReplay/releases/latest");
        //        var versionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(jsonString);
        //        BtvLog.Log(versionInfo.VersionNumber);
        //        m_Image.gameObject.SetActive(string.Compare(versionInfo.VersionNumber, Application.version) > 0);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogException(e);
        //    }
        //}
    }
}