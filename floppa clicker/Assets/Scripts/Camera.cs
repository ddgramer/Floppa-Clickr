using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Camera : MonoBehaviour
{
    private GameObject player;
    private Vector3 offset;
    public Vector3 defaultOffset;
    public Vector3 shiftOffset;

    public float shiftingSpeed;
    public float rotationX;
    public float rotationY;
    public float sens;

    // is called while the instance is loaded
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");// finds a object tagged as player and sets the variable player as it
        offset = defaultOffset;// set the offset the default
    }

    // called every frame
    private void Update()
    {
        Rotate();
    }

    // called after the other update methods
    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;// sets the mouse position to the player's plus offset
    }

    private void Rotate()
    {
        // if the player is pressing shift the camera's position offset moves at shiftingSpeed * time until it is equals to shiftOffset else until it is equals to defaultOffset
        if (Input.GetKey(KeyCode.LeftShift))
        {
            offset = Vector3.MoveTowards(offset, shiftOffset, shiftingSpeed * Time.deltaTime);
        }
        else 
        {
            offset = Vector3.MoveTowards(offset, defaultOffset, shiftingSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.Locked;// lock mouse cursor
            Cursor.visible = false;// hide mouse cursor
        }

        // passes if the right mouse key is pressed
        if (Input.GetKey(KeyCode.Mouse1))
        {
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

            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);// sets X and Y in the camera's rotation to the rotation variables and Z to 0
            player.transform.rotation = Quaternion.Euler(0, rotationY, 0);// sets Y in the player's rotation to the rotation variables and X and Z to 0
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.None;// unlock mouse cursor
            Cursor.visible = true;// turn mouse cursor visible
        }
    }
}