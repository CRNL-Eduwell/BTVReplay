using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// The "Path roots" tab of the preferences window, built at runtime by cloning the existing
/// tab pieces (toggle, panel, the label + FolderSelector row, and the FolderSelector's own
/// browse button). The window is entirely driven by Horizontal/Vertical LayoutGroups, so this
/// tab works WITH the layout system - element widths come from LayoutElements and rows stack
/// via the panel's VerticalLayoutGroup; nothing is positioned by hand. Layout per line:
/// [name (fixed)] [folder selector (flexible)] [- remove (fixed)]. The header carries the
/// column labels plus a [+] that appends a row.
/// </summary>
public class PathRootsPreferencesTab : MonoBehaviour
{
    private const float NameColumnWidth = 200f;
    private const float IconButtonWidth = 24f;

    private RectTransform m_RowTemplate = null;
    private Button m_IconButtonTemplate = null;
    private Text m_GlyphTemplate = null;
    private readonly List<(RectTransform row, InputField name, FolderSelector folder)> m_Rows = new List<(RectTransform, InputField, FolderSelector)>();

    /// <summary>
    /// Clones a tab toggle and a content panel after the existing ones and returns the
    /// component driving the new panel. Call before the window enumerates its tabs.
    /// </summary>
    public static PathRootsPreferencesTab Build(Transform tabSelectorRoot, Transform tabContentRoot)
    {
        // New toggle: clone the last one. Its position is handled by the tab selectors'
        // HorizontalLayoutGroup, so we only relabel it (no manual placement).
        RectTransform lastToggle = (RectTransform)tabSelectorRoot.GetChild(tabSelectorRoot.childCount - 1);
        RectTransform newToggle = Instantiate(lastToggle, tabSelectorRoot);
        newToggle.name = "Path roots";
        Text toggleLabel = newToggle.GetComponentInChildren<Text>(true);
        if (toggleLabel != null) toggleLabel.text = "Path roots";
        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(false);
        toggle.onValueChanged.RemoveAllListeners();

        // New panel: clone the last content panel (a VerticalLayoutGroup), keep its single row
        // as the template for our root rows.
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

        // Clone sources for the inline icon buttons: the browse button (a small square button)
        // and the folder input's Text (for the +/- glyph, so the font matches).
        m_IconButtonTemplate = templateSelector.GetComponentInChildren<Button>(true);
        m_GlyphTemplate = templateSelector.GetComponentInChildren<InputField>(true).textComponent;

        Text title = transform.GetChild(0).GetComponentInChildren<Text>(true);
        if (title != null && title.transform.parent != m_RowTemplate)
            title.text = "Path roots";

        // Replace the row's first column (a fixed-200 label) with an editable name field, cloned
        // from the folder input so it matches the skin. Give it the same fixed width the label
        // had (the clone otherwise carries the folder input's *flexible* LayoutElement).
        Transform prefix = m_RowTemplate.GetChild(0);
        InputField sourceField = templateSelector.GetComponentInChildren<InputField>(true);
        InputField nameField = Instantiate(sourceField, m_RowTemplate);
        nameField.name = "RootName";
        nameField.transform.SetSiblingIndex(0);
        nameField.text = "";
        SetFixedWidth(nameField.gameObject, NameColumnWidth);
        Text placeholder = nameField.placeholder != null ? nameField.placeholder.GetComponent<Text>() : null;
        if (placeholder != null) placeholder.text = "NAME";
        AlignInputContentLeft(nameField);
        AlignInputContentLeft(sourceField);
        // Immediate: rows are cloned from this template within the same frame.
        DestroyImmediate(prefix.gameObject);

        m_RowTemplate.gameObject.SetActive(false);

        BuildHeaderRow();
    }

    // The static "Name | Folder" header above the rows, carrying the [+] add button.
    private void BuildHeaderRow()
    {
        RectTransform header = Instantiate(m_RowTemplate, m_RowTemplate.parent);
        header.name = "Header";
        header.gameObject.SetActive(true);

        ConvertToLabel(header.GetChild(0).gameObject, "Name");

        FolderSelector selector = header.GetComponentInChildren<FolderSelector>(true);
        Button browse = selector.GetComponentInChildren<Button>(true);
        InputField folderField = selector.GetComponentInChildren<InputField>(true);
        // Capture before ConvertToLabel destroys the InputField component.
        GameObject folderObject = folderField.gameObject;
        DestroyImmediate(selector);
        if (browse != null) DestroyImmediate(browse.gameObject);
        ConvertToLabel(folderObject, "Folder");

        BuildIconButton("+", AddRow, header);
    }

    public void AddRow()
    {
        CreateRow("", "");
    }

    private void RemoveRow(RectTransform row)
    {
        int index = m_Rows.FindIndex(r => r.row == row);
        if (index < 0) return;
        m_Rows.RemoveAt(index);
        Destroy(row.gameObject);
    }

    private void CreateRow(string name, string path)
    {
        RectTransform row = Instantiate(m_RowTemplate, m_RowTemplate.parent);
        row.gameObject.SetActive(true);

        InputField nameField = row.GetChild(0).GetComponent<InputField>();
        FolderSelector folderSelector = row.GetComponentInChildren<FolderSelector>(true);
        nameField.text = name;
        folderSelector.Text = path;

        BuildIconButton("-", () => RemoveRow(row), row);

        m_Rows.Add((row, nameField, folderSelector));
    }

    /// <summary>
    /// Shows exactly the configured roots (no trailing empty rows); the user adds more with the
    /// [+] button in the header.
    /// </summary>
    public void Populate(List<PathRoot> roots)
    {
        foreach ((RectTransform row, InputField name, FolderSelector folder) in m_Rows)
            Destroy(row.gameObject);
        m_Rows.Clear();

        for (int i = 0; i < roots.Count; i++)
            CreateRow(roots[i].Name, roots[i].Path);
    }

    /// <summary>
    /// Reads the rows back; rows missing a name or a folder are dropped.
    /// </summary>
    public List<PathRoot> CollectRoots()
    {
        List<PathRoot> roots = new List<PathRoot>();
        foreach ((RectTransform row, InputField name, FolderSelector folder) in m_Rows)
        {
            string rootName = name.text.Trim().Trim('$', '{', '}');
            string rootPath = folder.Text.Trim();
            if (rootName.Length > 0 && rootPath.Length > 0)
                roots.Add(new PathRoot(rootName, rootPath));
        }
        return roots;
    }

    // Clones the browse button into a fixed-width +/- button at the end of the row. The row's
    // HorizontalLayoutGroup positions and sizes it from the LayoutElement, so no manual rect.
    private void BuildIconButton(string glyph, UnityAction onClick, RectTransform parentRow)
    {
        Button button = Instantiate(m_IconButtonTemplate, parentRow);
        button.name = "IconButton";
        button.transform.SetAsLastSibling();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        // The browse button is an icon-only button (white glyph, no box) on this dark theme.
        // Drop its magnifier sprite and make the background transparent so the +/- glyph is the
        // only visible part; the Image still receives the click at alpha 0.
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = null;
            image.color = new Color(1f, 1f, 1f, 0f);
        }

        Text text = Instantiate(m_GlyphTemplate, button.transform);
        text.text = glyph;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.color = Color.white;
        text.fontSize = 20;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform textRect = (RectTransform)text.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        SetFixedWidth(button.gameObject, IconButtonWidth);
        button.gameObject.SetActive(true);
    }

    // Forces a fixed layout width: the row's HorizontalLayoutGroup sizes children from their
    // LayoutElement, so this is how a column gets a stable width regardless of content.
    private static void SetFixedWidth(GameObject go, float width)
    {
        LayoutElement element = go.GetComponent<LayoutElement>();
        if (element == null) element = go.AddComponent<LayoutElement>();
        element.minWidth = width;
        element.preferredWidth = width;
        element.flexibleWidth = 0f;
    }

    // Aligns both the live text and the placeholder to the left so an empty field's hint sits
    // where typed text would.
    private static void AlignInputContentLeft(InputField field)
    {
        if (field.textComponent != null)
            field.textComponent.alignment = TextAnchor.MiddleLeft;
        Text placeholder = field.placeholder as Text;
        if (placeholder != null)
            placeholder.alignment = TextAnchor.MiddleLeft;
    }

    // Strips the editing behaviour and background off an input field, leaving its text as a
    // plain label in the same spot (keeping its LayoutElement, so the column width is preserved).
    private static void ConvertToLabel(GameObject inputFieldObject, string text)
    {
        InputField field = inputFieldObject.GetComponent<InputField>();
        Text textComponent = field.textComponent;
        Graphic placeholder = field.placeholder;
        DestroyImmediate(field);
        if (placeholder != null) DestroyImmediate(placeholder.gameObject);
        Image background = inputFieldObject.GetComponent<Image>();
        if (background != null) DestroyImmediate(background);
        if (textComponent != null) textComponent.text = text;
    }
}
