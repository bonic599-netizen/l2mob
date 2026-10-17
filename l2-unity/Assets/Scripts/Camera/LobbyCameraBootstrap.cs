using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures that when the scene (e.g. Menu or l2_lobby) is played:
/// 1. If started directly from l2_lobby without GameManager, boots Menu scene so LoginWindow, Pawns, Datatables, and Networking load.
/// 2. Black quad planes (LoginBG, LoginBG2) and unbaked probe volumes are deactivated.
/// 3. Directional sunlight and ambient lighting are properly configured.
/// 4. The lobby cameras and interactive controls are active and ready.
/// </summary>
public class LobbyCameraBootstrap : MonoBehaviour
{
    private static bool _hookedSceneLoaded = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeLobbyCamera()
    {
        if (!_hookedSceneLoaded)
        {
            _hookedSceneLoaded = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // If user pressed Play directly in l2_lobby.unity, boot Menu scene so Login UI, Datatables, Pawns, and Networking load
        if (GameManager.Instance == null && SceneManager.GetActiveScene().name == "l2_lobby")
        {
            if (Application.CanStreamedLevelBeLoaded("Menu"))
            {
                Debug.Log("[LobbyCameraBootstrap] Booting 'Menu' scene to initialize Login UI, Character Pawns, Datatables, and Hellsing Networking...");
                SceneManager.LoadScene("Menu");
                return;
            }
        }

        ApplyLobbySceneFixes();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "l2_lobby" || scene.name == "Menu")
        {
            ApplyLobbySceneFixes();
        }
        else
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.75f, 0.75f, 0.78f);
            RenderSettings.ambientSkyColor = new Color(0.75f, 0.75f, 0.78f);
            RenderSettings.ambientIntensity = 1.2f;
            FixRenderersAndMaterials();
        }
    }

    public static void ApplyLobbySceneFixes()
    {
        // 1. Deactivate blocking background quads
        GameObject bg1 = GameObject.Find("LoginBG");
        if (bg1 != null)
        {
            bg1.SetActive(false);
            Debug.Log("[LobbyCameraBootstrap] Deactivated LoginBG quad.");
        }
        GameObject bg2 = GameObject.Find("LoginBG2");
        if (bg2 != null)
        {
            bg2.SetActive(false);
            Debug.Log("[LobbyCameraBootstrap] Deactivated LoginBG2 quad.");
        }

        // 2. Deactivate unbaked Adaptive Probe Volumes that cause pitch black lighting
        GameObject apv = GameObject.Find("Adaptive Probe Volume (1)");
        if (apv != null)
        {
            apv.SetActive(false);
            Debug.Log("[LobbyCameraBootstrap] Deactivated unbaked Adaptive Probe Volume.");
        }
        GameObject pvData = GameObject.Find("ProbeVolumePerSceneData");
        if (pvData != null)
        {
            pvData.SetActive(false);
        }

        // 3. Ensure active directional sunlight exists
        Light dirLight = null;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional && l.enabled)
            {
                dirLight = l;
                break;
            }
        }
        if (dirLight == null)
        {
            GameObject sunGo = new GameObject("Lobby Sun Light");
            dirLight = sunGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1f, 0.96f, 0.90f);
            dirLight.intensity = 1.6f;
            dirLight.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Debug.Log("[LobbyCameraBootstrap] Created Directional Sun Light.");
        }
        RenderSettings.sun = dirLight;

        // 4. Configure bright, clean ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.75f, 0.75f, 0.78f);
        RenderSettings.ambientSkyColor = new Color(0.75f, 0.75f, 0.78f);
        RenderSettings.ambientIntensity = 1.2f;

        // 5. Find or create Camera container
        GameObject container = GameObject.Find("Cameras");
        if (container == null)
        {
            container = new GameObject("Cameras");
        }

        Camera activeCam = Camera.main;

        if (activeCam == null && LoginCameraManager.Instance == null)
        {
            Debug.Log("[LobbyCameraBootstrap] No active MainCamera found. Initializing Lobby cameras...");

            // Try to find existing "Login" or "CharSelect" camera
            GameObject loginCamGo = GameObject.Find("Login");
            if (loginCamGo == null)
            {
                loginCamGo = new GameObject("Login");
                loginCamGo.transform.SetParent(container.transform);
                loginCamGo.transform.position = new Vector3(4595.33f, -120f, -172.4f);
                loginCamGo.transform.rotation = Quaternion.Euler(0f, -140f, 0f);
            }

            activeCam = loginCamGo.GetComponent<Camera>();
            if (activeCam == null)
            {
                activeCam = loginCamGo.AddComponent<Camera>();
            }

            activeCam.tag = "MainCamera";
            activeCam.fieldOfView = 60f;
            activeCam.nearClipPlane = 0.3f;
            activeCam.farClipPlane = 20000f;
            activeCam.enabled = true;

            if (loginCamGo.GetComponent<AudioListener>() == null)
            {
                loginCamGo.AddComponent<AudioListener>();
            }

            // Create secondary cameras if missing
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

        // Set Skybox background on camera and only attach free-look controller when running standalone without LoginCameraManager
        if (activeCam != null)
        {
            activeCam.clearFlags = CameraClearFlags.Skybox;
            activeCam.backgroundColor = new Color(0.42f, 0.62f, 0.82f);

            if (GameManager.Instance == null && LoginCameraManager.Instance == null)
            {
                if (activeCam.GetComponent<LobbyCameraController>() == null)
                {
                    activeCam.gameObject.AddComponent<LobbyCameraController>();
                }
            }
            else
            {
                var ctrl = activeCam.GetComponent<LobbyCameraController>();
                if (ctrl != null)
                {
                    Object.Destroy(ctrl);
                }
            }
        }

        // 6. Runtime material and lightprobe safeguard
        FixRenderersAndMaterials();
    }

    private static void FixRenderersAndMaterials()
    {
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int fixedCount = 0;
        foreach (var r in renderers)
        {
            // Enable Spherical Harmonics ambient light sampling (with m_LightProbeSystem: 0 in URP)
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;

            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && (mats[i].shader == null || mats[i].shader.name == "Hidden/InternalErrorShader" || string.IsNullOrEmpty(mats[i].shader.name)))
                {
                    if (urpLitShader != null)
                    {
                        Texture existingTex = mats[i].HasProperty("_BaseMap") ? mats[i].GetTexture("_BaseMap") : null;
                        if (existingTex == null && mats[i].HasProperty("_MainTex"))
                        {
                            existingTex = mats[i].GetTexture("_MainTex");
                        }
                        mats[i].shader = urpLitShader;
                        if (existingTex != null)
                        {
                            mats[i].SetTexture("_BaseMap", existingTex);
                        }
                        fixedCount++;
                    }
                }
            }
        }
        if (fixedCount > 0)
        {
            Debug.Log($"[LobbyCameraBootstrap] Upgraded {fixedCount} error-shader materials to URP Lit while preserving textures.");
        }
    }
}

/// <summary>
/// Free-look / inspection controller for the lobby camera.
/// Supports keyboard (WASD), mouse dragging, mouse wheel zoom, and numeric keys (1-7) to teleport to character locations.
/// </summary>
public class LobbyCameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 35f;
    [SerializeField] private float _lookSpeed = 3f;
    [SerializeField] private float _zoomSpeed = 15f;

    private float _pitch = 0f;
    private float _yaw = -140f;
    private Vector2 _lastPointerPosition;
    private bool _isDragging = false;

    // Viewpoint bookmarks
    private static readonly Vector3 LoginPos = new Vector3(4595.33f, -122.27f, -172.41f);
    private static readonly Vector3 CharSelectPos = new Vector3(4540.89f, -118.02f, -317.32f);
    private static readonly Vector3 HumanPos = new Vector3(4642.45f, -121.33f, -281.87f);
    private static readonly Vector3 ElfPos = new Vector3(4705.32f, -122.06f, -165.05f);
    private static readonly Vector3 DarkElfPos = new Vector3(4765.33f, -122.40f, -216.59f);
    private static readonly Vector3 OrcPos = new Vector3(4790.28f, -114.52f, -388.61f);
    private static readonly Vector3 DwarfPos = new Vector3(4578.14f, -120.28f, -418.07f);

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _pitch = angles.x;
        _yaw = angles.y;
    }

    private void Update()
    {
        HandleHotkeys();
        HandleKeyboardMovement();
        HandleMouseAndTouchRotation();
    }

    private void HandleHotkeys()
    {
        // Jump between viewpoints
        if (Input.GetKeyDown(KeyCode.Alpha1)) JumpTo(LoginPos, 0f, -140f, "Login View");
        if (Input.GetKeyDown(KeyCode.Alpha2)) JumpTo(CharSelectPos, 0f, -20f, "CharSelect Courtyard");
        if (Input.GetKeyDown(KeyCode.Alpha3)) JumpTo(HumanPos, 0f, -90f, "Human Spot");
        if (Input.GetKeyDown(KeyCode.Alpha4)) JumpTo(ElfPos, 0f, 150f, "Elf Spot");
        if (Input.GetKeyDown(KeyCode.Alpha5)) JumpTo(DarkElfPos, 0f, 100f, "DarkElf Spot");
        if (Input.GetKeyDown(KeyCode.Alpha6)) JumpTo(OrcPos, 0f, 40f, "Orc Spot");
        if (Input.GetKeyDown(KeyCode.Alpha7)) JumpTo(DwarfPos, 0f, -45f, "Dwarf Spot");
    }

    private void JumpTo(Vector3 pos, float pitch, float yaw, string viewName)
    {
        transform.position = pos;
        _pitch = pitch;
        _yaw = yaw;
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Debug.Log($"[LobbyCameraController] Teleported to: {viewName}");
    }

    private void HandleKeyboardMovement()
    {
        float speed = _moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= 3f;
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
