using BTV.UI;
using BTV.Services;
using BTV.UI.Module3D;
using BTV.UI.Module3D.Tools;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the shared UI base classes that replaced the duplicated menu and
/// gain-tool code: the Menu open/hover state machine, the MenuBar mutual exclusion, and
/// the GainTool stepping rules (fine 0.25 steps inside [-1, 1], whole steps beyond).
/// </summary>
public class MenuAndGainToolTests
{
    private GameObject m_Root;

    [SetUp]
    public void SetUp()
    {
        m_Root = new GameObject("MenuAndGainToolTests");
    }

    [TearDown]
    public void TearDown()
    {
        if (m_Root != null) Object.DestroyImmediate(m_Root);
    }

    private Menu CreateMenu(string name)
    {
        GameObject menuObject = new GameObject(name);
        menuObject.transform.SetParent(m_Root.transform);
        return menuObject.AddComponent<Menu>();
    }

    /// <summary>
    /// Minimal concrete MenuBar; menus are assigned by the test and wired by calling
    /// Initialize() directly (edit-mode tests run without the MonoBehaviour lifecycle).
    /// </summary>
    private class TestMenuBar : MenuBar
    {
        public Menu[] menus;
        protected override Menu[] Menus { get { return menus; } }
    }

    private class TestTool : Tool
    {
        public int InitializeCount { get; private set; }
        protected override void OnInitialize() { InitializeCount++; }
    }

    private class TestToolbar : Toolbar
    {
        public Tool tool;
        protected override void AddTools() { m_Tools.Add(tool); }
    }

    private TestMenuBar CreateMenuBar(params Menu[] menus)
    {
        TestMenuBar bar = m_Root.AddComponent<TestMenuBar>();
        bar.menus = menus;
        bar.Initialize();
        return bar;
    }

    #region Menu state machine
    [Test]
    public void Menu_IsOpen_FiresChangeEventOnlyOnTransition()
    {
        Menu menu = CreateMenu("menu");
        int eventCount = 0;
        bool lastValue = false;
        menu.OnChangeOpenState.AddListener((isOpen) => { eventCount++; lastValue = isOpen; });

        menu.Open();
        Assert.IsTrue(menu.IsOpen);
        Assert.AreEqual(1, eventCount);
        Assert.IsTrue(lastValue);

        menu.Open();
        Assert.AreEqual(1, eventCount, "setting the same state again must not re-fire the event");

        menu.Close();
        Assert.IsFalse(menu.IsOpen);
        Assert.AreEqual(2, eventCount);
        Assert.IsFalse(lastValue);
    }

    [Test]
    public void Menu_SwapOpenState_Toggles()
    {
        Menu menu = CreateMenu("menu");

        menu.SwapOpenState();
        Assert.IsTrue(menu.IsOpen);
        menu.SwapOpenState();
        Assert.IsFalse(menu.IsOpen);
    }

    [Test]
    public void Menu_Hover_TracksPointerEnterAndExit()
    {
        Menu menu = CreateMenu("menu");
        bool? lastHover = null;
        menu.OnHover.AddListener((isHovered) => lastHover = isHovered);

        menu.OnPointerEnter(null);
        Assert.IsTrue(menu.IsHovered);
        Assert.AreEqual(true, lastHover);

        menu.OnPointerExit(null);
        Assert.IsFalse(menu.IsHovered);
        Assert.AreEqual(false, lastHover);
    }
    #endregion

    #region MenuBar mutual exclusion
    [Test]
    public void MenuBar_OpeningAMenu_ClosesTheOthersButNotItself()
    {
        Menu a = CreateMenu("a");
        Menu b = CreateMenu("b");
        Menu c = CreateMenu("c");
        CreateMenuBar(a, b, c);

        a.Open();
        Assert.IsTrue(a.IsOpen);

        b.Open();
        Assert.IsFalse(a.IsOpen, "opening b must close a");
        Assert.IsTrue(b.IsOpen, "opening b must not close b itself");
        Assert.IsFalse(c.IsOpen);
    }

    [Test]
    public void MenuBar_HoverSwitchesMenus_WhenOneIsOpen()
    {
        Menu a = CreateMenu("a");
        Menu b = CreateMenu("b");
        CreateMenuBar(a, b);

        a.Open();
        b.OnPointerEnter(null);

        Assert.IsTrue(b.IsOpen, "hovering b while the bar is open must open b");
        Assert.IsFalse(a.IsOpen, "switching to b must close a");
    }

    [Test]
    public void MenuBar_HoverDoesNothing_WhenAllMenusAreClosed()
    {
        Menu a = CreateMenu("a");
        Menu b = CreateMenu("b");
        CreateMenuBar(a, b);

        b.OnPointerEnter(null);

        Assert.IsFalse(b.IsOpen, "hovering must not open a menu when the bar is closed");
        Assert.IsFalse(a.IsOpen);
    }

    [Test]
    public void MenuBar_CloseAll_ClosesEverything()
    {
        Menu a = CreateMenu("a");
        Menu b = CreateMenu("b");
        TestMenuBar bar = CreateMenuBar(a, b);

        a.Open();
        bar.CloseAll();

        Assert.IsFalse(a.IsOpen);
        Assert.IsFalse(b.IsOpen);
    }
    #endregion

    #region Session injection
    [Test]
    public void Toolbar_InjectsTheSamePatientSessionIntoItsTools()
    {
        Session session = Session.Current;
        TestToolbar toolbar = m_Root.AddComponent<TestToolbar>();
        TestTool tool = m_Root.AddComponent<TestTool>();
        toolbar.tool = tool;

        toolbar.Initialize(session);

        Assert.AreSame(session, toolbar.PatientSession);
        Assert.AreSame(session, tool.PatientSession);
        Assert.AreEqual(1, tool.InitializeCount);
    }
    #endregion

    #region GainTool stepping
    [Test]
    public void GainTool_NextGain_FineStepsInsideCoarseStepsOutside()
    {
        Assert.AreEqual(0.25f, GainTool.NextGain(0f), 1e-4f);
        Assert.AreEqual(1f, GainTool.NextGain(0.75f), 1e-4f);
        Assert.AreEqual(-0.75f, GainTool.NextGain(-1f), 1e-4f);
        Assert.AreEqual(2f, GainTool.NextGain(1f), 1e-4f, "from 1 the step becomes a whole unit");
        Assert.AreEqual(3f, GainTool.NextGain(2f), 1e-4f);
        Assert.AreEqual(-1f, GainTool.NextGain(-2f), 1e-4f);
    }

    [Test]
    public void GainTool_PreviousGain_FineStepsInsideCoarseStepsOutside()
    {
        Assert.AreEqual(-0.25f, GainTool.PreviousGain(0f), 1e-4f);
        Assert.AreEqual(0.75f, GainTool.PreviousGain(1f), 1e-4f, "from 1 going down uses the fine step");
        Assert.AreEqual(-1f, GainTool.PreviousGain(-0.75f), 1e-4f);
        Assert.AreEqual(-2f, GainTool.PreviousGain(-1f), 1e-4f, "from -1 going down uses the whole step");
        Assert.AreEqual(1f, GainTool.PreviousGain(2f), 1e-4f);
    }

    [Test]
    public void GainTool_AddThenRemove_ReturnsToTheStartingGain()
    {
        // Walk up then back down from the default gain (1) along the reachable lattice.
        float gain = 1f;
        for (int i = 0; i < 5; i++) gain = GainTool.NextGain(gain);
        for (int i = 0; i < 5; i++) gain = GainTool.PreviousGain(gain);
        Assert.AreEqual(1f, gain, 1e-4f);

        for (int i = 0; i < 8; i++) gain = GainTool.PreviousGain(gain);
        for (int i = 0; i < 8; i++) gain = GainTool.NextGain(gain);
        Assert.AreEqual(1f, gain, 1e-4f);
    }
    #endregion
}
