using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private Vector2 moveActionInput;

    public Transform feet;
    public Rigidbody rb;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public Vector3 currentWallSide;
    public Vector3 recentWallSide;

    public float dampingXZ = 0f;
    public float airForceDecreaser = 0;

    public bool jumpRequested;
    private bool crouchRequested;
    private bool uncrouchRequested;
    private bool wallrunRequested;

    public bool isGrounded;
    public bool isCrouched;
    public bool isLanded;
    public bool isWallrunning;
    public bool isCrouchWalking;
    public bool isSliding;

    public bool canLand;
    public bool canSlideInitialBoost;
    
    // EDITABLE //
    private const float GROUND_FORCE = 7500f;
    private const float AIR_FORCE = 500f;
    private const float SLIDE_FORCE = CROUCH_FORCE / 2;
    private const float CROUCH_FORCE = 900f;
    private const float JUMP_FORCE = 350f;
    private const float WALLRUN_COUNTER_UP_FORCE = 70f;
    private const float DOWN_GRAVITY_FORCE = 400f;

    private const float JUMPOFF_UP_BOOST = 450f;
    private const float JUMPOFF_SIDE_BOOST = 8f;
    private const float JUMPOFF_DIRECTION_BOOST = 70f;
    private const float SLIDE_INITIAL_BOOST = 150f;
    private const float JUMP_INITIAL_BOOST = 100f;
    private const float CROUCH_ISLANDED_INITIAL_BOOST = 1.01f;
    private const float GROUNDED_ISLANDED_INITIAL_BOOST = 1.05f;

    private const float GROUNDED_VELOCITY_LIMIT = 15f; // if standing on ground
    private const float CROUCH_VELOCITY_LIMIT = 6f; // above is sliding
    private const float SLIDE_VELOCITY_LIMIT = 14f; // above won't let you go faster

    private const float GROUND_DAMPING = 0.2f;
    private const float AIR_DAMPING = 0.01f;
    private const float SLIDE_DAMPING = 0.01f;
    private const float CROUCH_DAMPING = 0.05f;

    private const float GROUNDED_VELOCITY_EPSILON = 0.1f;

    private const float ISLANDED_DURATION = 0.3f;
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
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        moveActionInput = moveAction.ReadValue<Vector2>();

        SetIsGroundedAndIsLanded();

        SetMoveRequests();

        SetWallSide();
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0) return;

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
            rb.AddForce(Vector3.down * DOWN_GRAVITY_FORCE, ForceMode.Force);
        }
    }
    //
    private void GroundMove()
    {
        Vector3 velocity = rb.linearVelocity;

        if (isGrounded)
        {
            // Adding force
            float force = GROUND_FORCE;

            if (isCrouched)
            {
                if (rb.linearVelocity.magnitude <= CROUCH_VELOCITY_LIMIT)
                {
                    isCrouchWalking = true;
                    isSliding = false;

                    force = CROUCH_FORCE;

                    rb.AddRelativeForce(new Vector3(moveActionInput.x, 0f, moveActionInput.y) * force, ForceMode.Force);
                }
                else if (rb.linearVelocity.magnitude > CROUCH_VELOCITY_LIMIT)
                {
                    isCrouchWalking = false;
                    isSliding = true;

                    force = SLIDE_FORCE;

                    float newMAIX;
                    float newMAIZ;
                    
                    Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
                    if (localVelocity.x > CROUCH_VELOCITY_LIMIT && moveActionInput.x > 0) newMAIX = 0f;
                    else if (localVelocity.x < -CROUCH_VELOCITY_LIMIT && moveActionInput.x < 0) newMAIX = 0f;
                    else newMAIX = moveActionInput.x;
                    if (localVelocity.z > CROUCH_VELOCITY_LIMIT && moveActionInput.y > 0) newMAIZ = 0f;
                    else if (localVelocity.z < -CROUCH_VELOCITY_LIMIT && moveActionInput.y < 0) newMAIZ = 0f;
                    else newMAIZ = moveActionInput.y;
                    
                    rb.AddRelativeForce(new Vector3(newMAIX, 0f, newMAIZ) * force, ForceMode.Force);         
                }
            }
            else
            {
                isCrouchWalking = false;
                isSliding = false;

                rb.AddRelativeForce(new Vector3(moveActionInput.x, 0f, moveActionInput.y) * force, ForceMode.Force);
            }

            // Manual damping
            dampingXZ = GROUND_DAMPING;
            dampingXZ = isCrouchWalking ? CROUCH_DAMPING : dampingXZ;
            dampingXZ = isSliding ? SLIDE_DAMPING : dampingXZ;

            velocity.x *= (1 - dampingXZ);
            velocity.z *= (1 - dampingXZ);

            velocity = velocity.magnitude < GROUNDED_VELOCITY_EPSILON ? new Vector3(0, velocity.y, 0) : velocity;

            rb.linearVelocity = velocity;

            // Grounded velocity limit
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

        if (isGrounded || isWallrunning) return;

        if (isGrounded) airForceDecreaser = 2; // resets


        // Adding force
        airForceDecreaser /= 1.05f;

        rb.AddRelativeForce(new Vector3(moveActionInput.x, 0f, moveActionInput.y) * AIR_FORCE, ForceMode.Force);

        // Manual damping
        dampingXZ = AIR_DAMPING;

        velocity.x *= (1 - dampingXZ);
        velocity.z *= (1 - dampingXZ);

        rb.linearVelocity = velocity;
    }
    //
    private void UseMoveRequestsAndMoves()
    {
        if (jumpRequested) Jump();

        if (crouchRequested) Crouch();
        else if (uncrouchRequested) Uncrouch();

        if (wallrunRequested) StartCoroutine(Wallrun());
    }
    private void Jump()
    {
        jumpRequested = false;

        rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);

        Vector3 dir = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;
        if (isCrouched)
        {
            rb.AddForce(dir * JUMP_INITIAL_BOOST / 3, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(dir * JUMP_INITIAL_BOOST, ForceMode.Impulse);
        }
    }
    private void Crouch()
    {
        crouchRequested = false;

        isCrouched = true;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);

        if (isGrounded && rb.linearVelocity.magnitude > CROUCH_VELOCITY_LIMIT && canSlideInitialBoost) // sliding
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
        uncrouchRequested = false;

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

        bool canVerticalWallrunInitialBoost = true;
        bool startingVerticalWallrun = false;

        while (isWallrunning)
        {
            // vertial wallrun inital boost
            if (GetWallDirection() == "Up" && canVerticalWallrunInitialBoost)
            {
                canVerticalWallrunInitialBoost = false;
                startingVerticalWallrun = true;
                rb.AddRelativeForce(Vector3.up * 150, ForceMode.Impulse);
            }
            else
            {
                canVerticalWallrunInitialBoost = false;
            }
            
            // jump-off
            if (jumpAction.WasPressedThisFrame())
            {
                if (rb.linearVelocity.y < 0)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                }

                rb.AddRelativeForce(Vector3.up * JUMPOFF_UP_BOOST, ForceMode.Impulse);

                if (GetWallDirection() != "Up")
                {
                    if (moveActionInput.y > 0)
                    {
                        rb.AddRelativeForce(Vector3.forward * JUMPOFF_DIRECTION_BOOST, ForceMode.Impulse);
                    }
                    else if (moveActionInput.y < 0)
                    {
                        rb.AddRelativeForce(Vector3.back * JUMPOFF_DIRECTION_BOOST, ForceMode.Impulse);
                    }

                    rb.linearVelocity += currentWallSide * JUMPOFF_SIDE_BOOST;
                }

                break;
            }

            // counter (up) force
            if (rb.linearVelocity.y <= 0) rb.AddRelativeForce(Vector3.up * WALLRUN_COUNTER_UP_FORCE, ForceMode.Force);

            // counter (side) force (if started vertical wallrun)
            if (startingVerticalWallrun)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x / 1.005f, rb.linearVelocity.y, rb.linearVelocity.z / 1.005f);
            }

            // stop wallrunnnig if out of the wall
            if (!Physics.CheckSphere(transform.position, 1.3f, wallLayer)) break;
            else if (isGrounded) break;

            yield return new WaitForSeconds(0.0001f);
        }

        isWallrunning = false;

        recentWallSide = currentWallSide;
        currentWallSide = Vector3.zero;
    }
    //
    private string GetWallDirection()
    {
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

        foreach (Vector3 dir in directions)
        {
            if (Physics.SphereCast(transform.position, 0.14f, dir, out RaycastHit hit, 1, wallLayer))
            {
                if (dir == transform.forward) return "Up";
                else if (dir == -transform.forward) return "Down";
                else if (dir == transform.right) return "Right";
                else return "Left";
            }
        }

        return null;
    }
    private void SetWallSide()
    {
        if (!isGrounded && !isCrouched && !isWallrunning)
        {
            Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

            foreach (Vector3 dir in directions)
            {
                if (Physics.SphereCast(transform.position, 0.14f, dir, out RaycastHit hit, 1, wallLayer))
                {
                    float distanceFromTop = hit.collider.bounds.max.y - hit.point.y;
                    if (distanceFromTop < 0.15f)
                    {
                        continue;
                    }

                    if (hit.normal != recentWallSide)
                    {
                        float alignment = Vector3.Dot(hit.normal, transform.up);
                        float toleranceDegrees = 45f;
                        float threshold = Mathf.Cos((90f - toleranceDegrees) * Mathf.Deg2Rad);

                        if (MathF.Abs(alignment) < threshold)
                        {
                            recentWallSide = Vector3.zero;
                            currentWallSide = hit.normal;
                            return;
                        } 
                    }
                }
            }
        }
        else if (isGrounded)
        {
            currentWallSide = Vector3.zero;
            recentWallSide = Vector3.zero;
        }
    }
    private void SetIsGroundedAndIsLanded()
    {
        if (Physics.CheckSphere(feet.position, 0.2f, groundLayer) && canLand == true)
        {
            isGrounded = true;
            canLand = false;
        }
        else if (!Physics.CheckSphere(feet.position, 0.2f, groundLayer))
        {
            isGrounded = false;
            canLand = true;
        }
    }
    private void SetMoveRequests()
    {
        if (jumpAction.WasPressedThisFrame() && !isWallrunning && isGrounded) jumpRequested = true;

        if (crouchAction.IsPressed() && !isCrouched) crouchRequested = true;
        else if (crouchAction.WasReleasedThisFrame() && isCrouched) uncrouchRequested = true;

        if (currentWallSide != Vector3.zero && !isWallrunning) wallrunRequested = true;
    }
}