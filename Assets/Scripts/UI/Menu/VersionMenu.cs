using UnityEngine;
using UnityEngine.UI;

public class VersionMenu : MonoBehaviour
{
    [SerializeField] Text _VersionLabel = null;

    private void Awake()
    {
        _VersionLabel.text = string.Format("{0} {1}", Application.productName, Application.version);
    }
}