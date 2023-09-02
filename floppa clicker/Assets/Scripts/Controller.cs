using System;
using UnityEngine;

public class Controller : MonoBehaviour
{
    #region Movement Variables

    // 2D movement variables
    [Header("2D movement variables")]
    public float defaultForce;
    public float shiftForce;

    public float extraAirBreak;
    public float extraNoInputBreak;
    public float defaultMaxSpeed;
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
    public bool isFixed;
    public bool isLate;

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

    // once every frame
    private void Update()
    {
        Jump();
        Drag();
    }

    // once after the other update functions
    private void LateUpdate()
    {
        Shift();
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
        float maxSpeed; 
        float force;

        // get player's 2D inputs with unity's input system
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        // player input is relative to the player by default so convert it to be relative to the world
        Vector3 worldRelativeInput = transform.TransformDirection(input);

        // if the player isn't pressing anything pass
        if (input == Vector3.zero && GroundChecks().Item1)
        {
            maxSpeed = 0f;// set current max speed to zero
            force = defaultForce;// set the current force to be the default
        }
        // if the player is pressing shift pass
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeed = maxShiftSpeed;// set the current max speed to the shift max speed
            force = shiftForce;// set the current force to the shift force
        }
        else
        {
            force = defaultForce;// set the current force to be the default
            maxSpeed = defaultMaxSpeed;// set the current maxSpeed to be the default
        }

        

        // process the input that was cenverted to be relative to the world by normalizing it and multiplying it by time and the current amount of force to be applied
        Vector3 processedInput = worldRelativeInput.normalized * force * Time.fixedDeltaTime;

        // processedInput's X and Z with the Rigidbody's Y velocity
        inputAndY = new Vector3(processedInput.x, rigidBody.velocity.y * 2f, processedInput.z);
        
        rigidBody.AddForce(inputAndY, ForceMode.Force);// add inputAndY amount of force to the player

        float speed = Vector3.Magnitude(rigidBody.velocity);// calculate current object speed

        //if the player's speed is higher than max speed pass
        if (speed > maxSpeed)
        {
            float horizontalBrakeSpeed = speed - maxSpeed;// calculate the horizontal speed decrease
            float verticalBrakeSpeed = speed - defaultMaxSpeed;// calculate the vertical speed decrease

            // instantiate a variable that is the player's velocity normalized
            Vector3 normalisedVelocity = rigidBody.velocity.normalized;

            // calculate the brake Vector3 value
            Vector3 brakeVelocity = new Vector3(normalisedVelocity.x * horizontalBrakeSpeed, normalisedVelocity.y * verticalBrakeSpeed, normalisedVelocity.z * horizontalBrakeSpeed);

            //if the player isn't on ground apply opposing brake force multiplied by air break
            if (!GroundChecks().Item1)
            {
                rigidBody.AddForce(-brakeVelocity * extraAirBreak);
            }
            // if the player isn't pressing anything multiply the breakspeed by the extraNoInputBreak
            else if (input == Vector3.zero)
            {
                rigidBody.AddForce(-brakeVelocity * extraNoInputBreak);
            }
            else// apply the break normally
            {
                rigidBody.AddForce(-brakeVelocity);
            }
        }
    }

    // jump code
    public void Jump()
    {
        //if the player is floating with shift or on ground sets jumped to zero
        if (GroundChecks().Item1 || isFloating)
        {
            jumped = 0;
        }

        // if the player jumped less times than the maxJumps and pressed space
        if (jumped < maxJumps && Input.GetKeyDown(KeyCode.Space))
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);// sets rigid body y force to 0 float
            rigidBody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);// adds jumpForce amount of force instantly to the y axis e.g. upwards
            jumped++;// increments jump by 1
        }
    }


    // shift/sneak code
    private void Shift()
    {
        RaycastHit hit;
        // instantiate an empty RaycastHit and set it's distance o twice the allowed shift walking distance
        RaycastHit nearHit = new RaycastHit();
        nearHit.distance = maxShiftDistance * 2f;

        // if the player is pressing LeftShift GroundChecks and doesn't have shiftYPos set shiftYPos to the Y position where the GroundChecks ray hit and hasShiftYPos to true
        if (Input.GetKey(KeyCode.LeftShift) && GroundChecks().Item1 && !hasShiftYPos)
        {
            shiftYPos = GroundChecks().Item2 + halfPlayerHeight;
            hasShiftYPos = true;
        }

        // if the player isn't on ground and hasShiftPos and is pressing shift set isFloating to true
        if (!GroundChecks().Item1 && hasShiftYPos && Input.GetKey(KeyCode.LeftShift) && jumped == 0)
        {
            isFloating = true;
        }

        // if the player is on ground or pressed space or isn't pressing shift set isfloating to false
        if (GroundChecks().Item1 || Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.LeftShift))
        {
            isFloating = false;
        }

        // if the player is presses space or isn't pressing shift set hasShiftPos to false
        if (Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.LeftShift))
        {
            hasShiftYPos = false;
        }

        // if shiftYPos has benn set and player isn't on ground pass
        if (hasShiftYPos && !GroundChecks().Item1)
        {
            // a for loop that runs iterations times
            for (int i = 1; i <= iterations; i++)
            {
                // a spinning direction that spins 360 / iterations + 1 deegrees each loop iteration
                Quaternion dir = Quaternion.AngleAxis((360f / iterations) * i, Vector3.up);

                // if a ray of dir direction hits pass
                if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - halfPlayerHeight, transform.position.z), dir * Vector3.forward))
                {
                    // sends the same raycast as in the if statement and sets hit
                    Physics.Raycast(new Vector3(transform.position.x, transform.position.y - halfPlayerHeight, transform.position.z), dir * Vector3.forward, out hit);

                    // if the distance hit hit is shorter than the nearHit distance and shorter than 2 times the max shift distance set nearHit to hit
                    if (hit.distance < nearHit.distance)
                    {
                        nearHit = hit;
                    }   
                }
            }
        }

        // if the player is pressing shift, hasShiftPos, is floating and isn't on ground pass
        if (isFloating)
        {
            rigidBody.useGravity = false;// dissable the player's gravity
            
            // clamps the player's X and Z position to a square centered on nearhit.point and sides 2 times maxShiftDistance of length and sets the Y position to shiftYPos
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, nearHit.point.x - maxShiftDistance, nearHit.point.x + maxShiftDistance), shiftYPos, Mathf.Clamp(transform.position.z, nearHit.point.z - maxShiftDistance, nearHit.point.z + maxShiftDistance));
        }
        else// enable the player's gravity
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
            rigidBody.drag = drag;// set drag to drag
            rigidBody.angularDrag = drag;// set angular drag to drag
        }
        else if (isFloating)
        {
            rigidBody.drag = floatingDrag;// set drag to floatingDrag
            rigidBody.angularDrag = floatingDrag;// set angular drag to floatingDrag
        }
        else
        {
            rigidBody.drag = airDrag;// set drag to airDrag
            rigidBody.angularDrag = airDrag;// set angular drag to airDrag
        }
    }

    // sends down a raycast and returns if the ray hit and the hit height
    private Tuple<bool, float> GroundChecks()
    {
        // instantiate an empty RaycastHit
        RaycastHit hit = new RaycastHit();

        // if a ray sent from the player's centre directed downward and of 1.01 times half the player's height hits return true and the Y point where the ray hit
        if (Physics.Raycast(transform.position, Vector3.down, out hit, halfPlayerHeight * 1.01f))
        {
            return Tuple.Create(true, hit.point.y);
        }
        else// return false and 0
        {
            return Tuple.Create(false, 0f);
        }
    }
}