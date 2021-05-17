using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainCamera : MonoBehaviour
{
    #region Public Members
    public bool IsMouseOver
    {
        get;
        set;
    }
    public float distance = 250.0f;
    public float speed = 5.0f;
    public float minDistance = 50.0f;
    public float maxDistance = 750.0f;
    public float zoomSpeed = 2;
    #endregion

    #region Private Members
    [SerializeField]
    private GameObject m_BrainHandle = null;
    private Vector3 m_Target, m_OriginalTarget;
    #endregion

    private void Awake()
    {
        Messenger.Default.Register<ShortcutMessage>(this, OnShortcutMessage, MessageContext.ShortcutMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.ShortcutMessage);
    }

    public void InitCameraPosition()
    {
        //== Check parameters integrity
        if (distance < minDistance)
            distance = minDistance;

        if (distance > maxDistance)
            distance = maxDistance;

        m_BrainHandle.transform.position += new Vector3(-10000, 0, 0);        // degage le cerveau du canvas et est uniquement rendu par la cam 
        m_BrainHandle.transform.Rotate(new Vector3(270, 0, 0));
        m_Target = m_BrainHandle.transform.position;
        m_OriginalTarget = m_Target;

        transform.position = m_Target - (transform.forward * distance);
    }

    private void OnShortcutMessage(ShortcutMessage message)
    {
        if (GetType() == message.RecipientType)
        {
            switch (message.Action)
            {
                case ShortcutActions.Move:
                    {
                        switch (message.Parameter)
                        {
                            case ShortcutActionsParameters.Left:
                                {
                                    MoveLeft(speed);
                                }
                                break;
                            case ShortcutActionsParameters.Right:
                                {
                                    MoveRight(speed);
                                }
                                break;
                            case ShortcutActionsParameters.Up:
                                {
                                    MoveUp(speed);
                                }
                                break;
                            case ShortcutActionsParameters.Down:
                                {
                                    MoveDown(speed);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Called multiple times per frame in response to GUI events. The Layout and Repaint events are processed first, followed by a Layout and keyboard/mouse event for each input event.
    /// </summary>
    protected void OnGUI()
    {
        if (IsMouseOver)
        {
            // zoom scroll mouse
            Vector2 scrollDelta = Input.mouseScrollDelta;
            if (scrollDelta.y != 0)
            {
                if (scrollDelta.y < 0)
                    MoveBackward(zoomSpeed);
                else
                    MoveForward(zoomSpeed);
            }

            if (Input.GetMouseButton(1)) // Mouse Right click
            {
                RotateBrainByMouse();
            }

            if (Input.GetMouseButton(2)) // Mouse wheel click
            {
                TranslateBrainByMouse();
            }

            IsMouseOver = false;
        }
    }

    protected void TranslateBrainByMouse()
    {
        float nx = 0;
        float ny = 0;

        nx = Input.GetAxis("Mouse X");
        ny = Input.GetAxis("Mouse Y");

        // check horizontal right click mouse drag movement
        if (nx != 0)
            if (nx < 0)
                HorizontalStrafe(true, nx * speed);
            else
                HorizontalStrafe(false, -nx * speed);


        //check vertical right click mouse drag movement
        if (ny != 0)
            if (ny < 0)
                VerticalStrafe(true, -ny * speed);
            else
                VerticalStrafe(false, ny * speed);
    }

    protected void RotateBrainByMouse()
    {
        float nx = 0;
        float ny = 0;

        nx = Input.GetAxis("Mouse X");
        ny = Input.GetAxis("Mouse Y");

        // check horizontal right click mouse drag movement
        if (nx != 0)
            if (nx < 0)
                MoveLeft(-nx * speed);
            else
                MoveRight(nx * speed);

        // check vertical right click mouse drag movement
        if (ny != 0)
            if (ny < 0)
                MoveUp((ny) * speed);
            else
                MoveDown((-ny) * speed);
    }

    protected void MoveRight(float amount)
    {
        Vector3 vecTargetPos_EyePos = transform.position - m_Target;
        Quaternion rotation = Quaternion.AngleAxis(amount, transform.up);
        transform.position = rotation * vecTargetPos_EyePos + m_Target;
        transform.LookAt(m_Target, transform.up);
    }

    protected void MoveLeft(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - m_Target;
        Quaternion rotation = Quaternion.AngleAxis(-amout, transform.up);
        transform.position = rotation * vecTargetPos_EyePos + m_Target;
        transform.LookAt(m_Target, transform.up);
    }

    protected void MoveUp(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - m_Target;
        Quaternion rotation = Quaternion.AngleAxis(-amout, transform.right);
        transform.position = rotation * vecTargetPos_EyePos + m_Target;
        transform.LookAt(m_Target, Vector3.Cross(m_Target - transform.position, transform.right));
    }

    protected void MoveDown(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - m_Target;
        Quaternion rotation = Quaternion.AngleAxis(amout, transform.right);
        transform.position = rotation * vecTargetPos_EyePos + m_Target;
        transform.LookAt(m_Target, Vector3.Cross(m_Target - transform.position, transform.right));
    }

    protected void MoveForward(float amount)
    {
        float length = Vector3.Distance(transform.position, m_Target);

        if (length - amount > minDistance)
        {
            transform.position -= transform.forward * (-amount);
        }
    }

    protected void MoveBackward(float amount)
    {
        float length = Vector3.Distance(transform.position, m_Target);

        if (length + amount < maxDistance)
        {
            transform.position += transform.forward * (-amount);
        }
    }

    protected void HorizontalStrafe(bool left, float amount)
    {
        Vector3 strafe;
        if (left)
            strafe = -transform.right * amount;
        else
            strafe = transform.right * amount;

        transform.position = transform.position + strafe;
        m_Target = m_Target + strafe;
    }

    protected void VerticalStrafe(bool up, float amount)
    {
        Vector3 strafe;
        if (up)
            strafe = transform.up * amount;
        else
            strafe = -transform.up * amount;

        transform.position = transform.position + strafe;
        m_Target = m_Target + strafe;
    }
}
