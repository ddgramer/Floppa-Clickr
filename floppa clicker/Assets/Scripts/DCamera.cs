using System.Collections;
using UnityEngine;

public class DCamera : MonoBehaviour
{
    private GameObject player;
    private GameObject cameraCenter;
    private GameObject center;

    public Vector3[] offsets;
    public Vector3[] offsetsExtraRotation;
    public bool[] offsetsIsLateUpdate;
    public bool[] offsetsIsSmoothed;
    public bool[] offsetUseCameraCenter;
    public float moveSpeed;
    private bool offsetChanged;

    private Vector3 offset;
    private Vector3 relativeOffset;
    private Vector3 projectedCamPos;
    private Quaternion projectedCamRotation;

    public int offsetNow;

    public float shiftYSubtractor;
    public float shiftingSpeed;
    public float rotationX;
    public float rotationY;
    public float sens;

    // is called while the instance is loaded
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");// finds a object tagged as player and sets the variable player as it
        cameraCenter = GameObject.Find($"{player.name}/CameraCenter");// finds a child object of the player called CameraCenter and sets the variable cameraCenter as it
        offset = offsets[0];// set the offset the default
    }

    // called every frame
    private void Update()
    {
        Offseter();
        Rotator();
        if (!offsetsIsLateUpdate[offsetNow])
        {
            Positioner();
        }
    }

    private void FixedUpdate()
    {
    }

    // called after the other update methods
    private void LateUpdate()
    {
        if (offsetsIsLateUpdate[offsetNow])
        {
            Positioner();
        }
    }

    private void Rotator()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.Locked;// lock mouse cursor
            Cursor.visible = false;// hide mouse cursor
        }

        // passes if the right mouse key is pressed
        if (Input.GetKey(KeyCode.Mouse1) || offsetChanged)
        {
            offsetChanged = false;

            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;// the change in Mouse X every second
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;// the change in Mouse Y every second

            rotationX -= mouseY;// adds mouseY to rotationX
            rotationY += mouseX;// subtracts mouseX from rotation

            // if rotationX is bigger than 360(a full rotation) or smaller than negative 360 subtract or add 360 untill it isn't
            if (rotationX > 360)
            {
                rotationX -= 360;
            }
            else if (rotationX < -360)
            {
                rotationX += 360;
            }

            // if rotationY is bigger than 360(a full rotation) or smaller than negative 360 subtract or add 360 untill it isn't
            if (rotationY > 360)
            {
                rotationY -= 360;
            }
            else if (rotationY < -360)
            {
                rotationY += 360;
            }

            projectedCamRotation = Quaternion.Euler(rotationX + offsetsExtraRotation[offsetNow].x, rotationY + offsetsExtraRotation[offsetNow].y, 0 + offsetsExtraRotation[offsetNow].z);// sets X and Y in the camera's rotation to the rotation variables and Z to 0
            cameraCenter.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);// sets X and Y in the camera's rotation to the rotation variables and Z to 0
            player.transform.rotation = Quaternion.Euler(0, rotationY, 0);// sets Y in the player's rotation to the rotation variables and X and Z to 0
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.None;// unlock mouse cursor
            Cursor.visible = true;// turn mouse cursor visible
        }
    }

    private void Offseter()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            offsetNow += 1;
            if (offsetNow >= offsets.Length)
            {
                offsetNow = 0;
            }
            offsetChanged = true;
        }

        if (offsetUseCameraCenter[offsetNow])
        {
            center = cameraCenter;
        }
        else
        {
            center = player;
        }

        if (offsetChanged)
        {
            offset = offsets[offsetNow];
        }

        relativeOffset = center.transform.TransformDirection(offset);
    }

    private void Positioner()
    {
        projectedCamPos = player.transform.position + relativeOffset;

        if (offsetsIsSmoothed[offsetNow])
        {
            transform.position = Vector3.Lerp(transform.position, projectedCamPos, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, projectedCamRotation, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = projectedCamPos;// sets the mouse position to the player's plus offset
            transform.rotation = projectedCamRotation;
        }
    }
}