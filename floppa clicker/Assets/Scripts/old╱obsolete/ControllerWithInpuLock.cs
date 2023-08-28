using System;
using UnityEngine;

public class ControllerWithInputLock : MonoBehaviour
{
    #region Movement Variables

    // 2D movement variables
    [Header("2D movement variables")]
    public float force;

    public float shiftForce;
    private float moddedForce;

    public float extraAirBreak;
    public float maxSpeed;
    public float maxShiftSpeed;

    public float drag;
    public float floatingDrag;
    public float airDrag;

    private Vector3 inputAndY;

    // sneak variables
    [Header("sneak variables")]
    public float maxShiftDistance;

    private float shiftYPos;

    public int iterations;

    private bool hasShiftYPos;
    private bool isFloating;

    private RaycastHit nearHit;

    // jump variables
    [Header("jump variables")]
    public float jumpForce;

    private float halfPlayerHeight;

    public int maxJumps;
    public int jumped;

    // Others
    [Header("others")]
    private Rigidbody rigidBody;

    private MeshRenderer rendererr;

    #endregion Movement Variables

    // while the instance is being loaded
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();// gets the Rigidbody from the object the instance of the code is atached to
        rendererr = GetComponent<MeshRenderer>();// gets the MeshRenderer from the object the instance of the code is atached to
        halfPlayerHeight = rendererr.bounds.extents.y;// uses the renderer bounds to find half the player height
    }

    // once before every frame
    private void Update()
    {
        Jump();
        Drag();
    }

    // once every physics step
    private void FixedUpdate()
    {
        Shift();
        OnTheMove();
    }

    // 2D movement code
    private void OnTheMove()
    {
        float maxSpeedModded;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal") * .8f, 0f, Input.GetAxis("Vertical"));// Horizontal input, as X, Vertical input, as Z, and 0, as Y

        Vector3 worldRelativeInput = transform.TransformDirection(input);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeedModded = maxShiftSpeed;
            moddedForce = shiftForce;
        }
        else
        {
            moddedForce = force;
        }

        Vector3 processedInput = worldRelativeInput.normalized * moddedForce * Time.deltaTime;

        inputAndY = new Vector3(processedInput.x, rigidBody.velocity.y, processedInput.z);// processedInput's X and Z with the Rigidbody's Y velocity

        rigidBody.AddForce(inputAndY, ForceMode.Force);// add inputAndY amount of force to the player

        // if the player isn't pressing anything and is on ground max speed is 0 else max speed is the default
        if (input == Vector3.zero && GroundChecks().Item1)
        {
            maxSpeedModded = 0f;
        }
        else
        {
            maxSpeedModded = maxSpeed;
        }

        float speed = Vector3.Magnitude(rigidBody.velocity);// get current object speed

        Debug.Log(speed);

        //if the player's speed is higher than max speed pass
        if (speed > maxSpeedModded)
        {
            float horizontalBrakeSpeed = speed - maxSpeedModded;// calculate the horizontal speed decrease
            float verticalBrakeSpeed = speed - maxSpeed;// calculate the vertical speed decrease

            Vector3 normalisedVelocity = rigidBody.velocity.normalized;
            // calculate the brake Vector3 value
            Vector3 brakeVelocity = new Vector3(normalisedVelocity.x * horizontalBrakeSpeed, normalisedVelocity.y * verticalBrakeSpeed, normalisedVelocity.z * horizontalBrakeSpeed);

            if (!GroundChecks().Item1)
            {
                rigidBody.AddForce(-brakeVelocity * extraAirBreak);// apply opposing brake force multiplied by air break
            }
            else
            {
                rigidBody.AddForce(-brakeVelocity);// apply opposing brake force
            }
        }
    }

    // jump code
    public void Jump()
    {
        //uses GroundChecks to check if player is on the ground and sets jumped to zero if the player is
        if (GroundChecks().Item1)
        {
            jumped = 0;//
        }
        // if the player is floating set jumped to zero
        else if (isFloating)
        {
            jumped = 0;
        }

        // if the player jumped less times than
        if (jumped < maxJumps && Input.GetKeyDown(KeyCode.Space))
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);// sets rigid body y force to 0 float
            rigidBody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);// adds {jumpForce} amount of force instantly to the y axis e.g. upwards
            jumped++;// increments jump by 1
        }
    }

    private void Shift()
    {
        RaycastHit hit = new RaycastHit();
        nearHit.distance = 1000f;

        // if the player is pressing LeftShift GroundChecks and doesn't have shiftYPos set shiftYPos to the player's Y position and hasShiftYPos to true
        if (Input.GetKey(KeyCode.LeftShift) && GroundChecks().Item1 && !hasShiftYPos)
        {
            shiftYPos = GroundChecks().Item2 + halfPlayerHeight;
            hasShiftYPos = true;
        }
        else
        {
            shiftYPos = transform.position.y;
        }

        // if the player isn't on ground and hasShiftPos and is pressing shift disable gravity and sets isFloating to true
        if (!GroundChecks().Item1 && hasShiftYPos && Input.GetKey(KeyCode.LeftShift))
        {
            isFloating = true;
        }

        // if the player is on ground or pressed space or isn't pressing shift enable gravity set isfloating to false and  sets all movement blocking booleans to false
        if (GroundChecks().Item1 || Input.GetKeyDown(KeyCode.Space) || !Input.GetKey(KeyCode.LeftShift))
        {
            isFloating = false;
        }

        // if the player is presses space set hasShiftPos to false
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasShiftYPos = false;
        }

        // if shift pos has been locked and player isn't on ground pass
        if (hasShiftYPos && !GroundChecks().Item1)
        {
            // a for loop that runs iterations times
            for (int i = 1; i < iterations; i++)
            {
                // a spinning direction that spins 360 / iterations + 1 each loop iteration
                Quaternion dir = Quaternion.AngleAxis((360 / iterations + 1) * i, Vector3.up);

                // if a ray of dir direction hits pass
                if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - halfPlayerHeight, transform.position.z), dir * Vector3.forward))
                {
                    // sends the same raycast as in the if statement and sets hit
                    Physics.Raycast(new Vector3(transform.position.x, transform.position.y - halfPlayerHeight, transform.position.z), dir * Vector3.forward, out hit);

                    // if the distance hit hit is short than the nearHit distance and shorter than 2 times the max shift distance set nearHit to hit
                    if (hit.distance < nearHit.distance && hit.distance < maxShiftDistance * 2f)
                    {
                        nearHit = hit;
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && hasShiftYPos && !GroundChecks().Item1)
        {
            rigidBody.useGravity = false;

            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);// set the players Y velocity to 0
            transform.position = new Vector3(transform.position.x, shiftYPos, transform.position.z);// set the player's Y position to shiftYPos

            transform.position = new Vector3(Mathf.Clamp(transform.position.x, nearHit.point.x - maxShiftDistance, nearHit.point.x + maxShiftDistance), transform.position.y, Mathf.Clamp(transform.position.z, nearHit.point.z - maxShiftDistance, nearHit.point.z + maxShiftDistance));
        }
        else
        {
            rigidBody.useGravity = true;
        }
    }

    // applies drag to the player
    public void Drag()
    {
        //uses GroundChecks to check if player is on the ground
        if (GroundChecks().Item1)
        {
            rigidBody.drag = drag;// set drag to 1
            rigidBody.angularDrag = drag;// set angular drag to 1
        }
        else if (isFloating)
        {
            rigidBody.drag = floatingDrag;// set drag to 1
            rigidBody.angularDrag = floatingDrag;// set angular drag to 1
        }
        else
        {
            rigidBody.drag = airDrag;// set drag to .05
            rigidBody.angularDrag = airDrag;// set angular drag to .1
        }
    }

    // sends down a raycast and returns if the ray hit and the hit height
    private Tuple<bool, float> GroundChecks()
    {
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(transform.position, Vector3.down, out hit, halfPlayerHeight * 1.01f))
        {
            return Tuple.Create(true, hit.point.y);
        }
        else
        {
            return Tuple.Create(false, hit.point.y);
        }
    }
}