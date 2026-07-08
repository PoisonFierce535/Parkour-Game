using TMPro;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI isGroundedText;
    public TextMeshProUGUI isCrouchedText;
    public TextMeshProUGUI dampingXZText;
    public TextMeshProUGUI staticFrictionText;
    public TextMeshProUGUI dynamicFrictionText;
    public TextMeshProUGUI isLandedText;
    public TextMeshProUGUI horizontalVelocityText;

    public PhysicsMaterial friction;



    void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        Vector3 velocity = playerMovement.rb.linearVelocity;
        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float horizontalSpeed = flatVelocity.magnitude;

        dampingXZText.text = "DampingXZ: " + playerMovement.dampingXZ;
        velocityText.text = "Velocity: " + playerMovement.rb.linearVelocity.x;
        isGroundedText.text = "isGrounded: " + playerMovement.isGrounded;
        isCrouchedText.text = "isCrouched: " + playerMovement.isCrouched;
        staticFrictionText.text = "StaticFriction: " + friction.staticFriction;
        dynamicFrictionText.text = "DynamicFriction: " + friction.dynamicFriction;
        isLandedText.text = "IsLanded: " + playerMovement.isLanded;
        horizontalVelocityText.text = "HorizontalVelocity: " + horizontalSpeed;
    }
}
