using System.IO;
using System.Reflection;
using BTV.Services.DatabaseService;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Edit-mode tests for the move/copy destination submenu of the patient-base manager. It used to
/// send the clicked button's name, and the window looked the destination up by short name: with
/// two open databases called alike (the same file name in two folders) every pick reached the
/// first one, and the two entries read the same.
/// </summary>
public class DbSubMenuTests
{
    private string m_TestDir;
    private GameObject m_Root, m_Prefab;
    private DbSubMenu m_Menu;

    [SetUp]
    public void SetUp()
    {
        m_TestDir = Path.Combine(Path.GetTempPath(), "BTVReplayTests_" + Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(m_TestDir);
        ClearDatabases();

        // The submenu as the scene wires it: a container and an item prefab (a Button whose
        // first child is the label).
        m_Root = new GameObject("submenu");
        m_Prefab = new GameObject("item", typeof(RectTransform), typeof(Button));
        new GameObject("label", typeof(RectTransform), typeof(Text)).transform.SetParent(m_Prefab.transform, false);
        m_Menu = m_Root.AddComponent<DbSubMenu>();
        typeof(DbSubMenu).GetField("_ItemContainer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(m_Menu, m_Root);
        typeof(DbSubMenu).GetField("m_MenuSubItemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(m_Menu, m_Prefab);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Root);
        Object.DestroyImmediate(m_Prefab);
        ClearDatabases();
        if (Directory.Exists(m_TestDir)) Directory.Delete(m_TestDir, true);
    }

    private static void ClearDatabases()
    {
        while (DatabaseService.Databases.Count > 0)
            DatabaseService.DeleteDatabase(DatabaseService.Databases[0]);
    }

    private SubjectRepository CreateDatabase(string folder, string name)
    {
        Directory.CreateDirectory(Path.Combine(m_TestDir, folder));
        Assert.IsTrue(DatabaseService.CreateNewDatabase(Path.Combine(m_TestDir, folder, name + ".dbtv2")));
        return DatabaseService.Databases[DatabaseService.Databases.Count - 1];
    }

    private string Label(int index)
    {
        return m_Root.transform.GetChild(index).GetChild(0).GetComponent<Text>().text;
    }

    [Test]
    public void ClickingAnEntry_SendsThatDatabase_EvenWhenAnotherHasTheSameName()
    {
        SubjectRepository first = CreateDatabase("a", "base");
        SubjectRepository second = CreateDatabase("b", "base");
        m_Menu.AddSubMenuItem(first);
        m_Menu.AddSubMenuItem(second);
        SubjectRepository clicked = null;
        m_Menu.ItemClicked.AddListener(database => clicked = database);

        m_Root.transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
        Assert.AreSame(second, clicked, "the second entry is the second database");

        m_Root.transform.GetChild(0).GetComponent<Button>().onClick.Invoke();
        Assert.AreSame(first, clicked, "the first entry is the first database");
    }

    [Test]
    public void EntriesWithTheSameName_ShowTheirFolder()
    {
        SubjectRepository first = CreateDatabase("a", "base");
        m_Menu.AddSubMenuItem(first);
        Assert.AreEqual("base", Label(0), "a name used once is shown alone");

        m_Menu.AddSubMenuItem(CreateDatabase("b", "base"));
        m_Menu.AddSubMenuItem(CreateDatabase("b", "other"));

        Assert.AreEqual("base (a)", Label(0), "the entry already in the menu is relabelled when a second database takes its name");
        Assert.AreEqual("base (b)", Label(1));
        Assert.AreEqual("other", Label(2));
    }

    [Test]
    public void RenamingADatabase_RelabelsBothEntries()
    {
        SubjectRepository first = CreateDatabase("a", "base");
        SubjectRepository second = CreateDatabase("b", "base");
        m_Menu.AddSubMenuItem(first);
        m_Menu.AddSubMenuItem(second);

        Assert.IsTrue(DatabaseService.UpdateDatabaseName(second, "renamed"));

        Assert.AreEqual("base", Label(0), "the name is no longer shared");
        Assert.AreEqual("renamed", Label(1));
    }
}
