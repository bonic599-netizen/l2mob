using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Master Touch & Gesture Controller for Lineage 2 Mobile Client (iOS).
/// Handles Mobile Joystick input, Camera rotation/pinch-zoom, and touch targeting.
/// </summary>
public class MobileTouchController : MonoBehaviour
{
    public static MobileTouchController Instance { get; private set; }

    [Header("Joystick Reference")]
    [SerializeField] private MobileJoystick _joystick;

    [Header("Camera Control Settings")]
    [SerializeField] private float _touchOrbitSensitivityX = 0.15f;
    [SerializeField] private float _touchOrbitSensitivityY = 0.12f;
    [SerializeField] private float _pinchZoomSensitivity = 0.02f;

    [Header("Movement Interval")]
    [SerializeField] private float _movePacketInterval = 0.15f; // sends move packet every 150ms while joystick held

    private Camera _mainCamera;
    private float _lastMovePacketTime = 0f;
    private int _cameraTouchId = -1;
    private Vector2 _lastTouchPosition;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_joystick == null)
            _joystick = FindFirstObjectByType<MobileJoystick>();
    }

    private void Update()
    {
        HandleJoystickMovement();
        HandleTouchGestures();
    }

    /// <summary>
    /// Translates joystick direction into L2 character movement
    /// </summary>
    private void HandleJoystickMovement()
    {
        if (_joystick == null || !_joystick.IsPressed)
            return;

        Vector2 joyDir = _joystick.Direction;
        if (joyDir.sqrMagnitude < 0.05f)
            return;

        // Rate-limit network packets to avoid flooding gameserver
        if (Time.time - _lastMovePacketTime < _movePacketInterval)
            return;

        _lastMovePacketTime = Time.time;

        // Calculate direction relative to current camera orientation
        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camRight * joyDir.x + camForward * joyDir.y).normalized;

        if (PlayerStateMachine.Instance != null && PlayerStateMachine.Instance.transform != null)
        {
            Vector3 currentPos = PlayerStateMachine.Instance.transform.position;
            Vector3 targetPos = currentPos + moveDir * 150f; // step 150 units forward

            // Use native L2 pathfinding / move event
            PlayerStateMachine.Instance.NotifyEvent(Event.CLICK_TO_MOVE, targetPos);
        }
    }

    /// <summary>
    /// Handles mobile screen gestures on right half of screen (Camera pan & pinch zoom)
    /// </summary>
    private void HandleTouchGestures()
    {
        // 2-finger pinch zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevDistance = (t0Prev - t1Prev).magnitude;
            float currentDistance = (t0.position - t1.position).magnitude;
            float deltaDistance = currentDistance - prevDistance;

            if (CameraController.Instance != null)
            {
                // Adjust camera zoom
                CameraController.Instance.transform.Translate(Vector3.forward * (deltaDistance * _pinchZoomSensitivity));
            }
            return;
        }

        // 1-finger camera orbit (on right half of screen, avoiding UI)
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // Check if touch is on right 50% of the screen
            if (t.position.x > Screen.width * 0.45f)
            {
                // Don't intercept UI touches
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    continue;

                if (t.phase == TouchPhase.Began)
                {
                    _cameraTouchId = t.fingerId;
                    _lastTouchPosition = t.position;
                }
                else if (t.fingerId == _cameraTouchId && t.phase == TouchPhase.Moved)
                {
                    Vector2 delta = t.position - _lastTouchPosition;
                    _lastTouchPosition = t.position;

                    if (CameraController.Instance != null)
                    {
                        float yaw = delta.x * _touchOrbitSensitivityX;
                        float pitch = -delta.y * _touchOrbitSensitivityY;
                        CameraController.Instance.transform.RotateAround(
                            PlayerStateMachine.Instance.transform.position,
                            Vector3.up,
                            yaw
                        );
                    }
                }
                else if (t.fingerId == _cameraTouchId && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
                {
                    _cameraTouchId = -1;
                }
            }
        }
    }
}
