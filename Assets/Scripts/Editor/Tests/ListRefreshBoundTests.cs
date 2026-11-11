using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Tools.Unity.Lists;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Edit-mode tests for the Refresh loop of the virtualized lists. m_LastIndexDisplayed is an
/// exclusive end (one past the last object shown). SelectableList and ActionableList looped with
/// `i <= m_LastIndexDisplayed` where List uses `<`: harmless while there are exactly as many
/// items as objects in range, but with more items than that (a mask applied, then a removal, before
/// the next Update resizes the items) the loop read one object past the end of the displayed list.
/// </summary>
public class ListRefreshBoundTests
{
    // Awake does not run in edit mode, so the items never cache their Toggle: the test item
    // answers Interactable itself and the test lists hand each item its Toggle.
    public class TestSelectableItem : SelectableItem<string>
    {
        public override bool Interactable { get { return true; } set { } }
    }

    public class TestActionableItem : ActionnableItem<string>
    {
        public override bool Interactable { get { return true; } set { } }
    }

    private static void GiveToggle(Item<string> item)
    {
        typeof(SelectableItem<string>).GetField("m_Toggle", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(item, item.GetComponent<Toggle>());
    }

    public interface ITestList
    {
        void Prepare(float itemHeight);
        string[] ShownObjects { get; }
    }

    public class TestSelectableList : SelectableList<string>, ITestList
    {
        public void Prepare(float itemHeight) { m_ItemHeight = itemHeight; }
        public string[] ShownObjects { get { return m_Items.Select(item => item.Object).ToArray(); } }
        protected override void SetItem(Item<string> item, string obj) { GiveToggle(item); base.SetItem(item, obj); }
    }

    public class TestActionableList : ActionableList<string>, ITestList
    {
        public void Prepare(float itemHeight) { m_ItemHeight = itemHeight; }
        public string[] ShownObjects { get { return m_Items.Select(item => item.Object).ToArray(); } }
        protected override void SetItem(Item<string> item, string obj) { GiveToggle(item); base.SetItem(item, obj); }
    }

    private GameObject m_Root, m_Prefab;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Root);
        Object.DestroyImmediate(m_Prefab);
    }

    // A 100-high viewport of 20-high items, so up to 6 items are spawned.
    private TList Build<TList, TItem>() where TList : SelectableList<string>, ITestList where TItem : Component
    {
        m_Root = new GameObject("list", typeof(RectTransform));
        GameObject viewport = new GameObject("viewport", typeof(RectTransform));
        viewport.transform.SetParent(m_Root.transform, false);
        GameObject content = new GameObject("content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        ((RectTransform)viewport.transform).sizeDelta = new Vector2(100, 100);
        ((RectTransform)content.transform).sizeDelta = Vector2.zero;

        ScrollRect scroll = m_Root.AddComponent<ScrollRect>();
        scroll.viewport = (RectTransform)viewport.transform;
        scroll.content = (RectTransform)content.transform;

        m_Prefab = new GameObject("item", typeof(RectTransform), typeof(Toggle), typeof(TItem));

        TList list = m_Root.AddComponent<TList>();
        list.ScrollRect = scroll;
        list.ItemPrefab = m_Prefab;
        list.Prepare(20f);
        return list;
    }

    // One frame of the list: its Update spawns, moves and refreshes the items.
    private static void Frame(SelectableList<string> list)
    {
        typeof(Tools.Unity.Lists.List<string>).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(list, null);
    }

    private void MaskThenRemove<TList, TItem>() where TList : SelectableList<string>, ITestList where TItem : Component
    {
        // Removing an object destroys its item with Destroy(), which logs an error in edit mode.
        LogAssert.ignoreFailingMessages = true;
        TList list = Build<TList, TItem>();
        string[] objects = { "o0", "o1", "o2", "o3", "o4" };
        foreach (string obj in objects) list.Add(obj);
        Frame(list);
        CollectionAssert.AreEquivalent(objects, list.ShownObjects, "all five objects are on screen");

        // Same frame: keep two objects, then remove one of them. Four items are left for one
        // object in range, and the range ends at the end of the displayed list.
        Assert.IsTrue(list.MaskList(new[] { true, true, false, false, false }));
        Assert.DoesNotThrow(() => list.Remove("o0"), "Refresh must stop at the last displayed object");

        Assert.AreEqual(4, list.ShownObjects.Length);
        CollectionAssert.Contains(list.ShownObjects, "o1", "the object still displayed is on an item");
        CollectionAssert.DoesNotContain(list.Objects, "o0");
    }

    [Test]
    public void SelectableList_Refresh_StopsAtTheLastDisplayedObject()
    {
        MaskThenRemove<TestSelectableList, TestSelectableItem>();
    }

    [Test]
    public void ActionableList_Refresh_StopsAtTheLastDisplayedObject()
    {
        MaskThenRemove<TestActionableList, TestActionableItem>();
    }
}
