using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhyObjCamera : MonoBehaviour
{
    GameObject Object3DHandle = null;
    Vector3 target, originalTarget;
    Material BaseMaterialTransparency = null;
    MeshRenderer[] brainRenderer = null;
    //=====================
    public float distance = 250.0f;
    public float speed = 5.0f;
    //=====================
    public float minDistance = 50.0f;
    public float maxDistance = 750.0f;
    //=====================
    public float zoomSpeed = 2;

    public void initCameraPosition()
    {
        Object3DHandle = GameObject.Find("GameObject");
        BaseMaterialTransparency = Resources.Load("Materials/MaterialTransparencyStencil", typeof(Material)) as Material;
        brainRenderer = Object3DHandle.transform.GetComponentsInChildren<MeshRenderer>();
        brainRenderer[0].GetComponent<Renderer>().material = Instantiate(BaseMaterialTransparency);
        brainRenderer[1].GetComponent<Renderer>().material = Instantiate(BaseMaterialTransparency);

        //== Check parameters integrity
        if (distance < minDistance)
            distance = minDistance;

        if (distance > maxDistance)
            distance = maxDistance;

        Object3DHandle.transform.position += new Vector3(-1000, 0, 0);        // degage le cerveau du canvas et est uniquement rendu par la cam 
        Object3DHandle.transform.Rotate(new Vector3(270, 0, 0));
        target = Object3DHandle.transform.position;
        originalTarget = target;

        transform.position = target - (transform.forward * distance);
    }

    public void initElecCameraPosition()
    {
        distance = 150;

        //== Check parameters integrity
        if (distance < minDistance)
            distance = minDistance;

        if (distance > maxDistance)
            distance = maxDistance;

        Object3DHandle.transform.position += new Vector3(-1000, 0, 0);        // degage le cerveau du canvas et est uniquement rendu par la cam 
        //Object3DHandle.transform.Rotate(new Vector3(270, 0, 0));
        target = Object3DHandle.transform.position;
        originalTarget = target;

        transform.position = target - (transform.forward * distance);
    }

    void Update ()
    {
        if (Input.GetMouseButton(1)) // Mouse Right click
        {
            rotateBrainByMouse(); 
        }

        if (Input.GetMouseButton(2)) // Mouse wheel click
        {
            translateBrainByMouse();
        }

        keyboardAction();
    }

    /// <summary>
    /// Called multiple times per frame in response to GUI events. The Layout and Repaint events are processed first, followed by a Layout and keyboard/mouse event for each input event.
    /// </summary>
    protected void OnGUI()
    {
        // zoom scroll mouse
        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (scrollDelta.y != 0)
        {
            if (scrollDelta.y < 0)
                moveBackward(zoomSpeed);
            else
                moveForward(zoomSpeed);
        }
    }

    protected void translateBrainByMouse()
    {
        float nx = 0;
        float ny = 0;

        nx = Input.GetAxis("Mouse X");
        ny = Input.GetAxis("Mouse Y");

        // check horizontal right click mouse drag movement
        if (nx != 0)
            if (nx < 0)
                horizontalStrafe(true, nx * speed);
            else
                horizontalStrafe(false, -nx * speed);


        //check vertical right click mouse drag movement
        if (ny != 0)
            if (ny < 0)
                verticalStrafe(true, -ny * speed);
            else
                verticalStrafe(false, ny * speed);
    }

    protected void rotateBrainByMouse()
    {
        float nx = 0;
        float ny = 0;

        nx = Input.GetAxis("Mouse X");
        ny = Input.GetAxis("Mouse Y");

        // check horizontal right click mouse drag movement
        if (nx != 0)
            if (nx < 0)
                moveLeft(-nx * speed);
            else
                moveRight(nx * speed);

        // check vertical right click mouse drag movement
        if (ny != 0)
            if (ny < 0)
                moveUp((ny) * speed);
            else
                moveDown((-ny) * speed);
    }

    protected void keyboardAction()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            moveLeft(speed);
        if (Input.GetKey(KeyCode.RightArrow))
            moveRight(speed);
        if (Input.GetKey(KeyCode.UpArrow))
            moveUp(speed);
        if (Input.GetKey(KeyCode.DownArrow))
            moveDown(speed);
        if (Input.GetKey(KeyCode.Z))
            moveForward(zoomSpeed);
        if (Input.GetKey(KeyCode.S))
            moveBackward(zoomSpeed);
    }

    protected void moveRight(float amount)
    {
        Vector3 vecTargetPos_EyePos = transform.position - target;
        Quaternion rotation = Quaternion.AngleAxis(amount, transform.up);
        transform.position = rotation * vecTargetPos_EyePos + target;
        transform.LookAt(target, transform.up);
    }

    protected void moveLeft(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - target;
        Quaternion rotation = Quaternion.AngleAxis(-amout, transform.up);
        transform.position = rotation * vecTargetPos_EyePos + target;
        transform.LookAt(target, transform.up);
    }

    protected void moveUp(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - target;
        Quaternion rotation = Quaternion.AngleAxis(-amout, transform.right);
        transform.position = rotation * vecTargetPos_EyePos + target;
        transform.LookAt(target, Vector3.Cross(target - transform.position, transform.right));
    }

    protected void moveDown(float amout)
    {
        Vector3 vecTargetPos_EyePos = transform.position - target;
        Quaternion rotation = Quaternion.AngleAxis(amout, transform.right);
        transform.position = rotation * vecTargetPos_EyePos + target;
        transform.LookAt(target, Vector3.Cross(target - transform.position, transform.right));
    }

    protected void moveForward(float amount)
    {
        float length = Vector3.Distance(transform.position, target);

        if (length - amount > minDistance)
        {
            transform.position -= transform.forward * (-amount);
        }
    }

    protected void moveBackward(float amount)
    {
        float length = Vector3.Distance(transform.position, target);

        if (length + amount < maxDistance)
        {
            transform.position += transform.forward * (-amount);
        }
    }

    protected void horizontalStrafe(bool left, float amount)
    {
        Vector3 strafe;
        if (left)
            strafe = -transform.right * amount;
        else
            strafe = transform.right * amount;

        transform.position = transform.position + strafe;
        target = target + strafe;
    }

    protected void verticalStrafe(bool up, float amount)
    {
        Vector3 strafe;
        if (up)
            strafe = transform.up * amount;
        else
            strafe = -transform.up * amount;

        transform.position = transform.position + strafe;
        target = target + strafe;
    }
}
