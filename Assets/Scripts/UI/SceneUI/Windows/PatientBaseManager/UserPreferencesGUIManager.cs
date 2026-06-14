using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTV.Services.UserPreferencesService;

public class UserPreferencesGUIManager : MonoBehaviour
{
    [SerializeField] private Button _HeaderClose = null;
    [SerializeField] private FolderSelector _DBPathSelector = null;
    [SerializeField] private Button _Save = null;
    [SerializeField] private Button _Cancel = null;

    // Path-roots editor: design lives in prefabs (PathRootsSection = header + [+] add button +
    // a Rows container; PathRootRow = name field + folder selector + [-] remove). This manager
    // only instantiates them and moves data to/from the preferences.
    private Transform m_RowsParent = null;
    private GameObject m_RowPrefab = null;
    private readonly List<Transform> m_RootRows = new List<Transform>();

    private void Awake()
    {
        _HeaderClose.onClick.AddListener(Close);
        _DBPathSelector.Text = UserPreferencesService.UserPreferences.DatabasePreferences.Path;
        _Save.onClick.AddListener(Save);
        _Cancel.onClick.AddListener(Close);

        BuildPathRootsSection();
    }

    private void OnDestroy()
    {
        _HeaderClose.onClick.RemoveAllListeners();
        _Save.onClick.RemoveAllListeners();
        _Cancel.onClick.RemoveAllListeners();
    }

    private void BuildPathRootsSection()
    {
        m_RowPrefab = Resources.Load<GameObject>("Prefabs/PathRootRow");
        GameObject sectionPrefab = Resources.Load<GameObject>("Prefabs/PathRootsSection");
        if (m_RowPrefab == null || sectionPrefab == null)
        {
            Debug.LogError("UserPreferencesGUIManager: path-roots prefabs missing from Resources/Prefabs.");
            return;
        }

        // Drop the section into the same options panel that holds the database-path row
        // (FolderSelector -> row -> panel), so it stacks below it via the panel's layout group.
        Transform panel = _DBPathSelector.transform.parent.parent;
        Transform section = Instantiate(sectionPrefab, panel).transform;
        section.name = "PathRootsSection";

        m_RowsParent = section.Find("Rows");
        Button addButton = section.Find("Header/AddButton").GetComponent<Button>();
        addButton.onClick.AddListener(AddRow);

        foreach (PathRoot root in UserPreferencesService.UserPreferences.DatabasePreferences.PathRoots)
            CreateRow(root.Name, root.Path);
    }

    private void AddRow()
    {
        CreateRow("", "");
    }

    private void CreateRow(string name, string path)
    {
        Transform row = Instantiate(m_RowPrefab, m_RowsParent).transform;
        row.Find("Name").GetComponent<InputField>().text = name;
        row.GetComponentInChildren<FolderSelector>().Text = path;
        row.Find("Remove").GetComponent<Button>().onClick.AddListener(() => RemoveRow(row));
        m_RootRows.Add(row);
    }

    private void RemoveRow(Transform row)
    {
        m_RootRows.Remove(row);
        Destroy(row.gameObject);
    }

    private List<PathRoot> CollectRoots()
    {
        List<PathRoot> roots = new List<PathRoot>();
        foreach (Transform row in m_RootRows)
        {
            string name = row.Find("Name").GetComponent<InputField>().text.Trim().Trim('$', '{', '}');
            string path = row.GetComponentInChildren<FolderSelector>().Text.Trim();
            if (name.Length > 0 && path.Length > 0)
                roots.Add(new PathRoot(name, path));
        }
        return roots;
    }

    private void Save()
    {
        UserPreferencesService.UserPreferences.DatabasePreferences.Path = _DBPathSelector.Text;
        UserPreferencesService.UserPreferences.DatabasePreferences.PathRoots = CollectRoots();
        UserPreferencesService.SavePreferences();
        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
