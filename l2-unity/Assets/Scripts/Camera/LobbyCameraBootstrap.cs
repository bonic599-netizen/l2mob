using UnityEngine;

/// <summary>
/// Ensures that when the scene (e.g. l2_lobby) is played without pre-existing cameras,
/// the lobby cameras and MainCamera are created automatically.
/// </summary>
public class LobbyCameraBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeLobbyCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        Debug.Log("[LobbyCameraBootstrap] No active MainCamera found. Initializing Lobby cameras...");

        // Find or create "Cameras" container
        GameObject container = GameObject.Find("Cameras");
        if (container == null)
        {
            container = new GameObject("Cameras");
        }

        // 1. Create Login (Main) Camera
        GameObject loginCamGo = GameObject.Find("Login");
        if (loginCamGo == null)
        {
            loginCamGo = new GameObject("Login");
            loginCamGo.transform.SetParent(container.transform);
        }

        // Position camera in front of lobby background facing the scene
        loginCamGo.transform.position = new Vector3(4595.33f, -120f, -172.4f);
        loginCamGo.transform.rotation = Quaternion.Euler(0f, -140f, 0f);

        Camera cam = loginCamGo.GetComponent<Camera>();
        if (cam == null)
        {
            cam = loginCamGo.AddComponent<Camera>();
        }

        cam.tag = "MainCamera";
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 20000f;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.enabled = true;

        if (loginCamGo.GetComponent<AudioListener>() == null)
        {
            loginCamGo.AddComponent<AudioListener>();
        }

        // Add interactive camera controls (WASD + Mouse/Touch)
        if (loginCamGo.GetComponent<LobbyCameraController>() == null)
        {
            loginCamGo.AddComponent<LobbyCameraController>();
        }

        // 2. Create secondary cameras for LoginCameraManager race selection
        string[] raceCameras = new string[] { "CharSelect", "Human", "Elf", "DarkElf", "Orc", "Dwarf" };
        foreach (string raceName in raceCameras)
        {
            GameObject raceGo = GameObject.Find(raceName);
            if (raceGo == null)
            {
                raceGo = new GameObject(raceName);
                raceGo.transform.SetParent(container.transform);
                raceGo.transform.position = loginCamGo.transform.position;
                raceGo.transform.rotation = loginCamGo.transform.rotation;
                Camera raceCam = raceGo.AddComponent<Camera>();
                raceCam.enabled = false;
                raceCam.fieldOfView = 60f;
            }
        }

        Debug.Log("[LobbyCameraBootstrap] Lobby MainCamera and race cameras initialized successfully.");
    }
}

/// <summary>
/// Free-look / inspection controller for the lobby camera.
/// Supports keyboard (WASD), mouse dragging, mouse wheel zoom, and mobile touch drag.
/// </summary>
public class LobbyCameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 30f;
    [SerializeField] private float _lookSpeed = 3f;
    [SerializeField] private float _zoomSpeed = 10f;

    private float _pitch = 0f;
    private float _yaw = -140f;
    private Vector2 _lastPointerPosition;
    private bool _isDragging = false;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _pitch = angles.x;
        _yaw = angles.y;
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseAndTouchRotation();
    }

    private void HandleKeyboardMovement()
    {
        float speed = _moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= 2.5f;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;

        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
        {
            move += Vector3.up;
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
        {
            move += Vector3.down;
        }

        if (move.sqrMagnitude > 0.001f)
        {
            transform.position += move.normalized * (speed * Time.deltaTime);
        }

        // Mouse scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * (scroll * _zoomSpeed);
        }
    }

    private void HandleMouseAndTouchRotation()
    {
        // Right mouse button drag or touch drag
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastPointerPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        if (_isDragging && (Input.GetMouseButton(1) || (Input.touchCount > 0 && Input.GetMouseButton(0))))
        {
            Vector2 currentPos = (Vector2)Input.mousePosition;
            Vector2 delta = currentPos - _lastPointerPosition;
            _lastPointerPosition = currentPos;

            _yaw += delta.x * _lookSpeed * 0.1f;
            _pitch -= delta.y * _lookSpeed * 0.1f;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}
