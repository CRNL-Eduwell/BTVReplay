using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
    [SerializeField] WindowLayout _RightWindowLayout = null;

    private bool IsControlPressed
    {
        get
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
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

    private bool MoveWindow1ToPlace1ActionPerformed
    {
        get
        {
            return IsControlPressed && !IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Ampersand));
        }
    }
    private bool MoveWindow1ToPlace2ActionPerformed
    {
        get
        {
            return IsControlPressed && !IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.None + 161));
        }
    }
    private bool MoveWindow1ToPlace3ActionPerformed
    {
        get
        {
            return IsControlPressed && !IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.DoubleQuote));
        }
    }
    private bool MoveWindow2ToPlace1ActionPerformed
    {
        get
        {
            return IsControlPressed && IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Ampersand));
        }
    }
    private bool MoveWindow2ToPlace2ActionPerformed
    {
        get
        {
            return IsControlPressed && IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.None + 161));
        }
    }
    private bool MoveWindow2ToPlace3ActionPerformed
    {
        get
        {
            return IsControlPressed && IsShiftPressed && (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.DoubleQuote));
        }
    }

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
        if (MoveWindow1ToPlace1ActionPerformed)
        {
            MoveWindow(1, 0);
        }
        else if (MoveWindow1ToPlace2ActionPerformed)
        {
            MoveWindow(1, 1);
        }
        else if (MoveWindow1ToPlace3ActionPerformed)
        {
            MoveWindow(1, 2);
        }
        else if (MoveWindow2ToPlace1ActionPerformed)
        {
            MoveWindow(2, 0);
        }
        else if (MoveWindow2ToPlace2ActionPerformed)
        {
            MoveWindow(2, 1);
        }
        else if (MoveWindow2ToPlace3ActionPerformed)
        {
            MoveWindow(2, 2);
        }
    }

    private void MoveWindow(int WindowIndex, int PositionIndex)
    {
        Trace ObjectToMove = WindowIndex == 1 ? ApplicationState.Module3D.Window1 : ApplicationState.Module3D.Window2;
        _RightWindowLayout.ForceDrop(ObjectToMove.gameObject, GridLayout.OneBy3, PositionIndex);
    }
}
