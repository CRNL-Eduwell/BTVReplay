using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The "Path roots" tab of the preferences window, built at runtime by cloning the existing
/// tab pieces (toggle, panel, label + FolderSelector row) so it inherits the window's skin and
/// anchors without touching the prefab. Each row is a root name next to a folder selector;
/// the list always ends with empty rows for adding new roots, and clearing a row's name
/// removes that root on save.
/// </summary>
public class PathRootsPreferencesTab : MonoBehaviour
{
    private const int EmptyTrailingRows = 2;
    private const float RowSpacing = 6f;

    private RectTransform m_RowTemplate = null;
    private readonly List<(InputField name, FolderSelector folder)> m_Rows = new List<(InputField, FolderSelector)>();

    /// <summary>
    /// Clones a tab toggle and a content panel after the existing ones and returns the
    /// component driving the new panel. Call before the window enumerates its tabs.
    /// </summary>
    public static PathRootsPreferencesTab Build(Transform tabSelectorRoot, Transform tabContentRoot)
    {
        // New toggle: clone the last one and continue the spacing between the existing two.
        RectTransform firstToggle = (RectTransform)tabSelectorRoot.GetChild(0);
        RectTransform lastToggle = (RectTransform)tabSelectorRoot.GetChild(tabSelectorRoot.childCount - 1);
        RectTransform newToggle = Instantiate(lastToggle, tabSelectorRoot);
        newToggle.name = "Path roots";
        newToggle.anchoredPosition = lastToggle.anchoredPosition + (lastToggle.anchoredPosition - firstToggle.anchoredPosition);
        Text toggleLabel = newToggle.GetComponentInChildren<Text>(true);
        if (toggleLabel != null) toggleLabel.text = "Path roots";
        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(false);
        toggle.onValueChanged.RemoveAllListeners();

        // New panel: clone the last content panel (same anchors), keep its single row as the
        // template for our root rows.
        RectTransform panelTemplate = (RectTransform)tabContentRoot.GetChild(tabContentRoot.childCount - 1);
        RectTransform newPanel = Instantiate(panelTemplate, tabContentRoot);
        newPanel.name = "Path roots";
        newPanel.gameObject.SetActive(false);

        PathRootsPreferencesTab tab = newPanel.gameObject.AddComponent<PathRootsPreferencesTab>();
        tab.InitializeFromClonedPanel();
        return tab;
    }

    private void InitializeFromClonedPanel()
    {
        FolderSelector templateSelector = GetComponentInChildren<FolderSelector>(true);
        m_RowTemplate = (RectTransform)templateSelector.transform.parent;

        // The panel's first child is its title; explain the remove rule there.
        Text title = transform.GetChild(0).GetComponentInChildren<Text>(true);
        if (title != null && title.transform.parent != m_RowTemplate)
            title.text = "Path roots (clear a name to remove its root)";

        // Turn the cloned row into a template: replace its label by a name input field cloned
        // from the FolderSelector's own input field, so it matches the window's skin.
        Transform label = m_RowTemplate.GetChild(0);
        InputField sourceField = templateSelector.GetComponentInChildren<InputField>(true);
        InputField nameField = Instantiate(sourceField, m_RowTemplate);
        nameField.name = "RootName";
        CopyRect((RectTransform)label, (RectTransform)nameField.transform);
        nameField.transform.SetSiblingIndex(0);
        nameField.text = "";
        Text placeholder = nameField.placeholder != null ? nameField.placeholder.GetComponent<Text>() : null;
        if (placeholder != null) placeholder.text = "NAME";
        // Immediate: rows are cloned from this template within the same frame, and a deferred
        // Destroy would leave the dying label inside every clone.
        DestroyImmediate(label.gameObject);

        m_RowTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows one row per configured root plus empty rows to add new ones.
    /// </summary>
    public void Populate(List<PathRoot> roots)
    {
        foreach ((InputField name, FolderSelector folder) in m_Rows)
            Destroy(name.transform.parent.gameObject);
        m_Rows.Clear();

        for (int i = 0; i < roots.Count + EmptyTrailingRows; i++)
        {
            RectTransform row = Instantiate(m_RowTemplate, m_RowTemplate.parent);
            row.gameObject.SetActive(true);
            row.anchoredPosition = m_RowTemplate.anchoredPosition + new Vector2(0, -(m_RowTemplate.rect.height + RowSpacing) * i);

            InputField nameField = row.GetChild(0).GetComponent<InputField>();
            FolderSelector folderSelector = row.GetComponentInChildren<FolderSelector>(true);
            if (i < roots.Count)
            {
                nameField.text = roots[i].Name;
                folderSelector.Text = roots[i].Path;
            }
            m_Rows.Add((nameField, folderSelector));
        }
    }

    /// <summary>
    /// Reads the rows back; rows missing a name or a folder are dropped.
    /// </summary>
    public List<PathRoot> CollectRoots()
    {
        List<PathRoot> roots = new List<PathRoot>();
        foreach ((InputField name, FolderSelector folder) in m_Rows)
        {
            string rootName = name.text.Trim().Trim('$', '{', '}');
            string rootPath = folder.Text.Trim();
            if (rootName.Length > 0 && rootPath.Length > 0)
                roots.Add(new PathRoot(rootName, rootPath));
        }
        return roots;
    }

    private static void CopyRect(RectTransform source, RectTransform target)
    {
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
    }
}
