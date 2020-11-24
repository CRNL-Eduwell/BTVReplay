using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
    //=== Main Keys
    private bool IsControlPressed
    {
#if UNITY_STANDALONE_OSX
        get
        {
            return Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        }
#else
        get
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }
#endif
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
    //=== Brain
    private bool MoveBrainActionPerformed
    {
        get
        {
            return !IsControlPressed && !IsShiftPressed && ((IsArrowKeyPressed && m_Timer >= Time.deltaTime) || IsArrowKeyDown);
        }
    }
    //=== Traces
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
    private bool ChangeOptionValueUp_WindowsActionPerformed
    {
        get
        {
            return !IsControlPressed && !IsShiftPressed && ((Input.GetKey(KeyCode.UpArrow) && m_Timer >= DELAY) || Input.GetKeyDown(KeyCode.UpArrow));
        }
    }
    private bool ChangeOptionValueDown_WindowsActionPerformed
    {
        get
        {
            return !IsControlPressed && !IsShiftPressed && ((Input.GetKey(KeyCode.DownArrow) && m_Timer >= DELAY) || Input.GetKeyDown(KeyCode.DownArrow));
        }
    }
    //=== Trace Displayer 
    private bool FocusTracesDisplayerActionperformed
    {
        get
        {
            return !IsControlPressed && IsShiftPressed && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
        }
    }
    private bool ChangeSelectedOptionLeft_TracesDisplayerActionperformed
    {
        get
        {
            return !IsControlPressed && IsShiftPressed && Input.GetKeyDown(KeyCode.LeftArrow);
        }
    }
    private bool ChangeSelectedOptionRight_TracesDisplayerActionperformed
    {
        get
        {
            return !IsControlPressed && IsShiftPressed && Input.GetKeyDown(KeyCode.RightArrow);
        }
    }
    private bool ChangeOptionValueUp_TracesDisplayerActionperformed
    {
        get
        {
            return !IsControlPressed && IsShiftPressed && ((Input.GetKey(KeyCode.UpArrow) && m_Timer >= DELAY) || Input.GetKeyDown(KeyCode.UpArrow));
        }
    }
    private bool ChangeOptionValueDown_TracesDisplayerActionperformed
    {
        get
        {
            return !IsControlPressed && IsShiftPressed && ((Input.GetKey(KeyCode.DownArrow) && m_Timer >= DELAY) || Input.GetKeyDown(KeyCode.DownArrow));
        }
    }

    private const float DELAY = 0.2f;
    private float m_Timer = 0.0f;
    private ShortcutMessage m_Message = null;

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
        if (MoveBrainActionPerformed)
        {
            UnityEngine.Debug.Log("Move Brain Keyboard Shortcut");
            m_Timer = 0;
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Move,
                Parameter = ListenForKeyDirection(),
                RecipientType = typeof(BrainCamera)
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (MoveWindow1ActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Move Window 1 Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Move,
                Parameter = ListenForKeyDirection(),
                RecipientType = typeof(Trace),
                RecipientIndex = 1
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (MoveWindow2ActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Move Window 2 Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Move,
                Parameter = ListenForKeyDirection(),
                RecipientType = typeof(Trace),
                RecipientIndex = 2
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (FocusWindow1ActionPerformed)
        {
            UnityEngine.Debug.Log("Focus Window 1 Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Focus,
                Parameter = ShortcutActionsParameters.In,
                RecipientType = typeof(Trace),
                RecipientIndex = 1
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (FocusWindow2ActionPerformed)
        {
            UnityEngine.Debug.Log("Focus Window 2 Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Focus,
                Parameter = ShortcutActionsParameters.In,
                RecipientType = typeof(Trace),
                RecipientIndex = 2
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeOptionValueUp_WindowsActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Update Window Option Up Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.UpdateOptionValue,
                Parameter = ShortcutActionsParameters.Up,
                RecipientType = typeof(Trace),
                RecipientIndex = -1 //we want to send to all window, so no indication
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeOptionValueDown_WindowsActionPerformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Update Window Option Down Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.UpdateOptionValue,
                Parameter = ShortcutActionsParameters.Down,
                RecipientType = typeof(Trace),
                RecipientIndex = -1 //we want to send to all window, so no indication
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (FocusTracesDisplayerActionperformed)
        {
            UnityEngine.Debug.Log("Focus Trace Displayer Shortcut");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.Focus,
                Parameter = ShortcutActionsParameters.In,
                RecipientType = typeof(TracesDisplayer),
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeSelectedOptionLeft_TracesDisplayerActionperformed)
        {
            UnityEngine.Debug.Log("Change Trace Displayer Selected Option Shortcut (left)");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.ChangeOption,
                Parameter = ShortcutActionsParameters.Left,
                RecipientType = typeof(TracesDisplayer)
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeSelectedOptionRight_TracesDisplayerActionperformed)
        {
            UnityEngine.Debug.Log("Change Trace Displayer Selected Option Shortcut (right)");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.ChangeOption,
                Parameter = ShortcutActionsParameters.Right,
                RecipientType = typeof(TracesDisplayer)
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeOptionValueUp_TracesDisplayerActionperformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Update Trace Displayer Option value Shortcut (up)");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.UpdateOptionValue,
                Parameter = ShortcutActionsParameters.Up,
                RecipientType = typeof(TracesDisplayer)
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
        else if (ChangeOptionValueDown_TracesDisplayerActionperformed)
        {
            m_Timer = 0;
            UnityEngine.Debug.Log("Update Trace Displayer Option value Shortcut (down)");
            m_Message = new ShortcutMessage
            {
                Action = ShortcutActions.UpdateOptionValue,
                Parameter = ShortcutActionsParameters.Down,
                RecipientType = typeof(TracesDisplayer)
            };
            Messenger.Default.Send(m_Message, MessageContext.ShortcutMessage);
        }
    }

    private ShortcutActionsParameters ListenForKeyDirection()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) return ShortcutActionsParameters.Left;
        else if (Input.GetKey(KeyCode.RightArrow)) return ShortcutActionsParameters.Right;
        else if (Input.GetKey(KeyCode.UpArrow)) return ShortcutActionsParameters.Up;
        else if (Input.GetKey(KeyCode.DownArrow)) return ShortcutActionsParameters.Down;
        else return ShortcutActionsParameters.None;
    }
}
