using UnityEngine;
using System;

public class CubeRotateLock : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float smoothFactor = 10f;

    [Header("Click Alignment Settings")]
    [SerializeField] private float clickAlignSpeed = 8f;
    [SerializeField] private float clickSnapOvershoot = 0.1f;
    [SerializeField] private float clickSnapRecovery = 3f;

    [Header("Idle Alignment Settings")]
    [SerializeField] private float idleAlignDelay = 2f;
    [SerializeField] private float idleAlignSpeed = 3f;
    [SerializeField] private float idleSnapThreshold = 10f;

    [Header("Inversion Settings")]
    [SerializeField] private bool invertXAxis = false;
    [SerializeField] private bool invertYAxis = false;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask cubeLayer;

    public enum CubeFace { Front, Back, Left, Right, Top, Bottom, None }

    // Rotation state
    private Quaternion _targetRotation;
    private Quaternion _lockedFaceTarget;
    private bool _isFaceLocked;
    private bool _wasFaceLocked;
    private bool _isRightMouseHeld;
    private bool _rightClickConsumed;
    private Vector3 _lastMousePosition;
    private CubeFace _lockedFace;

    // Alignment timing
    private float _idleTimer;
    private float _snapProgress;
    private bool _isSnapping;

    // Events
    public System.Action<CubeFace> OnFaceLocked;
    public System.Action OnFaceUnlocked;

    private void OnEnable()
    {
        OnFaceLocked += HandleFaceLocked;
    }

    private void OnDisable()
    {
        OnFaceLocked -= HandleFaceLocked;
    }

    private void HandleFaceLocked(CubeFace face)
    {
        switch (face)
        {
            case CubeFace.Front:
                FrontFaceFunction();
                break;
            case CubeFace.Back:
                BackFaceFunction();
                break;
            case CubeFace.Left:
                //LeftFaceFunction();
                break;
            case CubeFace.Right:
                //RightFaceFunction();
                break;
            case CubeFace.Top:
                //TopFaceFunction();
                break;
            case CubeFace.Bottom:
                //BottomFaceFunction();
                break;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        _targetRotation = transform.rotation;
        _lockedFace = CubeFace.None;
    }

    private void Update()
    {
        HandleMouseInput();
        UpdateAlignment();
        CheckLockStateChange();
        ApplyRotation();
    }

    private void HandleMouseInput()
    {
        // Right click handling
        if (Input.GetMouseButtonDown(1))
        {
            _isRightMouseHeld = true;
            _rightClickConsumed = false;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isRightMouseHeld = false;
            _rightClickConsumed = false;
        }

        // Left click to lock face (works even while right mouse is held)
        if (Input.GetMouseButtonDown(0))
        {
            TryLockFace();
        }

        // Handle rotation only if:
        // 1. Right mouse is held
        // 2. Not locked (or if locked, this is the first frame of right click)
        // 3. Right click hasn't been "consumed" yet
        if (_isRightMouseHeld && (!_isFaceLocked || !_rightClickConsumed))
        {
            if (_isFaceLocked)
            {
                // This right click will unlock
                _isFaceLocked = false;
                _rightClickConsumed = true;
            }
            else
            {
                // Normal rotation
                RotateCube();
            }
        }
    }

    private void TryLockFace()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubeLayer))
        {
            Vector3 localNormal = transform.InverseTransformDirection(hit.normal).normalized;

            float absX = Mathf.Abs(localNormal.x);
            float absY = Mathf.Abs(localNormal.y);
            float absZ = Mathf.Abs(localNormal.z);

            if (absX > absY && absX > absZ)
            {
                _lockedFace = localNormal.x > 0 ? CubeFace.Right : CubeFace.Left;
            }
            else if (absY > absX && absY > absZ)
            {
                _lockedFace = localNormal.y > 0 ? CubeFace.Top : CubeFace.Bottom;
            }
            else
            {
                _lockedFace = localNormal.z > 0 ? CubeFace.Front : CubeFace.Back;
            }

            Quaternion alignRotation = Quaternion.FromToRotation(hit.normal, -mainCamera.transform.forward) * transform.rotation;

            _lockedFaceTarget = SnapTo90Degrees(alignRotation);
            _isFaceLocked = true;
            _isSnapping = true;
            _snapProgress = 0f;
            _idleTimer = 0f;
        }
    }

    private void CheckLockStateChange()
    {
        if (_isFaceLocked != _wasFaceLocked)
        {
            if (_isFaceLocked)
            {
                Debug.Log("LOCKED IN to face: " + _lockedFace);
                OnFaceLocked?.Invoke(_lockedFace);
            }
            else
            {
                Debug.Log("LOCKED OUT");
                OnFaceUnlocked?.Invoke();
                _lockedFace = CubeFace.None;
            }
            _wasFaceLocked = _isFaceLocked;
        }
    }

    private void UpdateAlignment()
    {
        if (_isFaceLocked && _isSnapping)
        {
            UpdateClickAlignment();
        }
        else if (!_isFaceLocked && !_isRightMouseHeld && _idleTimer > idleAlignDelay)
        {
            UpdateIdleAlignment();
        }
    }

    private void UpdateClickAlignment()
    {
        float angleToTarget = Quaternion.Angle(transform.rotation, _lockedFaceTarget);

        _snapProgress += clickAlignSpeed * Time.deltaTime;
        float snapEase = Mathf.SmoothStep(0, 1 + clickSnapOvershoot, _snapProgress);

        if (snapEase > 1f)
        {
            float overshoot = snapEase - 1f;
            snapEase = 1f + (overshoot * Mathf.Exp(-clickSnapRecovery * (_snapProgress - 1f)));
        }

        _targetRotation = Quaternion.Slerp(transform.rotation, _lockedFaceTarget, Mathf.Clamp01(snapEase));

        if (_snapProgress >= 1f && angleToTarget < 0.5f)
        {
            _targetRotation = _lockedFaceTarget;
            _isSnapping = false;
        }
    }

    private void UpdateIdleAlignment()
    {
        Quaternion nearestRotation = FindNearestCubeRotation(_targetRotation);
        float angleToAlign = Quaternion.Angle(_targetRotation, nearestRotation);

        if (angleToAlign > idleSnapThreshold)
        {
            _targetRotation = Quaternion.RotateTowards(
                _targetRotation,
                nearestRotation,
                idleAlignSpeed * Time.deltaTime * angleToAlign
            );
        }
        else
        {
            _targetRotation = Quaternion.Slerp(
                _targetRotation,
                nearestRotation,
                idleAlignSpeed * 2f * Time.deltaTime
            );
        }
    }

    private Quaternion FindNearestCubeRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        return Quaternion.Euler(euler);
    }

    private Quaternion SnapTo90Degrees(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        return Quaternion.Euler(euler);
    }

    private void RotateCube()
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
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            smoothFactor * Time.deltaTime
        );
    }

    //puzzle functions

    private void FrontFaceFunction()
    {
        Debug.Log("Front face action!");
        //front face logic
    }

    private void BackFaceFunction()
    {
        Debug.Log("Back face action!");
        //back face logic
    }
}
