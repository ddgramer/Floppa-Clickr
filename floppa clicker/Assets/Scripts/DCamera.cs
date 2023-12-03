using UnityEngine;

namespace camera
{
    public class DCamera : MonoBehaviour
    {
        private GameObject player;

        private Vector3 offset;
        public Vector3 defaultOffset;

        public float shiftYSubtractor;
        public float shiftingSpeed;

        public float rotationX;
        public float rotationY;
        public float sens;

        // is called while the instance is loaded
        private void Awake()
        {
            player = GameObject.FindWithTag("Player");// finds a object tagged as player and sets the variable player as it

            offset = defaultOffset;
        }

        // called every frame
        private void Update()
        {
            Rotator();
        }

        // called after the other update methods
        private void LateUpdate()
        {
            Positioner();
        }

        private void Rotator()
        {
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

        // positions the camera and changes offset when needeed
        private void Positioner()
        {
            // if the player is pressing shift move the offset down acording to shiftYSubtractor else move towards the default
            if (Input.GetKey(KeyCode.LeftShift))
            {
                offset = Vector3.MoveTowards(offset, new Vector3(defaultOffset.x, defaultOffset.y - shiftYSubtractor, defaultOffset.z), shiftingSpeed * Time.deltaTime);
            }
            else
            {
                offset = Vector3.MoveTowards(offset, defaultOffset, shiftingSpeed * Time.deltaTime);
            }

            // set the position to the player's poition plus offset
            transform.position = player.transform.position + offset;
        }
    }
}