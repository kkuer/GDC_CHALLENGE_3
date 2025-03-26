using UnityEngine;

public class CubeRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float smoothFactor = 10f;

    [Header("Auto-Alignment Settings")]
    [SerializeField] private bool enableAutoAlign = true;
    [SerializeField] private float alignDelay = 1f; // Reduced from 2s
    [SerializeField] private float alignApproachSpeed = 3f; // Increased from 1f
    [SerializeField] private float snapSpeed = 8f; // Increased from 5f
    [SerializeField] private float snapAngleThreshold = 10f; // Reduced from 15f

    [Header("Inversion Settings")]
    [SerializeField] private bool invertXAxis = false;
    [SerializeField] private bool invertYAxis = false;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    private Vector3 _lastMousePosition;
    private Quaternion _targetRotation;
    private Quaternion _alignmentTarget;
    private bool _isRotating;
    private float _idleTimer;
    private bool _isAligning;
    private Quaternion[] _cubeRotations;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        _targetRotation = transform.rotation;
        _alignmentTarget = _targetRotation;
        _cubeRotations = GenerateAllCubeRotations();
    }

    private void Update()
    {
        HandleRotationInput();

        if (enableAutoAlign && !_isRotating)
            HandleAutoAlignment();

        ApplyRotation();
    }

    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _lastMousePosition = Input.mousePosition;
            _isRotating = true;
            _isAligning = false;
            _idleTimer = 0f;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isRotating = false;
        }

        if (_isRotating && Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            _lastMousePosition = Input.mousePosition;

            Vector3 cameraRight = mainCamera.transform.right;
            Vector3 cameraUp = mainCamera.transform.up;

            float xRotation = delta.y * rotationSpeed * (invertYAxis ? 1 : -1);
            float yRotation = delta.x * rotationSpeed * (invertXAxis ? -1 : 1);

            Quaternion rotationX = Quaternion.AngleAxis(xRotation, cameraRight);
            Quaternion rotationY = Quaternion.AngleAxis(yRotation, cameraUp);

            _targetRotation = rotationY * rotationX * _targetRotation;
            _alignmentTarget = _targetRotation;
        }
    }

    private void HandleAutoAlignment()
    {
        _idleTimer += Time.deltaTime;

        if (_idleTimer > alignDelay)
        {
            if (!_isAligning)
            {
                _alignmentTarget = FindBestAlignment(_targetRotation);
                _isAligning = true;
            }

            float angleToTarget = Quaternion.Angle(_targetRotation, _alignmentTarget);
            float speed = angleToTarget > snapAngleThreshold ? alignApproachSpeed : snapSpeed;

            _targetRotation = Quaternion.RotateTowards(_targetRotation, _alignmentTarget, speed * Time.deltaTime * angleToTarget);

            if (angleToTarget < 0.1f)
            {
                _targetRotation = _alignmentTarget;
                _isAligning = false;
            }
        }
    }

    private Quaternion FindBestAlignment(Quaternion currentRotation)
    {
        Quaternion bestRotation = currentRotation;
        float smallestAngle = float.MaxValue;

        foreach (Quaternion rot in _cubeRotations)
        {
            float angle = Quaternion.Angle(currentRotation, rot);
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestRotation = rot;
            }
        }

        return bestRotation;
    }

    private Quaternion[] GenerateAllCubeRotations()
    {
        Quaternion[] rotations = new Quaternion[24];
        int index = 0;

        Vector3[] directions = {
            Vector3.up, Vector3.down,
            Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back
        };

        foreach (Vector3 up in directions)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                Vector3 forward = GetPerpendicularForward(up);
                Quaternion rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(0, rot * 90, 0);
                rotations[index++] = rotation;
            }
        }

        return rotations;
    }

    private Vector3 GetPerpendicularForward(Vector3 up)
    {
        if (Mathf.Abs(up.y) > 0.9f) // Up or down
            return Vector3.forward;
        else if (Mathf.Abs(up.z) > 0.9f) // Forward or back
            return Vector3.up;
        else // Left or right
            return Vector3.up;
    }

    private void ApplyRotation()
    {
        transform.rotation = smoothRotation ?
            Quaternion.Slerp(transform.rotation, _targetRotation, smoothFactor * Time.deltaTime) :
            _targetRotation;
    }
}
