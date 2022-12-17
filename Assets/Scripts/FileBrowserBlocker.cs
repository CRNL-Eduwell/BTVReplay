using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowserBlocker : MonoBehaviour
{
    [SerializeField] private Image _BlockerImage = null;

    private void Awake()
    {
        FileBrowser.FileBrowserOpen.AddListener(OnFileBrowserOpen);
    }

    private void OnDestroy()
    {
        FileBrowser.FileBrowserOpen.RemoveAllListeners();
    }

    private void OnFileBrowserOpen(bool isOpen)
    {
        if (isOpen)
        {
            _BlockerImage.enabled = isOpen;
        }
        else
        {
            StartCoroutine(WaitAndClose(0.5f));
        }
    }

    private IEnumerator WaitAndClose(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _BlockerImage.enabled = false;
    }
}
