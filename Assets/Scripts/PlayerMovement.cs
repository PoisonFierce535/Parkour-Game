using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{
    // TODO: add a slight speed boost to isLanded state and sliding a a bit of jumping to add speed
    // and make isLanded state better -
    // by not lossing as much speed as now, it works, but it should be better
    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    public PhysicsMaterial friction;
    public Transform feet;
    public Rigidbody rb;
    private CapsuleCollider coll;

    public LayerMask groundLayer;

    private Vector2 moveInput;

    public float dampingXZ = 0f;

    private bool jumpRequested;
    private bool crouchRequested;
    public bool isGrounded;
    public bool isCrouched;
    public bool isLanded;
    public bool canLand;

    // EDITABLE //
    private const float GROUND_FORCE = 7500f;
    private const float AIR_FORCE = 500f;
    private const float SLIDE_FORCE = 300f;
    private const float CROUCH_FORCE = SLIDE_FORCE * 3f;
    private const float JUMP_FORCE = 350f;

    private const float SLIDE_INITIAL_BOOST = 150f;

    private const float GROUNDED_VELOCITY_LIMIT = 15f;
    private const float CROUCHING_VELOCITY_LIMIT = 4f;

    private const float GROUND_DAMPING = 0.2f;
    private const float AIR_DAMPING = 0.01f;
    private const float SLIDE_DAMPING = 0.001f;
    private const float CROUCH_DAMPING = 0.001f;

    private const float IS_LANDED_DURATION = 0.3f;
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
        moveInput = moveAction.ReadValue<Vector2>();

        SetFriction();

        SetIsGroundedAndLanded();

        SetMoveRequests();
    }

    private void FixedUpdate()
    {
        Move();

        DownGravity();

        CheckMoveRequests();
    }


    // FUNCTIONS //
    private void Move()
    {
        Vector3 velocity = rb.linearVelocity;

        // Adding force
        float force = 0;
        force = isGrounded ? GROUND_FORCE : AIR_FORCE; // grounded or in-air
        force = isCrouched && isGrounded && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT ? SLIDE_FORCE : force; // sliding
        force = isCrouched && isGrounded && rb.linearVelocity.magnitude <= CROUCHING_VELOCITY_LIMIT ? CROUCH_FORCE : force; // crouching

        rb.AddRelativeForce(new Vector3(moveInput.x, 0f, moveInput.y) * force, ForceMode.Force);

        // Manual damping
        dampingXZ = isGrounded ? GROUND_DAMPING : AIR_DAMPING; // grounded or in-air
        dampingXZ = isCrouched && isGrounded && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT ? SLIDE_DAMPING : dampingXZ; // sliding
        dampingXZ = isCrouched && isGrounded && rb.linearVelocity.magnitude <= CROUCHING_VELOCITY_LIMIT ? CROUCH_DAMPING : dampingXZ; // crouching

        velocity.x *= (1 - dampingXZ);
        velocity.z *= (1 - dampingXZ);

        rb.linearVelocity = velocity;

        // Grounded velocity limit, XZ axes
        if (isGrounded && !isCrouched)
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

    private void Jump()
    {
        jumpRequested = false;

        rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
    }
    private void DownGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            Debug.Log("down");
            rb.AddForce(Vector3.up * -100, ForceMode.Force);
        }
    }

    void Crouch()
    {
        isCrouched = true;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);

        if (isGrounded && rb.linearVelocity.magnitude > CROUCHING_VELOCITY_LIMIT)
        {
            Vector3 dir = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).normalized;

            rb.AddForce(dir * SLIDE_INITIAL_BOOST, ForceMode.Impulse);
        }

        if (isGrounded)
        {
            transform.position -= new Vector3(0, 0.5f, 0);
        }
    }
    void Uncrouch()
    {
        isCrouched = false;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2, transform.localScale.z);

        if (isGrounded)
        {
            transform.position += new Vector3(0, 0.5f, 0);
        }
    }

    void SetFriction()
    {
        if (isGrounded && !isCrouched) // grounded
        {
            friction.dynamicFriction = 1;
            friction.staticFriction = 2;
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
        }
        else if (isLanded) // landed
        {
            friction.dynamicFriction = 0;
            friction.staticFriction = 0;
        }
    }
    void SetIsGroundedAndLanded()
    {
        if (Physics.CheckSphere(feet.position, 0.2f, groundLayer) && canLand == true)
        {
            isGrounded = true;
            canLand = false;

            StartCoroutine(ShortLandedState());
        }
        else if (!Physics.CheckSphere(feet.position, 0.2f, groundLayer))
        {
            isGrounded = false;
            canLand = true;
        }
    }
    void SetMoveRequests()
    {
        if (jumpAction.WasPressedThisFrame()) jumpRequested = true;

        if (crouchAction.IsPressed()) crouchRequested = true;
        else if (crouchAction.WasReleasedThisFrame()) crouchRequested = false;
    }

    void CheckMoveRequests()
    {
        if (jumpRequested && isGrounded) Jump();

        if (crouchRequested && !isCrouched) Crouch();
        else if (!crouchRequested && isCrouched) Uncrouch();
    }

    IEnumerator ShortLandedState()
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
            float boost = 0f;
            if (isCrouched) boost = 1.05f;
            else boost = 1.1f;
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