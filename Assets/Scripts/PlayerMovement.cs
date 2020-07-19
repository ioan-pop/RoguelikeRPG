using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController characterController;
    [SerializeField] Animator characterAnimator;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Transform followCamera;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    float turnSmoothVelocity;
    float turnSmoothTime = 0.1f;

    const float gravity = -35f;
    Vector3 velocity;
    float groundDistance = 0.49f;
    float jumpHeight = 3f;
    float terminalVelocity = -25f;

    bool isGrounded;

    bool validSlope;
    Vector3 slideVelocity;
    const float slideFriction = 0.8f;

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJumping();
        HandleSliding();
    }

    private void CheckGrounded() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded) {
            velocity.y = 0f;
        }
    }

    private void HandleMovement() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f) {
            if(vertical > 0) {
                characterAnimator.SetBool("Running", true);
            } else {
                characterAnimator.SetBool("Running", false);
            }

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + followCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
        }
    }

    private void HandleJumping() {
        if (Input.GetButtonDown("Jump") && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        if (velocity.y < terminalVelocity) {
            velocity.y = terminalVelocity;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    void OnControllerColliderHit (ControllerColliderHit hit) {
        Vector3 groundNormal = hit.normal;

        Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.yellow, 1f);
        Debug.DrawLine(transform.position, transform.position + groundNormal, Color.green, 1f);

        if(Vector3.Angle(Vector3.up, groundNormal) < characterController.slopeLimit) {
            validSlope = true;
            slideVelocity = Vector3.zero;
        } else {
            validSlope = false;
            slideVelocity.x += (1f - groundNormal.y) * groundNormal.x * (1f - slideFriction);
            slideVelocity.z += (1f - groundNormal.y) * groundNormal.z * (1f - slideFriction);
        }
    }

    private void HandleSliding() {
        if(!validSlope) {
            characterController.Move(slideVelocity * Time.deltaTime);
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
