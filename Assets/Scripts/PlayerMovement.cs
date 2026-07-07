using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    public PhysicsMaterial friction;
    public Transform feet;
    private Rigidbody rb;
    private CapsuleCollider coll;

    public LayerMask groundLayer;

    private Vector2 moveInput;

    public float dampingXZ = 0f;

    private bool jumpRequested;
    private bool crouchRequested;
    public bool isGrounded;
    public bool isCrouched;

    public Vector3 velocity;

    // EDITABLE //
    private const float GROUND_FORCE = 3000f;
    private const float AIR_FORCE = 500f;
    private const float CROUCH_FORCE = 300f;
    private const float VELOCITY_LIMIT = 15f;
    private const float GROUND_DAMPING = 0.1f;
    private const float AIR_DAMPING = 0.01f;
    private const float CROUCH_DAMPING = 0.001f;
    private const float JUMP_FORCE = 20000f;
    // EDITABLE //



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

        Debug.Log(isGrounded = Physics.CheckSphere(feet.position, 0.2f, groundLayer));

        if (isGrounded = Physics.CheckSphere(feet.position, 0.2f, groundLayer))
            isGrounded = true;
        else
            isGrounded = false;

        if (jumpAction.WasPressedThisFrame())
            jumpRequested = true;

        if (crouchAction.WasPressedThisFrame())
            crouchRequested = true;
    }

    private void FixedUpdate()
    {
        Move();

        if (jumpRequested && isGrounded)
        {
            jumpRequested = false;
            Jump();
        }
        
        if (crouchRequested && !isCrouched)
        {
            isCrouched = true;
            crouchRequested = false;
            Crouch();
        }
        else if (crouchRequested && isCrouched)
        {
            isCrouched = false;
            crouchRequested = false;
            Uncrouch();
        }
    }


    // Functions
    private void Move()
    {
        Debug.Log(rb.linearVelocity.magnitude);

        float force = isGrounded ? GROUND_FORCE : AIR_FORCE;
        force = isCrouched && isGrounded ? CROUCH_FORCE : force;

        dampingXZ = isGrounded ? GROUND_DAMPING : AIR_DAMPING;
        dampingXZ = isCrouched && isGrounded ? CROUCH_DAMPING : dampingXZ;

        rb.AddRelativeForce(new Vector3(moveInput.x, 0f, moveInput.y) * force, ForceMode.Force);

        // Manual damping
        velocity = rb.linearVelocity;

        velocity.x *= (1 - dampingXZ);
        velocity.z *= (1 - dampingXZ);

        rb.linearVelocity = velocity;

        // Standing velocity limit XZ axes
        if (isGrounded && !isCrouched)
        {
            if (rb.linearVelocity.x > VELOCITY_LIMIT)
            {
                rb.linearVelocity = new Vector3(VELOCITY_LIMIT, rb.linearVelocity.y, rb.linearVelocity.z);
            }
            else if (rb.linearVelocity.x < -VELOCITY_LIMIT)
            {
                rb.linearVelocity = new Vector3(-VELOCITY_LIMIT, rb.linearVelocity.y, rb.linearVelocity.z);
            }

            if (rb.linearVelocity.z > VELOCITY_LIMIT)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, VELOCITY_LIMIT);
            }
            else if (rb.linearVelocity.z < -VELOCITY_LIMIT)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, -VELOCITY_LIMIT);
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * JUMP_FORCE);
    }

    void Crouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
        transform.position -= new Vector3(0, 0.25f, 0);

        friction.dynamicFriction = 0;
        friction.staticFriction = 0.5f;
    }
    void Uncrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2, transform.localScale.z);
        transform.position += new Vector3(0, 0.25f, 0);

        friction.dynamicFriction = 1;
        friction.staticFriction = 2;
    }
}