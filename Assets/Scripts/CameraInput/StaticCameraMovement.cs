using UnityEngine;
using UnityEngine.InputSystem;

public class StaticCameraMovement : MonoBehaviour
{
    public Vector2 movementVector;

    [Header("Rotation Settings")]
    [Tooltip("How smoothly the rotation changes")]
    [Range(1f, 20f)]
    public float rotationSmoothing = 10f;
    [Tooltip("Separate speed for horizontal rotation")]
    public float horizontalSpeed = 8f;
    [Tooltip("Separate speed for vertical rotation")]
    public float verticalSpeed = 5f;

    [Header("Rotation Limits")]
    [Tooltip("Minimum and maximum rotation on X axis (vertical)")]
    public Vector2 xRotationLimits = new Vector2(-80f, 80f);
    [Tooltip("Minimum and maximum rotation on Y axis (horizontal)")]
    public Vector2 yRotationLimits = new Vector2(-180f, 180f);

    [Header("Deadzone Settings")]
    [Range(0f, 45f)]
    public float deadzoneAngle = 30f;
    private const float DEADZONE_MAGNITUDE = 0.1f;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private Vector2 processedMovement;
    private Quaternion targetRotation;

    [SerializeField] private MoveGimbal moveGimbal;
    IGimbalMovement gimbalMovement;

    private void Start()
    {
        gimbalMovement = moveGimbal as IGimbalMovement;

        Vector3 currentRotation = transform.localRotation.eulerAngles;
        currentXRotation = currentRotation.x;
        currentYRotation = currentRotation.y;
        
        currentXRotation = ClampAngle(currentXRotation, xRotationLimits.x, xRotationLimits.y);
        currentYRotation = ClampAngle(currentYRotation, yRotationLimits.x, yRotationLimits.y);
    }

    public void CameraInputPlayer(InputAction.CallbackContext _context) 
    {
        movementVector = _context.ReadValue<Vector2>();
        gimbalMovement.OnInputPressed(movementVector);
    }

    private void Update()
    {
        if (movementVector.magnitude > DEADZONE_MAGNITUDE)
        {
            HandleRotation();
        }
    }

    private void HandleRotation()
    {
        float angle = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        bool isPrimaryY = (angle >= 90f - deadzoneAngle && angle <= 90f + deadzoneAngle) || 
                         (angle >= 270f - deadzoneAngle && angle <= 270f + deadzoneAngle);
        bool isPrimaryX = (angle >= 0f - deadzoneAngle && angle <= 0f + deadzoneAngle) || 
                         (angle >= 180f - deadzoneAngle && angle <= 180f + deadzoneAngle);

        if (isPrimaryY)
        {
            processedMovement = new Vector2(movementVector.x * 0.2f, movementVector.y);
        }
        else if (isPrimaryX)
        {
            processedMovement = new Vector2(movementVector.x, movementVector.y * 0.2f);
        }
        else
        {
            processedMovement = movementVector;
        }

        currentXRotation -= processedMovement.y * verticalSpeed * Time.deltaTime;
        currentYRotation += processedMovement.x * horizontalSpeed * Time.deltaTime;

        currentXRotation = ClampAngle(currentXRotation, xRotationLimits.x, xRotationLimits.y);
        currentYRotation = ClampAngle(currentYRotation, yRotationLimits.x, yRotationLimits.y);

        targetRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmoothing);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
