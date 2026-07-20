using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{
    // ALWAYS CHECK TRELLO //
    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private Vector2 moveActionInput;

    public PhysicsMaterial friction;
    public Transform feet;
    public Rigidbody rb;
    private CapsuleCollider coll;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private GameObject recentWall;

    private Vector3 wallRotation;

    private string wallRunSide;

    public float dampingXZ = 0f;
    public float airForceDecreaser = 0;

    private bool jumpRequested;
    private bool crouchRequested;
    private bool wallrunRequested;
    public bool isGrounded;
    public bool isCrouched;
    public bool isLanded;
    public bool isWallrunning;
    public bool canLand;
    public bool canSlideInitialBoost;
    private bool canWallrunInitialBoost;

    // EDITABLE //
    private const float GROUND_FORCE = 7500f;
    private const float AIR_FORCE = 500f;
    private const float SLIDE_FORCE = 300f;
    private const float CROUCH_FORCE = SLIDE_FORCE * 3f;
    private const float JUMP_FORCE = 350f;
    private const float WALLRUN_FORCE = 1000f;
    private const float WALLRUN_COUNTER_UP_FORCE = 50f;
    private const float JUMP_OFF_UP_FORCE = 600f;
    private const float JUMP_OFF_FORWARD_FORCE = 600f;

    private const float SLIDE_INITIAL_BOOST = 150f;
    private const float JUMP_INITIAL_BOOST = 100f;
    private const float CROUCHED_IS_LANDED_INITIAL_BOOST = 1.01f;
    private const float GROUNDED_IS_LANDED_INITIAL_BOOST = 1.05f;
    private const float WALLRUN_INITIAL_BOOST_UP = 100f;
    private const float WALLRUN_INITIAL_BOOST_FORWARD = 200f;

    private const float GROUNDED_VELOCITY_LIMIT = 15f;
    private const float CROUCHING_VELOCITY_LIMIT = 4f;

    private const float GROUND_DAMPING = 0.2f;
    private const float AIR_DAMPING = 0.01f;
    private const float SLIDE_DAMPING = 0.005f;
    private const float CROUCH_DAMPING = 0.001f;

    private const float IS_LANDED_DURATION = 0.3f;

    private const float RAYCAST_WALLRUN_LENGTH = 0.6f;
    // EDITABLE //



    // Enables/disables the Input System
    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Start()
    {
        moveAction = InputActions.FindAction("Move");
        jumpAction = InputActions.FindAction("Jump");
        crouchAction = InputActions.FindAction("Crouch");

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        moveActionInput = moveAction.ReadValue<Vector2>();

        SetFriction();

        SetIsGroundedAndLandedState();

        SetMoveRequests();

        SetWallrunStuff();
    }

    private void FixedUpdate()
    {
        GroundMove();
        AirMove();

        DownGravity();

        UseMoveRequestsAndMoves();
    }


    // FUNCTIONS //
    private void DownGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * 150, ForceMode.Force);
        }
    }
    //
    private void GroundMove()
    {
        Vector3 velocity = rb.linearVelocity;

        if (isGrounded)
        {
            // Adding force
            float force = GROUND_FORCE; // grounded
            force = isCrouched && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT ? SLIDE_FORCE : force; // sliding
            force = isCrouched && rb.linearVelocity.magnitude <= CROUCHING_VELOCITY_LIMIT ? CROUCH_FORCE : force; // crouching

            rb.AddRelativeForce(new Vector3(moveActionInput.x, 0f, moveActionInput.y) * force, ForceMode.Force);

            // Manual damping
            dampingXZ = GROUND_DAMPING; // grounded
            dampingXZ = isCrouched && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT ? SLIDE_DAMPING : dampingXZ; // sliding
            dampingXZ = isCrouched && rb.linearVelocity.magnitude <= CROUCHING_VELOCITY_LIMIT ? CROUCH_DAMPING : dampingXZ; // crouching

            velocity.x *= (1 - dampingXZ);
            velocity.z *= (1 - dampingXZ);

            rb.linearVelocity = velocity;

            // Grounded velocity limit, XZ axes
            if (!isCrouched)
            {
                if (rb.linearVelocity.x > GROUNDED_VELOCITY_LIMIT)
                {
                    rb.linearVelocity = new Vector3(GROUNDED_VELOCITY_LIMIT, rb.linearVelocity.y, rb.linearVelocity.z);
                }
                else if (rb.linearVelocity.x < -GROUNDED_VELOCITY_LIMIT)
                {
                    rb.linearVelocity = new Vector3(-GROUNDED_VELOCITY_LIMIT, rb.linearVelocity.y, rb.linearVelocity.z);
                }

                if (rb.linearVelocity.z > GROUNDED_VELOCITY_LIMIT)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, GROUNDED_VELOCITY_LIMIT);
                }
                else if (rb.linearVelocity.z < -GROUNDED_VELOCITY_LIMIT)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, -GROUNDED_VELOCITY_LIMIT);
                }
            }


        }
    }
    private void AirMove()
    {
        Vector3 velocity = rb.linearVelocity;
        if (isGrounded) airForceDecreaser = 2; // resets

        if (isGrounded || isWallrunning) return;

        // Adding force
        float forceX = AIR_FORCE;
        float forceZ = AIR_FORCE;

        if (Math.Abs(rb.linearVelocity.x) > 13)
        {
            forceX /= 100;
        }
        if (Math.Abs(rb.linearVelocity.z) > 13)
        {
            forceZ /= 100;
        }

        airForceDecreaser /= 1.05f;

        forceX *= airForceDecreaser;
        forceZ *= airForceDecreaser;
        Debug.Log(forceX);
        rb.AddRelativeForce(new Vector3(moveActionInput.x * forceX, 0f, moveActionInput.y * forceZ), ForceMode.Force);

        // Manual damping
        dampingXZ = AIR_DAMPING;

        velocity.x *= (1 - dampingXZ);
        velocity.z *= (1 - dampingXZ);

        rb.linearVelocity = velocity;
    }
    //
    private void UseMoveRequestsAndMoves()
    {
        if (jumpRequested && isGrounded) Jump();

        if (crouchRequested && !isCrouched) Crouch();
        else if (!crouchRequested && isCrouched) Uncrouch();

        if (wallrunRequested) StartCoroutine(Wallrun());
    }
    private void Jump()
    {
        jumpRequested = false;

        rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);

        Vector3 dir = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
        if (!isCrouched)
        {
            rb.AddForce(dir * JUMP_INITIAL_BOOST, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(dir * JUMP_INITIAL_BOOST / 3, ForceMode.Impulse);
        }
    }
    private void Crouch()
    {
        isCrouched = true;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);

        if (isGrounded && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT && canSlideInitialBoost) // sliding
        {
            canSlideInitialBoost = false;

            Vector3 dir = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;

            rb.AddForce(dir * SLIDE_INITIAL_BOOST, ForceMode.Impulse);
        }

        if (isGrounded)
        {
            transform.position -= new Vector3(0, 0.5f, 0);
        }
    }
    private void Uncrouch()
    {
        isCrouched = false;

        canSlideInitialBoost = true;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2, transform.localScale.z);

        if (isGrounded)
        {
            transform.position += new Vector3(0, 0.5f, 0);
        }
    }
    private IEnumerator Wallrun()
    {
        wallrunRequested = false;
        isWallrunning = true;

        Vector3 boostDir;


        if (Math.Abs(transform.localEulerAngles.y - wallRotation.y) <= 90)
        {
            boostDir = wallRotation;
        }
        else
        {
            boostDir = new Vector3(wallRotation.x, -wallRotation.y, wallRotation.z);
        }

        while (isWallrunning)
        {
            // jump-off
            if (jumpAction.WasPressedThisFrame())
            {
                isWallrunning = false;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                // base boost
                //rb.AddRelativeForce(Vector3.up * JUMP_OFF_UP_FORCE, ForceMode.Impulse);
                //rb.AddRelativeForce(new Vector3(1, 0, 1) * (boostDir.z * WALLRUN_INITIAL_BOOST_FORWARD), ForceMode.Impulse);

                // directional boost
                float yRotRad = Math.Abs(transform.localEulerAngles.y - boostDir.y) * Mathf.Deg2Rad;
                float lookMultiplier = Mathf.Abs(Mathf.Cos(yRotRad));

                rb.AddRelativeForce(Vector3.forward * JUMP_OFF_FORWARD_FORCE * lookMultiplier, ForceMode.Impulse);
                rb.AddRelativeForce(Vector3.up * JUMP_OFF_UP_FORCE * lookMultiplier, ForceMode.Impulse);

                break;
            }

            // initial boost
            if (canWallrunInitialBoost)
            {
                canWallrunInitialBoost = false;

                rb.AddRelativeForce(Vector3.up * WALLRUN_INITIAL_BOOST_UP, ForceMode.Impulse);
                rb.AddRelativeForce(Vector3.forward * (boostDir.z * WALLRUN_INITIAL_BOOST_FORWARD), ForceMode.Impulse);
            }
            // forward force (change it to the player's input)
            rb.AddRelativeForce(Vector3.forward * (boostDir.z * WALLRUN_FORCE * moveActionInput), ForceMode.Force);
            // counter (up) force
            if (rb.linearVelocity.y <= 0)
            {
                rb.AddRelativeForce(Vector3.up * WALLRUN_COUNTER_UP_FORCE, ForceMode.Force);
            }

            // stop wallrunnnig if out of the wall
            if (!Physics.CheckSphere(transform.position, 1, wallLayer))
            {
                isWallrunning = false;
                break;
            }
            else if (isGrounded)
            {
                isWallrunning = false;
                break;
            }

            yield return new WaitForSeconds(0.001f);
        }

        isWallrunning = false;
        wallRotation = Vector3.zero;
        wallRunSide = string.Empty;
    }
    //
    private void SetFriction()
    {
        if (isGrounded && !isCrouched) // grounded
        {
            friction.dynamicFriction = 1;
            friction.staticFriction = 2;
        }
        else if (isLanded) // landed
        {
            friction.dynamicFriction = 0;
            friction.staticFriction = 0;

            friction.frictionCombine = PhysicsMaterialCombine.Average;
        }
        else if (isGrounded && isCrouched && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT) // sliding
        {
            friction.dynamicFriction = 0.2f;
            friction.staticFriction = 0.2f;
        }
        else if (isGrounded && isCrouched && rb.linearVelocity.magnitude <= CROUCHING_VELOCITY_LIMIT) // crouching
        {
            friction.dynamicFriction = 1f;
            friction.staticFriction = 1;
        }
        else if (!isGrounded) // in-air
        {
            friction.dynamicFriction = 0;
            friction.staticFriction = 0;

            friction.frictionCombine = PhysicsMaterialCombine.Minimum;
        }

    }
    private void SetWallrunStuff()
    {
        if (!isGrounded && !isCrouched && !isWallrunning)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.right, out hit, RAYCAST_WALLRUN_LENGTH, wallLayer))
            {
                if (recentWall != hit.collider.gameObject)
                {
                    wallRunSide = "Right";
                    wallRotation = hit.collider.gameObject.transform.localEulerAngles;
                    recentWall = hit.collider.gameObject;
                }
            }
            else if (Physics.Raycast(transform.position, -transform.right, out hit, RAYCAST_WALLRUN_LENGTH, wallLayer))
            {
                if (recentWall != hit.collider.gameObject)
                {
                    wallRunSide = "Left";
                    wallRotation = hit.collider.gameObject.transform.localEulerAngles;
                    recentWall = hit.collider.gameObject;
                }
            }

            /*
            for (int i = 0; i < 179; i++)
            {
                bool wallHitRight = Physics.Raycast(transform.position, new Vector3(1 + i, 0, 0), out hit, RAYCAST_WALLRUN_LENGTH, wallLayer);
                bool wallHitLeft = Physics.Raycast(transform.position, new Vector3(1 - i, 0, 0), out hit, RAYCAST_WALLRUN_LENGTH, wallLayer);

                if (wallHitRight)
                {
                    wallRunSide = "Right";
                    wallRotation = hit.collider.gameObject.transform.localEulerAngles;
                }
                else if (wallHitLeft)
                {
                    wallRunSide = "Left";
                    wallRotation = hit.collider.gameObject.transform.localEulerAngles;
                }
            }
           */
        }
        else if (isGrounded)
        {
            wallRunSide = string.Empty;
            wallRotation = Vector3.zero;
            recentWall = null;
        }
    }
    private void SetIsGroundedAndLandedState()
    {
        if (Physics.CheckSphere(feet.position, 0.2f, groundLayer) && canLand == true)
        {
            isGrounded = true;
            canLand = false;

            if (!isCrouched)
            {
                StartCoroutine(StartLandedState());
            }
        }
        else if (!Physics.CheckSphere(feet.position, 0.2f, groundLayer))
        {
            isGrounded = false;
            canLand = true;
        }
    }
    private void SetMoveRequests()
    {
        if (jumpAction.WasPressedThisFrame()) jumpRequested = true;

        if (crouchAction.IsPressed()) crouchRequested = true;
        else if (crouchAction.WasReleasedThisFrame()) crouchRequested = false;

        if ((wallRunSide == "Right" || wallRunSide == "Left") && !isWallrunning)
        {
            wallrunRequested = true;
            canWallrunInitialBoost = true;
        }
        else
        {
            wallrunRequested = false;
        }
    }
    //
    private IEnumerator StartLandedState()
    {
        isLanded = true;

        // remove any resistance
        dampingXZ = 0f;
        friction.dynamicFriction = 0;
        friction.staticFriction = 0;

        float timer = 0f;
        while (timer < IS_LANDED_DURATION)
        {
            // boost
            float boost = isCrouched ? CROUCHED_IS_LANDED_INITIAL_BOOST : GROUNDED_IS_LANDED_INITIAL_BOOST;
            Vector3 dir = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;

            rb.linearVelocity += dir * boost;

            if (!isGrounded)
            {
                break;
            }

            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        isLanded = false;
    }
}