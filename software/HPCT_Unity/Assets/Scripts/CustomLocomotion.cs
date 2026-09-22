using UnityEngine;
using UnityEngine.InputSystem;

public class CustomLocomotion : MonoBehaviour
{
    [Tooltip("Action Reference from Input Action Asset (Vector2 - joystick movement)")]
    public InputActionReference moveActionReference;

    public Transform xrRigTransform;
    public Transform headTransform;
    public Transform controllerTransform;
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 90f;

    private InputAction moveAction;

    private void OnEnable()
    {
        if (moveActionReference != null)
        {
            moveAction = moveActionReference.action;
            moveAction.Enable();
        }
    }

    private void OnDisable()
    {
        moveAction?.Disable();
    }

    void Update()
    {
        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();

        // Forward/backward
        if (Mathf.Abs(input.y) >= 0.3f)
        {
            Vector3 forward = new Vector3(controllerTransform.forward.x, 0, controllerTransform.forward.z).normalized;
            Vector3 move = forward * input.y * moveSpeed * Time.deltaTime;
            xrRigTransform.position += move;
        }
        // Left/right turn
        else if (Mathf.Abs(input.x) >= 0.3f)
        {
            float rotation = input.x * rotationSpeed * Time.deltaTime;
            xrRigTransform.Rotate(0, rotation, 0);
        }
    }
}
