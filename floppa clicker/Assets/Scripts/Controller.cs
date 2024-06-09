using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Utilities;

namespace player
{
    [System.Serializable]
    public class SavedData
    {
        public Vector3 position;
        public int maxJumps;
        public int jumpForce;
    }

    public class Controller : MonoBehaviour
    {
        #region Variables

        [Header("2D movement variables")]
        [SerializeField] private float defaultForce;
        [SerializeField] private float shiftForce;

        [SerializeField] private float extraAirBreak;
        [SerializeField] private float extraNoInputBreak;

        [SerializeField] private float defaultMaxSpeed;
        [SerializeField] private float maxShiftSpeed;

        [SerializeField] private float defaultDrag;
        [SerializeField] private float floatingDrag;
        [SerializeField] private float airDrag;

        [SerializeField] private Vector3 worldRelativeInput;

        [Header("sneak variables")]
        [SerializeField] private int sneakIterations;

        [SerializeField] private float precisionDividend;

        [SerializeField] private float maxShiftDistance;

        [SerializeField] private float shiftYPos;

        [SerializeField] private bool hasShiftYPos;
        [SerializeField] private bool isFloating;

        [SerializeField] private Vector3 cancelDirection;

        [Header("stairs variables")]
        [SerializeField] private int lastGroundedState;

        [SerializeField] private float maxStepHeight;

        [SerializeField] private bool shouldFall;
        [SerializeField] private bool stepped;

        [Header("jump variables")]
        [SerializeField] private int maxJumps;

        [SerializeField] private int jumped;

        [SerializeField] private bool jumpedBool;

        [SerializeField] private float jumpForce;

        [SerializeField] private float jumpCooldown;
        [SerializeField] public float jumpCooldownDefault;

        [Header("others")]
        [SerializeField] private Rigidbody rigidBody;

        [SerializeField] private MeshRenderer rendererr;

        [SerializeField] private LayerMask playerLayer;

        [SerializeField] private Vector3 playerDimensions;

        [SerializeField] Vector3 parentLastPosition;

        #endregion Variables

        // while the instance is being loaded
        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();// gets the Rigidbody from the object the instance of the code is atached to
            rendererr = GetComponent<MeshRenderer>();// gets the MeshRenderer from the object the instance of the code is atached to
            playerDimensions = rendererr.bounds.extents;// uses the renderer bounds to find half the player height
            playerLayer = gameObject.layer;
        }

        // once every frame
        private void Update()
        {
            Jump();
            Drag();
        }

        // once every physics step
        private void FixedUpdate()
        {
            Gravity();
            Shift();
            OnTheMove();
            StairStepper(-45);
            StairStepper(0);
            StairStepper(45);
            lastGroundedState = GroundChecks().Item1;
        }

        // 2D movement code
        private void OnTheMove()
        {
            float maxSpeed;
            float force;

            // get player's 2D inputs with unity's input system
            Vector3 rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            // player input is relative to the player by default so convert it to be relative to the world
            worldRelativeInput = transform.TransformDirection(rawInput);

            // if the player isn't pressing anything pass
            if (rawInput == Vector3.zero && GroundChecks().Item1 == 1)
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
            Vector3 processedInput = (worldRelativeInput - cancelDirection).normalized * force * Time.fixedDeltaTime;

            Vector3 inputAndY = RemoveY(new Vector3(processedInput.x, rigidBody.velocity.y * 2f, processedInput.z), isFloating);

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
                Vector3 brakeVelocity = RemoveY(new Vector3(normalisedVelocity.x * horizontalBrakeSpeed, normalisedVelocity.y * verticalBrakeSpeed, normalisedVelocity.z * horizontalBrakeSpeed), rigidBody.velocity.y < 0);

                //if the player isn't on ground apply opposing brake force multiplied by air break
                if (GroundChecks().Item1 == 0)
                {
                    rigidBody.AddForce(-brakeVelocity * extraAirBreak);
                }
                // if the player isn't pressing anything multiply the breakspeed by the extraNoInputBreak
                else if (rawInput == Vector3.zero)
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
        private void Jump()
        {
            //if the player is floating with shift or on ground sets jumped and cooldown to zero
            if (GroundChecks().Item1 == 1)
            {
                jumped = 0;
                jumpedBool = false;
            }

            if (jumpCooldown > 0)
            {
                jumpCooldown -= Time.deltaTime;
            }

            // this will make it so the player will have to have double jump before jumping on air
            if (jumped == 0 && GroundChecks().Item1 == 0)
            {
                jumped++;
            }

            // if the player jumped less times than the maxJumps and pressed space and the jump ooldown has worn off
            if (jumped < maxJumps && Input.GetKeyDown(KeyCode.Space) && jumpCooldown <= 0)
            {
                rigidBody.velocity = RemoveY(rigidBody.velocity);// sets rigid body y force to 0 float
                rigidBody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);// adds jumpForce amount of force instantly to the y axis e.g. upwards

                jumpedBool = true;
                jumped++;// increments jump by 1
                jumpCooldown += jumpCooldownDefault;// adds jump cooldown to prevent miscollisions with the floor
            }
        }

        // shift/sneak code
        private void Shift()
        {
            // if the player is pressing LeftShift GroundChecks and doesn't have shiftYPos set shiftYPos to the Y position where the GroundChecks ray hit and hasShiftYPos to true
            if (Input.GetKey(KeyCode.LeftShift) && GroundChecks().Item1 == 1 && !hasShiftYPos)
            {
                shiftYPos = GroundChecks().Item2 + playerDimensions.y;
                hasShiftYPos = true;
            }
            // if the player isn't on ground and hasShiftPos and is pressing shift set isFloating to true
            if (GroundChecks().Item1 != 1 && hasShiftYPos && Input.GetKey(KeyCode.LeftShift))
            {
                isFloating = true;
            }
            // if the player is on ground or pressed space or isn't pressing shift set isfloating to false
            if (GroundChecks().Item1 != 0 || Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.LeftShift))
            {
                isFloating = false;
            }
            // if the player is presses space or isn't pressing shift set hasShiftPos to false
            if (Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.LeftShift))
            {
                hasShiftYPos = false;
            }

            RaycastHit hit;
            // instantiate an empty RaycastHit and set it's distance o twice the allowed shift walking distance
            RaycastHit nearHit = new RaycastHit();
            nearHit.distance = maxShiftDistance * 2f;

            // if shiftYPos has been set
            if (isFloating)
            {
                // a for loop that runs a tenth of iterations times
                for (int i = 1; i <= precisionDividend; i++)
                {
                    // a spinning direction that spins a proportionaly to the iteration number each loop iteration
                    Quaternion dir = Quaternion.AngleAxis((360f / precisionDividend) * i, Vector3.up);
                    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * Vector3.forward, Color.red);
                    // if a ray of dir direction hits pass
                    if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * Vector3.forward))
                    {
                        // sends the same raycast as in the if statement and sets hit
                        Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * Vector3.forward, out hit);
                        // if the distance hit hit is shorter than the nearHit distance
                        if (hit.distance < nearHit.distance)
                        {
                            nearHit = hit;
                        }
                    }
                }
            }

            Vector3 dir2 = nearHit.point - InsertY(transform.position, transform.position.y - playerDimensions.y);
            float precisionAngle = 360f / precisionDividend * 2;

            // if shiftYPos has been set
            if (isFloating)
            {
                // a for loop that runs iterations times
                for (int i = -(sneakIterations / 2); i <= sneakIterations / 2; i++)
                {
                    // a spinning direction that spins a proportionaly to the iteration number each loop iteration
                    Quaternion dir = Quaternion.AngleAxis((precisionAngle / sneakIterations) * i, Vector3.up);

                    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * dir2, Color.red);
                    // if a ray of dir direction hits pass
                    if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * dir2))
                    {
                        // sends the same raycast as in the if statement and sets hit
                        Physics.Raycast(new Vector3(transform.position.x, transform.position.y - playerDimensions.y, transform.position.z), dir * dir2, out hit);
                        // if the distance hit hit is shorter than the nearHit distance
                        if (hit.distance < nearHit.distance)
                        {
                            nearHit = hit;
                        }
                    }
                }
            }

            cancelDirection = Vector3.zero;

            if (isFloating)
            {
                cancelDirection = RemoveY(transform.position - nearHit.point).normalized;

                print(cancelDirection);

                rigidBody.velocity -= cancelDirection;

                transform.position = InsertY(transform.position, shiftYPos);
            }
        }

        private void StairStepper(float angle)
        {
            RaycastHit downwardRay;

            // if the player didn't jump and a downward raycast casted from maxStepLength above ground and in front of the player but rotated by angle degrees around the player hits something
            if (jumped == 0 && Physics.Raycast(InsertY(transform.position + (Quaternion.AngleAxis(angle, Vector3.up) * worldRelativeInput), (transform.position.y - playerDimensions.y) + maxStepHeight), Vector3.down, out downwardRay, maxStepHeight, ~playerLayer))
            {
                // if the player IS moving and isn't pressing shift
                if (worldRelativeInput != Vector3.zero && !Input.GetKey(KeyCode.LeftShift))
                {
                    // change the player's Y position to the Y position the raycast hit
                    transform.position = InsertY(transform.position, downwardRay.point.y + playerDimensions.y);
                    // set stepped, a boolean used to avoid teleport the player to the round while they move up the step, to true
                    stepped = true;
                }
            }

            // if the player is on ground set stepped to false 
            if (GroundChecks().Item1 == 1)
            {
                stepped = false;
            }

            // if the player shouldn't be falling is near the ground and hasn't recetly stepped up a step and isn't pressing shift  
            if (!shouldFall && GroundChecks().Item1 == 2 && !stepped && !Input.GetKey(KeyCode.LeftShift) && jumped == 0)
            {
                // teleport the player to the ground
                transform.position = InsertY(transform.position, GroundChecks().Item2 + playerDimensions.y);
            }
        }

        private void Gravity()
        {
            shouldFall = false;

            // if the player wasn't near the ground last frame, isn't on ground this frame and isn't floating or has jumped
            shouldFall = ((lastGroundedState != 2 && GroundChecks().Item1 == 0 && !isFloating) || jumpedBool);

            // if the player should fall use gravity else don't
            rigidBody.useGravity = shouldFall;
        }

        // applies different amounts of drag to the player
        private void Drag()
        {
            //uses GroundChecks to check if player is on the ground
            if (GroundChecks().Item1 == 1)
            {
                rigidBody.drag = defaultDrag;
                rigidBody.angularDrag = defaultDrag;
            }
            else if (isFloating)
            {
                rigidBody.drag = floatingDrag;
                rigidBody.angularDrag = floatingDrag;
            }
            else
            {
                rigidBody.drag = airDrag;
                rigidBody.angularDrag = airDrag;
            }
        }

        // sends down a raycast and returns if the ray hit and the hit height
        private Tuple<int, float> GroundChecks()
        {
            // instantiate an empty RaycastHit
            RaycastHit hit;

            // if a ray sent from the player's centre directed downward and of 1.01 times half the player's height hits return 1 and the Y point where the ray hit
            if (Physics.Raycast(transform.position, Vector3.down, out hit, playerDimensions.y * 1.01f, ~playerLayer))
            {
                return Tuple.Create(1, hit.point.y);
            }
            // if a ray sent from the player's centre directed downward and of maxStepHeight length hits return 2 and the Y point where the ray hit
            else if (Physics.Raycast(transform.position, Vector3.down, out hit, playerDimensions.y + maxStepHeight, ~playerLayer))
            {
                return Tuple.Create(2, hit.point.y);
            }
            else// return 0 and 0
            {
                return Tuple.Create(0, 0f);
            }
        }
    }
}