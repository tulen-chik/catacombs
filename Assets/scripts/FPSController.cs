using UnityEngine;
using UnityEngine.InputSystem; // ����������� ���������� ��� ������������ ����

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    // ... (��������� ���������� �������� ���� ��)
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public Camera playerCamera;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    // ���������� ��� ����� ������� �����
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed = false;
    private bool isRunning = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ������ ��� ��������� ����� �� Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }


    void Update()
    {
        // ��������
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = (isRunning ? runSpeed : walkSpeed) * moveInput.y;
        float curSpeedY = (isRunning ? runSpeed : walkSpeed) * moveInput.x;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // ������
        if (jumpPressed && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        jumpPressed = false; // ���������� ���� ������

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // �������� ������
        rotationX += -lookInput.y * lookSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed * Time.deltaTime, 0);
    }
}