using Assets.Scripts.Data.Factory;
using UnityEngine;
using UnityEngine.UI;

public class WorkspaceManager : MonoBehaviour
{
    [SerializeField] BrainWarden _BrainWarden = null;
    [SerializeField] Trace _Trace1 = null;
    [SerializeField] Trace _Trace2 = null;

    private void Awake()
    {
        Messenger.Default.Register<UiToLayoutsMessage>(this, OnUiToLayoutsMessage, MessageContext.UiToLayouts);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToLayouts);
    }

    private void OnUiToLayoutsMessage(UiToLayoutsMessage message)
    {
        switch (message.TaskToExecute)
        {
            case 0:
                {
                    UnityEngine.Debug.Log("Should Load a layout from : " + message.Path);
                    LoadLayout(message.Path);
                    break;
                }
            case 1:
                {
                    UnityEngine.Debug.Log("Should save layout at : " + message.Path);
                    SaveLayout(message.Path);
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

    //At this point, placement of the windows doesn't work when forced
    //Need to improve that
    private void SetTraceLayoutParameters(Trace trace, TraceParameters traceParameters)
    {
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

        trace.TraceEeg.Gain = traceParameters.Gain;
        trace.TraceEeg.UpdateOffset(traceParameters.Offset);
        trace.GraphGrid.IsOn = traceParameters.ShowGrid;
        trace.TraceEeg.PeriodInSeconds = traceParameters.Window;
        trace.TraceEeg.Color = traceParameters.Color;
        trace.TraceEeg.LineWidth = traceParameters.Width;
        //Window win = trace.GetComponent<Window>();
        //if (win != null)
        //{
        //    win.OnBeginDrag();
        //    GameObject Parent = GameObject.Find(traceParameters.Parent);
        //    Parent.GetComponent<windowLayout>().OnDrop(null);
        //    if (Parent != null)
        //    {
        //        win.transform.SetParent(Parent.transform);
        //        win.gameObject.GetComponent<RectTransform>().sizeDelta = traceParameters.SizeDelta;
        //        win.gameObject.GetComponent<RectTransform>().localPosition = traceParameters.Position;
        //        win.windowId = traceParameters.Id;
        //    }
        //}
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
        TraceParameters param = new TraceParameters
        {
            Gain = trace.TraceEeg.Gain,
            Offset = trace.TraceEeg.Offset,
            ShowGrid = trace.GraphGrid.IsOn,
            Window = trace.TraceEeg.PeriodInSeconds,
            Color = trace.TraceEeg.Color,
            Width = trace.TraceEeg.LineWidth,
            //Position = win != null ? win.gameObject.GetComponent<RectTransform>().localPosition : new Vector3(0, 0, 0),
            //SizeDelta = win != null ? win.gameObject.GetComponent<RectTransform>().sizeDelta : new Vector2(0, 0),
            //Parent = win != null ? win.transform.parent.name : null,
            //Id = win != null ? win.windowId : -1
        };
        return param;
    }
}