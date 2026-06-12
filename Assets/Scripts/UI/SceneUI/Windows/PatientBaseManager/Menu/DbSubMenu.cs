using UnityEngine;
using BTV.Services.DatabaseService;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

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

    private Dictionary<SubjectRepository, GameObject> m_ChildElements = new Dictionary<SubjectRepository, GameObject>();

    public void AddSubMenuItem(SubjectRepository item)
    {
        GameObject itemObject = AddSubMenuItem(item.ShortName);
        item.PropertyChanged += DatabaseInformationUpdated;
        m_ChildElements.Add(item, itemObject);
    }

    private void DatabaseInformationUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ShortName")
        {
            BtvLog.Log("DBSubmenu : Database Name updated");
            SubjectRepository item = sender as SubjectRepository;
            if (m_ChildElements.ContainsKey(item))
            {
                GameObject itemObject = m_ChildElements[item];
                // ItemClicked sends Button.name: keep it in sync or copy/move to a renamed
                // database keeps targeting the old name and fails.
                itemObject.name = item.ShortName;
                itemObject.transform.GetChild(0).GetComponent<Text>().text = item.ShortName;
            }
        }
    }

    private GameObject AddSubMenuItem(string ItemName)
    {
        GameObject newMenu = Instantiate(m_MenuSubItemPrefabs, _ItemContainer.transform);
        newMenu.name = ItemName;
        newMenu.transform.GetChild(0).GetComponent<Text>().text = ItemName;
        newMenu.GetComponent<Button>().onClick.AddListener(()=> { ItemClicked.Invoke(newMenu.GetComponent<Button>().name); });
        return newMenu;
    }

    public void RemoveSubMenuItem(SubjectRepository item)
    {
        if (m_ChildElements.ContainsKey(item))
        {
            item.PropertyChanged -= DatabaseInformationUpdated;
            GameObject itemObject = m_ChildElements[item];
            m_ChildElements.Remove(item);
            Destroy(itemObject);
        }
    }

    private void OnDestroy()
    {
        // SubjectRepository instances live in the static DatabaseService and outlive this menu.
        // Without dropping our PropertyChanged subscriptions here, a later rename fires into this
        // destroyed object and throws MissingReferenceException (it touches destroyed child UI).
        foreach (SubjectRepository item in m_ChildElements.Keys)
        {
            item.PropertyChanged -= DatabaseInformationUpdated;
        }
        m_ChildElements.Clear();
    }
}