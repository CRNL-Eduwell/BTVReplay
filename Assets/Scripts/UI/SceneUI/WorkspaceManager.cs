using System.Collections;
using Assets.Scripts.Data.Factory;
using UnityEngine;
using UnityEngine.UI;
using BTV.Services;

public class WorkspaceManager : MonoBehaviour
{
    [SerializeField] ResizableGrid m_grid = null;
    [SerializeField] WindowLayout _LeftWindowLayout = null;
    [SerializeField] WindowLayout _RightWindowLayout = null;
    [SerializeField] BrainWarden _BrainWarden = null;
    [SerializeField] Trace _Trace1 = null;
    [SerializeField] Trace _Trace2 = null;
    private Session m_PatientSession = null;

    private void Start()
    {
        Messenger.Default.Register<UiToLayoutsMessage>(this, OnUiToLayoutsMessage, MessageContext.UiToLayouts);
        Messenger.Default.Register<ShortcutMessage>(this, OnShortcutMessage, MessageContext.ShortcutMessage);
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        StartCoroutine(InitDisplayWhenRendered());
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToLayouts);
        Messenger.Default.Unregister(this, MessageContext.ShortcutMessage);
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.MediaLoader && Session.IsCurrent(message.PatientSession))
            m_PatientSession = message.PatientSession;
    }

    //Since we don't intantiate the workspace but it is already there, it is subjected to the rendering
    //phase of unity , and therefore passes through stages where it's rect is 0,0 and has some NaN data
    //we want to init the display only when the container is rendered. That is until we do the whole
    //thing dynamically at the loading time, which should make the issue disapear because the area
    //will already be rendered.
    //Waiting in a one-shot coroutine instead of polling every frame in Update(): once the grid has a
    //valid rect we init once and the coroutine ends, so there is no per-frame work for the app lifetime.
    private IEnumerator InitDisplayWhenRendered()
    {
        yield return new WaitUntil(() =>
            m_grid.RectTransform.rect.width > 0 && m_grid.RectTransform.rect.height > 0);
        if (!m_grid.InitDone)
            InitDisplay();
    }

    private void InitDisplay()
    {
        BtvLog.Log("Init");
        m_grid.Init();

        m_grid.VerticalHandlers[0].MagneticPosition = 0.495f;
        m_grid.VerticalHandlers[1].MagneticPosition = 0.99f;

        m_grid.VerticalHandlers[0].Position = 0.495f;
        m_grid.VerticalHandlers[1].Position = 0.99f;

        m_grid.SetVerticalHandlersPosition(0);
        m_grid.SetVerticalHandlersPosition(1);
        //
        m_grid.UpdateAnchors();
        m_grid.InitDone = true;
    }

    private void OnUiToLayoutsMessage(UiToLayoutsMessage message)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        switch (message.TaskToExecute)
        {
            case UiToLayoutsMessage.Task.Load:
                {
                    BtvLog.Log("Should Load a layout from : " + message.Path);
                    LoadLayout(message.Path);
                    break;
                }
            case UiToLayoutsMessage.Task.Save:
                {
                    BtvLog.Log("Should save layout at : " + message.Path);
                    SaveLayout(message.Path);
                    break;
                }
        }
    }

    private void OnShortcutMessage(ShortcutMessage message)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        if (message.RecipientType == typeof(Trace))
        {
            switch (message.Action)
            {
                case ShortcutActions.Focus:
                    {
                        ForceToggleToolbar toggleMessage = new ForceToggleToolbar
                        {
                            toolbar = "EEG" + message.RecipientIndex.ToString()
                        };
                        Messenger.Default.Send(toggleMessage, MessageContext.ForceToggleToolbar);
                    }
                    break;
                case ShortcutActions.Move:
                    {
                        Trace move = message.RecipientIndex == 1 ? _Trace1 : message.RecipientIndex == 2 ? _Trace2 : null;
                        if (move != null)
                        {
                            Window ObjectToMoveWindow = move.GetComponent<Window>();
                            Transform parent = move.transform.parent;
                            switch (message.Parameter)
                            {
                                case ShortcutActionsParameters.Left:
                                    {
                                        if (parent != _LeftWindowLayout.transform.parent)
                                        {
                                            _LeftWindowLayout.ForceDrop(move.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId);
                                        }
                                    }
                                    break;
                                case ShortcutActionsParameters.Right:
                                    {
                                        if (parent != _RightWindowLayout.transform.parent)
                                        {
                                            _RightWindowLayout.ForceDrop(move.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId);
                                        }
                                    }
                                    break;
                                case ShortcutActionsParameters.Up:
                                    {
                                        if (ObjectToMoveWindow.windowId + 1 < 3)
                                        {
                                            WindowLayout currentLayout = parent == _LeftWindowLayout.transform ? _LeftWindowLayout : _RightWindowLayout;
                                            currentLayout.ForceDrop(ObjectToMoveWindow.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId + 1);
                                        }
                                    }
                                    break;
                                case ShortcutActionsParameters.Down:
                                    {
                                        if (ObjectToMoveWindow.windowId - 1 >= 0)
                                        {
                                        WindowLayout currentLayout = parent == _LeftWindowLayout.transform ? _LeftWindowLayout : _RightWindowLayout;
                                        currentLayout.ForceDrop(ObjectToMoveWindow.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId - 1);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case ShortcutActions.UpdateOptionValue:
                    {
                        //Trace updateOption = _Trace1.GetComponent<Window>().hasFocus ? _Trace1 : _Trace2.GetComponent<Window>().hasFocus ? _Trace2 : null;
                        int updateOptionID = _Trace1.GetComponent<Window>().hasFocus ? 0 : _Trace2.GetComponent<Window>().hasFocus ? 1 : -1;
                        if (updateOptionID > -1)
                        {
                            TraceOption option = TracesService.GetOptionsFor(m_PatientSession, updateOptionID);
                            int index = option.ElectrodeID;
                            //int index = updateOption.TraceEeg.ElectrodeID;
                            if (message.Parameter == ShortcutActionsParameters.Up)
                            {
                                index += 1;
                                //updateOption.UpdateElectrodeById(index);
                                option.ElectrodeID = index;
                            }
                            else if (message.Parameter == ShortcutActionsParameters.Down)
                            {
                                index -= 1;
                                //updateOption.UpdateElectrodeById(index);
                                option.ElectrodeID = index;
                            }
                            else if (message.Parameter == ShortcutActionsParameters.Left)
                            {
                                ForceUpdateTraceMessage forceUpdateMessage = new ForceUpdateTraceMessage
                                {
                                    TraceID = updateOptionID,
                                    Gain = option.Gain,
                                    Offset = option.Offset,
                                    ShowGrid = option.IsGridOn,
                                    Period = option.WindowInSeconds,
                                    Color = option.Color,
                                    FileNextID = -1
                                };
                                Messenger.Default.Send(forceUpdateMessage, MessageContext.ForceUpdateTraceMessage);
                            }
                            else if (message.Parameter == ShortcutActionsParameters.Right)
                            {
                                ForceUpdateTraceMessage forceUpdateMessage = new ForceUpdateTraceMessage
                                {
                                    TraceID = updateOptionID,
                                    Gain = option.Gain,
                                    Offset = option.Offset,
                                    ShowGrid = option.IsGridOn,
                                    Period = option.WindowInSeconds,
                                    Color = option.Color,
                                    FileNextID = 1
                                };
                                Messenger.Default.Send(forceUpdateMessage, MessageContext.ForceUpdateTraceMessage);
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void LoadLayout(string path)
    {
        IWorkspaceContext workspaceContext = WorkspaceFactory.GetWorkspaceContext(path);
        SetBrainLayoutParameters(workspaceContext.Workspace.BrainParameters);
        SetTraceLayoutParameters(_Trace1, workspaceContext.Workspace.Trace1);
        SetTraceLayoutParameters(_Trace2, workspaceContext.Workspace.Trace2);
    }

    private void SetBrainLayoutParameters(BrainParameters brainParameters)
    {
        _BrainWarden.IsMaxed = brainParameters.IsMaxed;
    }

    private void SetTraceLayoutParameters(Trace trace, TraceParameters traceParameters)
    {
        //Update Option UI Display (without triggering events)
        ForceUpdateTraceMessage message = new ForceUpdateTraceMessage
        {
            TraceID = trace.TraceId,
            Gain = traceParameters.Gain,
            Offset = traceParameters.Offset,
            ShowGrid = traceParameters.ShowGrid,
            Period = traceParameters.Window,
            Color = traceParameters.Color,
        };
        Messenger.Default.Send(message, MessageContext.ForceUpdateTraceMessage);

        //Update 3D Module with the data
        TraceOption opt = TracesService.GetOptionsFor(m_PatientSession, trace.TraceId);
        opt.Gain = traceParameters.Gain;
        opt.Offset = traceParameters.Offset;
        opt.IsGridOn = traceParameters.ShowGrid;
        opt.WindowInSeconds = traceParameters.Window;
        opt.Color = traceParameters.Color;
        opt.LineWidth = (int)traceParameters.Width;

        //Try to load the different positions of the traces.
        // Guard the lookup (Find returns null on a missing/blank saved parent name, which used to
        // NRE), and route by *which* layout the saved parent actually is - the old ternary found
        // a layout then ignored it, degenerating to "found anything -> left, else right".
        GameObject parentObject = string.IsNullOrEmpty(traceParameters.Parent) ? null : GameObject.Find(traceParameters.Parent);
        WindowLayout layout = parentObject != null ? parentObject.GetComponent<WindowLayout>() : null;
        WindowLayout layouthandle = (layout == _LeftWindowLayout) ? _LeftWindowLayout : _RightWindowLayout;
        layouthandle.ForceDrop(trace.gameObject, traceParameters.GridLayout, traceParameters.Id);
    }

    private void SaveLayout(string path)
    {
        BrainParameters brain = GetBrainLayoutParameters();
        TraceParameters trace1 = GetTraceLayoutParameters(_Trace1);
        TraceParameters trace2 = GetTraceLayoutParameters(_Trace2);
        Workspace workspace = new Workspace(brain, trace1, trace2);
        WorkspaceFactory.SaveWorkspace(path, workspace);
    }

    private BrainParameters GetBrainLayoutParameters()
    {
        return new BrainParameters(_BrainWarden.IsMaxed);
    }

    private TraceParameters GetTraceLayoutParameters(Trace trace)
    {
        Window win = trace.GetComponent<Window>();
        TraceOption opt = TracesService.GetOptionsFor(m_PatientSession, trace.TraceId);
        TraceParameters param = new TraceParameters
        {
            Gain = opt.Gain,
            Offset = opt.Offset,
            ShowGrid = opt.IsGridOn,
            Window = opt.WindowInSeconds,
            Color = opt.Color,
            Width = opt.LineWidth,
            Parent = win != null ? win.transform.parent.name : null,
            Id = win != null ? win.windowId : -1,
            GridLayout = win != null ? win.GridLayout : GridLayout.TwoBy2
        };
        return param;
    }
}
