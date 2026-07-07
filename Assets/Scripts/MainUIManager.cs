using TMPro;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI isGroundedText;
    public TextMeshProUGUI isCrouchedText;
    public TextMeshProUGUI dampingXZText;



    void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        dampingXZText.text = "DampingXZ: " + playerMovement.dampingXZ;
        velocityText.text = "Velocity: " + playerMovement.velocity.magnitude;
        isGroundedText.text = "isGrounded: " + playerMovement.isGrounded;
        isCrouchedText.text = "isCrouched: " + playerMovement.isCrouched;
    }
}
