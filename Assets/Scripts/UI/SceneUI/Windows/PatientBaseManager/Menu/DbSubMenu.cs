using UnityEngine;
using UnityEditor;
using BTV.Services.DatabaseService;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class DbSubMenu : MonoBehaviour
{
    public GenericEvent<string> ItemClicked = new GenericEvent<string>();

    public bool Show
    {
        get
        {
            return gameObject.activeSelf;
        }
        set
        {
            gameObject.SetActive(value);
        }
    }

    [SerializeField]
    private GameObject _ItemContainer = null;
    [SerializeField]
    private GameObject m_MenuSubItemPrefabs = null;

    public void AddSubMenuItem(string ItemName)
    {
        GameObject newMenu = Instantiate(m_MenuSubItemPrefabs, _ItemContainer.transform);
        newMenu.name = ItemName;
        newMenu.transform.GetChild(0).GetComponent<Text>().text = ItemName;
        newMenu.GetComponent<Button>().onClick.AddListener(()=> { ItemClicked.Invoke(newMenu.GetComponent<Button>().name); });
    }

    public void RemoveSubMenuItem(string itemName)
    {
        Transform container = _ItemContainer.transform;
        for (int i = 0; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            if (child != null && child.name == itemName)
            {
                child.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(child.gameObject);
                break;
            }
        }
    }
}