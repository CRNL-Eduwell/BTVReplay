using UnityEngine;
using BTV.Services.DatabaseService;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

public class DbSubMenu : MonoBehaviour
{
    // Carries the database itself. It used to carry the button's name, and the window looked the
    // destination up by that name: with two open databases called alike (same file name in two
    // folders), every pick landed on the first one.
    public GenericEvent<SubjectRepository> ItemClicked = new GenericEvent<SubjectRepository>();

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
        GameObject itemObject = Instantiate(m_MenuSubItemPrefabs, _ItemContainer.transform);
        itemObject.GetComponent<Button>().onClick.AddListener(() => { ItemClicked.Invoke(item); });
        item.PropertyChanged += DatabaseInformationUpdated;
        m_ChildElements.Add(item, itemObject);
        RefreshLabels();
    }

    // Every label, not only the one that changed: opening, closing or renaming a database can
    // create or resolve a name shared with another entry (DatabaseService.DisplayName).
    private void RefreshLabels()
    {
        foreach (KeyValuePair<SubjectRepository, GameObject> element in m_ChildElements)
        {
            string label = DatabaseService.DisplayName(element.Key);
            element.Value.name = label;
            element.Value.transform.GetChild(0).GetComponent<Text>().text = label;
        }
    }

    private void DatabaseInformationUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ShortName")
        {
            BtvLog.Log("DBSubmenu : Database Name updated");
            RefreshLabels();
        }
    }

    public void RemoveSubMenuItem(SubjectRepository item)
    {
        if (m_ChildElements.ContainsKey(item))
        {
            item.PropertyChanged -= DatabaseInformationUpdated;
            GameObject itemObject = m_ChildElements[item];
            m_ChildElements.Remove(item);
            Destroy(itemObject);
            RefreshLabels();
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