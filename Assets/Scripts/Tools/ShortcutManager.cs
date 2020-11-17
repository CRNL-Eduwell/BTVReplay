using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
    [SerializeField] WindowLayout _LeftWindowLayout = null;
    [SerializeField] WindowLayout _RightWindowLayout = null;

    private bool IsControlPressed
    {
        get
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        }
    }
    private bool IsAltPressed
    {
        get
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }
    }
    private bool IsShiftPressed
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
    private bool IsArrowKeyPressed
    {
        get
        {
            return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
        }
    }
    private bool IsArrowKeyDown
    {
        get
        {
            return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
        }
    }
    private bool MoveWindow1ActionPerformed
    {
        get
        {
            return IsControlPressed && !IsShiftPressed && ((IsArrowKeyPressed && m_Timer >= DELAY) || IsArrowKeyDown);
        }
    }
    private bool MoveWindow2ActionPerformed
    {
        get
        {
            return IsControlPressed && IsShiftPressed && ((IsArrowKeyPressed && m_Timer >= DELAY) || IsArrowKeyDown);
        }
    }
    private bool FocusWindow1ActionPerformed
    {
        get
        {
            return IsControlPressed && !IsShiftPressed && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
        }
    }
    private bool FocusWindow2ActionPerformed
    {
        get
        {
            return IsControlPressed && IsShiftPressed && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
        }
    }

    private const float DELAY = 0.2f;
    private float m_Timer = 0.0f;

    //Debug in case of not finding the value for shortcuts
    //void OnGUI()
    //{
    //    Event e = Event.current;
    //    if (e.isKey)
    //    {
    //        Debug.Log("Detected key code: " + e.keyCode);
    //    }
    //}

    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (MoveWindow1ActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Move Window 1 Shortcut");
            MoveWindow(1);
        }
        else if (MoveWindow2ActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Move Window 2 Shortcut");
            MoveWindow(2);
        }
        else if (FocusWindow1ActionPerformed)
        {
            UnityEngine.Debug.Log("Focus Window 1 Shortcut");
            FocusWindow(ApplicationState.Module3D.Window1);
        }
        else if (FocusWindow2ActionPerformed)
        {
            UnityEngine.Debug.Log("Focus Window 2 Shortcut");
            FocusWindow(ApplicationState.Module3D.Window2);
        }
    }

    private void MoveWindow(int WindowIndex)
    {
        Trace ObjectToMove = WindowIndex == 1 ? ApplicationState.Module3D.Window1 : ApplicationState.Module3D.Window2;
        Window ObjectToMoveWindow = ObjectToMove.GetComponent<Window>();
        Transform parent = ObjectToMove.transform.parent;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (parent != _LeftWindowLayout.transform.parent)
            {
                _LeftWindowLayout.ForceDrop(ObjectToMove.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId);
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (parent != _RightWindowLayout.transform.parent)
            {
                _RightWindowLayout.ForceDrop(ObjectToMove.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId);
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (ObjectToMoveWindow.windowId + 1 < 3)
            {
                WindowLayout currentLayout = parent == _LeftWindowLayout.transform ? _LeftWindowLayout : _RightWindowLayout;
                currentLayout.ForceDrop(ObjectToMoveWindow.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId + 1);
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (ObjectToMoveWindow.windowId - 1 >= 0)
            {
                WindowLayout currentLayout = parent == _LeftWindowLayout.transform ? _LeftWindowLayout : _RightWindowLayout;
                currentLayout.ForceDrop(ObjectToMoveWindow.gameObject, GridLayout.OneBy3, ObjectToMoveWindow.windowId - 1);
            }
        }
    }

    private void FocusWindow(Trace window)
    {
        int status = window.GetComponent<Window>().hasFocus ? 1 : 3;
        window.UpdateWindowState(status);
    }
}
