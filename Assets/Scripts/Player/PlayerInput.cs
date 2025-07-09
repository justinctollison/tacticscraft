using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private CinemachineCamera _cineMachineCamera;
    [SerializeField] private CameraConfig _cameraConfig;

    private CinemachineFollow _cineMachineFollow;
    private Vector3 _startingFollowOffset;
    private float _zoomStartTime;
    private float _rotationStartTime;
    private float _maxRotationAmount;

    private void Awake()
    {
        if (!_cineMachineCamera.TryGetComponent<CinemachineFollow>(out _cineMachineFollow))
        {
            Debug.LogError("Cinemachine Camera is missing CinemachineFollow.");
        }

        _startingFollowOffset = _cineMachineFollow.FollowOffset;
        _maxRotationAmount = Mathf.Abs(_cineMachineFollow.FollowOffset.z);
    }

    private void Update()
    {
        HandlePanning();
        HandleZooming();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (ShouldSetRotationStartTime())
        {
            _rotationStartTime = Time.time;
        }

        float rotationTime = Mathf.Clamp01((Time.time - _rotationStartTime) * _cameraConfig.RotationSpeed);
        Vector3 targetOffset;

        if (Keyboard.current.pageDownKey.isPressed)
        {
            targetOffset = new Vector3(_maxRotationAmount, _cineMachineFollow.FollowOffset.y, 0);
        }
        else if (Keyboard.current.pageUpKey.isPressed)
        {
            targetOffset = new Vector3(-_maxRotationAmount, _cineMachineFollow.FollowOffset.y, 0);
        }
        else
        {
            targetOffset = new Vector3(_startingFollowOffset.x, _cineMachineFollow.FollowOffset.y, _startingFollowOffset.z);
        }

        _cineMachineFollow.FollowOffset = Vector3.Slerp(_cineMachineFollow.FollowOffset, targetOffset, rotationTime);
    }

    private bool ShouldSetRotationStartTime()
    {
        return Keyboard.current.pageDownKey.wasPressedThisFrame || Keyboard.current.pageUpKey.wasPressedThisFrame ||
            Keyboard.current.pageDownKey.wasReleasedThisFrame || Keyboard.current.pageUpKey.wasReleasedThisFrame;
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomStartTime())
        {
            _zoomStartTime = Time.time;
        }

        float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);
        Vector3 targetFollowOffset;

        if (Keyboard.current.endKey.isPressed)
        {
            targetFollowOffset = new Vector3(_cineMachineFollow.FollowOffset.x, _cameraConfig.MinZoomDistance, _cineMachineFollow.FollowOffset.z);
        }
        else
        {
            targetFollowOffset = new Vector3(_cineMachineFollow.FollowOffset.x, _startingFollowOffset.y, _cineMachineFollow.FollowOffset.z);
        }

        _cineMachineFollow.FollowOffset = Vector3.Slerp(_cineMachineFollow.FollowOffset, targetFollowOffset, zoomTime);
    }

    private bool ShouldSetZoomStartTime()
    {
        return Keyboard.current.endKey.wasPressedThisFrame || Keyboard.current.endKey.wasReleasedThisFrame;
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = GetKeyboardMoveAmount();
        moveAmount += GetMouseMoveAmount();

        moveAmount *= Time.deltaTime;
        _cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
    }

    private Vector2 GetMouseMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (!_cameraConfig.EnableEdgePan) { return moveAmount; }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        if (mousePosition.x <= _cameraConfig.EdgePanSize)
        {
            moveAmount.x -= _cameraConfig.MousePanSpeed;
        }
        else if (mousePosition.x >= screenWidth - _cameraConfig.EdgePanSize)
        {
            moveAmount.x += _cameraConfig.MousePanSpeed;

        }

        if (mousePosition.y >= screenHeight - _cameraConfig.EdgePanSize)
        {
            moveAmount.y += _cameraConfig.MousePanSpeed;
        }
        else if (mousePosition.y <= _cameraConfig.EdgePanSize)
        {
            moveAmount.y -= _cameraConfig.MousePanSpeed;
        }

        return moveAmount;
    }

    private Vector2 GetKeyboardMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += _cameraConfig.KeyboardPanSpeed;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= _cameraConfig.KeyboardPanSpeed;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= _cameraConfig.KeyboardPanSpeed;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += _cameraConfig.KeyboardPanSpeed;
        }

        return moveAmount;
    }
}
