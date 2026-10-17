using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapWindow : L2PopupWindow
{
    private static MapWindow _instance;
    public static MapWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                if (L2GameUI.Instance != null)
                {
                    _instance = L2GameUI.Instance.gameObject.AddComponent<MapWindow>();
                }
            }
            return _instance;
        }
    }

    private Texture2D _mapTexture;
    private VisualElement _mapContainer;
    private VisualElement _mapImage;
    private VisualElement _pinsLayer;
    private VisualElement _playerPin;
    private Label _playerCoordsLabel;
    private Label _currentRegionLabel;
    private TextField _searchField;
    private ScrollView _locationsScrollView;
    private DropdownField _categoryDropdown;

    private float _zoomLevel = 1.0f;
    private Vector2 _mapPanOffset = Vector2.zero;
    private bool _isDraggingMap = false;
    private Vector2 _lastMouseDragPos;

    private const float L2_MIN_X = -135000f;
    private const float L2_MAX_X = 185000f;
    private const float L2_MIN_Y = -95000f;
    private const float L2_MAX_Y = 265000f;

    private int _selectedCategoryId = 0;
    private string _searchFilter = "";
    private LocationPoint _selectedLocation = null;
    private readonly List<VisualElement> _activeLocationPins = new List<VisualElement>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected override void LoadAssets()
    {
        _mapTexture = Resources.Load<Texture2D>("Data/UI/Assets/Map/AdenWorldMap");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        _root = root;
        _isWindowHidden = true;

        if (_mapTexture == null)
        {
            _mapTexture = Resources.Load<Texture2D>("Data/UI/Assets/Map/AdenWorldMap");
        }

        // Build main window element
        _windowEle = new VisualElement();
        _windowEle.name = "MapWindow";
        _windowEle.style.position = Position.Absolute;
        _windowEle.style.width = 960;
        _windowEle.style.height = 640;
        _windowEle.style.left = new Length(50, LengthUnit.Percent);
        _windowEle.style.top = new Length(50, LengthUnit.Percent);
        _windowEle.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent)));
        _windowEle.style.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 0.94f);
        _windowEle.style.borderTopColor = new Color(0.72f, 0.58f, 0.32f, 0.9f);
        _windowEle.style.borderBottomColor = new Color(0.40f, 0.32f, 0.18f, 0.9f);
        _windowEle.style.borderLeftColor = new Color(0.55f, 0.44f, 0.24f, 0.9f);
        _windowEle.style.borderRightColor = new Color(0.55f, 0.44f, 0.24f, 0.9f);
        _windowEle.style.borderTopWidth = 2;
        _windowEle.style.borderBottomWidth = 2;
        _windowEle.style.borderLeftWidth = 2;
        _windowEle.style.borderRightWidth = 2;
        _windowEle.style.borderTopLeftRadius = 4;
        _windowEle.style.borderTopRightRadius = 4;
        _windowEle.style.borderBottomLeftRadius = 4;
        _windowEle.style.borderBottomRightRadius = 4;
        _windowEle.style.flexDirection = FlexDirection.Column;

        _mouseOverDetection = new MouseOverDetectionManipulator(_windowEle);
        _windowEle.AddManipulator(_mouseOverDetection);

        // Header
        VisualElement header = new VisualElement();
        header.style.height = 34;
        header.style.backgroundColor = new Color(0.14f, 0.13f, 0.15f, 1f);
        header.style.borderBottomWidth = 1;
        header.style.borderBottomColor = new Color(0.6f, 0.48f, 0.25f, 0.8f);
        header.style.flexDirection = FlexDirection.Row;
        header.style.alignItems = Align.Center;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.paddingLeft = 12;
        header.style.paddingRight = 8;

        DragManipulator drag = new DragManipulator(header, _windowEle, this);
        header.AddManipulator(drag);

        Label title = new Label("World Map - Continent of Aden");
        title.style.color = new Color(0.95f, 0.82f, 0.45f);
        title.style.fontSize = 14;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.Add(title);

        Button closeBtn = new Button(() => {
            AudioManager.Instance?.PlayUISound("window_close");
            HideWindow(false);
        });
        closeBtn.text = "X";
        closeBtn.style.width = 24;
        closeBtn.style.height = 20;
        closeBtn.style.color = Color.white;
        closeBtn.style.backgroundColor = new Color(0.4f, 0.1f, 0.1f, 0.8f);
        closeBtn.style.borderTopWidth = 1;
        closeBtn.style.borderBottomWidth = 1;
        closeBtn.style.borderLeftWidth = 1;
        closeBtn.style.borderRightWidth = 1;
        closeBtn.style.borderTopColor = new Color(0.7f, 0.2f, 0.2f);
        closeBtn.style.borderBottomColor = new Color(0.2f, 0.05f, 0.05f);
        closeBtn.style.borderLeftColor = new Color(0.5f, 0.15f, 0.15f);
        closeBtn.style.borderRightColor = new Color(0.5f, 0.15f, 0.15f);
        header.Add(closeBtn);
        _windowEle.Add(header);

        // Body: Row with Map on left and List on right
        VisualElement body = new VisualElement();
        body.style.flexDirection = FlexDirection.Row;
        body.style.flexGrow = 1;
        _windowEle.Add(body);

        // Left: Map Viewport
        VisualElement mapViewport = new VisualElement();
        mapViewport.style.width = 600;
        mapViewport.style.flexGrow = 0;
        mapViewport.style.overflow = Overflow.Hidden;
        mapViewport.style.position = Position.Relative;
        mapViewport.style.backgroundColor = new Color(0.04f, 0.04f, 0.05f);

        _mapContainer = new VisualElement();
        _mapContainer.style.width = 600;
        _mapContainer.style.height = 600;
        _mapContainer.style.position = Position.Absolute;
        _mapContainer.style.left = 0;
        _mapContainer.style.top = 0;

        _mapImage = new VisualElement();
        _mapImage.style.width = 600;
        _mapImage.style.height = 600;
        _mapImage.style.position = Position.Absolute;
        if (_mapTexture != null)
        {
            _mapImage.style.backgroundImage = new StyleBackground(_mapTexture);
        }
        _mapContainer.Add(_mapImage);

        _pinsLayer = new VisualElement();
        _pinsLayer.style.width = 600;
        _pinsLayer.style.height = 600;
        _pinsLayer.style.position = Position.Absolute;
        _mapContainer.Add(_pinsLayer);

        // Player pin
        _playerPin = new VisualElement();
        _playerPin.style.position = Position.Absolute;
        _playerPin.style.width = 16;
        _playerPin.style.height = 16;
        _playerPin.style.borderTopLeftRadius = 8;
        _playerPin.style.borderTopRightRadius = 8;
        _playerPin.style.borderBottomLeftRadius = 8;
        _playerPin.style.borderBottomRightRadius = 8;
        _playerPin.style.backgroundColor = new Color(0.1f, 0.85f, 0.2f, 0.95f);
        _playerPin.style.borderTopWidth = 2;
        _playerPin.style.borderBottomWidth = 2;
        _playerPin.style.borderLeftWidth = 2;
        _playerPin.style.borderRightWidth = 2;
        _playerPin.style.borderTopColor = Color.white;
        _playerPin.style.borderBottomColor = Color.white;
        _playerPin.style.borderLeftColor = Color.white;
        _playerPin.style.borderRightColor = Color.white;

        Label playerTag = new Label("YOU");
        playerTag.style.position = Position.Absolute;
        playerTag.style.top = -14;
        playerTag.style.left = -6;
        playerTag.style.fontSize = 9;
        playerTag.style.color = new Color(0.2f, 1f, 0.3f);
        playerTag.style.unityFontStyleAndWeight = FontStyle.Bold;
        _playerPin.Add(playerTag);

        _pinsLayer.Add(_playerPin);
        mapViewport.Add(_mapContainer);

        // Zoom & Reset controls overlay
        VisualElement zoomControls = new VisualElement();
        zoomControls.style.position = Position.Absolute;
        zoomControls.style.top = 10;
        zoomControls.style.left = 10;
        zoomControls.style.flexDirection = FlexDirection.Row;

        Button zoomInBtn = new Button(() => AdjustZoom(0.25f)) { text = "+" };
        StyleMapControlButton(zoomInBtn);
        Button zoomOutBtn = new Button(() => AdjustZoom(-0.25f)) { text = "-" };
        StyleMapControlButton(zoomOutBtn);
        Button centerBtn = new Button(CenterOnPlayer) { text = "Center Player" };
        StyleMapControlButton(centerBtn, 90);

        zoomControls.Add(zoomInBtn);
        zoomControls.Add(zoomOutBtn);
        zoomControls.Add(centerBtn);
        mapViewport.Add(zoomControls);

        // Mouse pan and zoom callbacks
        mapViewport.RegisterCallback<WheelEvent>(evt => {
            AdjustZoom(evt.delta.y < 0 ? 0.2f : -0.2f);
            evt.StopPropagation();
        });

        mapViewport.RegisterCallback<MouseDownEvent>(evt => {
            if (evt.button == 0)
            {
                _isDraggingMap = true;
                _lastMouseDragPos = evt.mousePosition;
            }
        });

        mapViewport.RegisterCallback<MouseMoveEvent>(evt => {
            if (_isDraggingMap)
            {
                Vector2 delta = evt.mousePosition - _lastMouseDragPos;
                _lastMouseDragPos = evt.mousePosition;
                _mapPanOffset += delta;
                ApplyMapTransform();
            }
        });

        mapViewport.RegisterCallback<MouseUpEvent>(evt => {
            _isDraggingMap = false;
        });

        body.Add(mapViewport);

        // Right: Navigation Panel
        VisualElement navPanel = new VisualElement();
        navPanel.style.flexGrow = 1;
        navPanel.style.backgroundColor = new Color(0.10f, 0.10f, 0.12f, 0.95f);
        navPanel.style.borderLeftWidth = 1;
        navPanel.style.borderLeftColor = new Color(0.6f, 0.48f, 0.25f, 0.6f);
        navPanel.style.paddingTop = 8;
        navPanel.style.paddingBottom = 8;
        navPanel.style.paddingLeft = 8;
        navPanel.style.paddingRight = 8;
        navPanel.style.flexDirection = FlexDirection.Column;

        // Search Bar
        _searchField = new TextField();
        _searchField.label = "Search:";
        _searchField.style.marginBottom = 6;
        _searchField.style.color = Color.white;
        _searchField.RegisterValueChangedCallback(evt => {
            _searchFilter = evt.newValue != null ? evt.newValue.Trim().ToLowerInvariant() : "";
            RefreshLocationList();
        });
        navPanel.Add(_searchField);

        // Category Selector
        List<string> categoryChoices = new List<string>
        {
            "All Categories (All 14)",
            "1. Towns and Villages (19)",
            "2. Castles and Territories (18)",
            "3. Starting Locations (12)",
            "4. Gludio & Gludin (30)",
            "5. Dion & Floran (20)",
            "6. Giran Area (16)",
            "7. Oren & Hunters Area (25)",
            "8. Aden Area (29)",
            "9. Goddard Area (25)",
            "10. Rune Area (15)",
            "11. Schuttgart Area (12)",
            "12. Catacombs & Necropolises (14)",
            "13. Dungeons & Special Zones (21)",
            "14. Islands & Harbors (9)"
        };

        _categoryDropdown = new DropdownField("Category:", categoryChoices, 0);
        _categoryDropdown.style.marginBottom = 8;
        _categoryDropdown.RegisterValueChangedCallback(evt => {
            _selectedCategoryId = _categoryDropdown.index;
            RefreshLocationList();
            RefreshPinsOnMap();
        });
        navPanel.Add(_categoryDropdown);

        // ScrollView for location items
        _locationsScrollView = new ScrollView();
        _locationsScrollView.style.flexGrow = 1;
        _locationsScrollView.style.marginBottom = 6;
        navPanel.Add(_locationsScrollView);

        // Bottom Info Bar inside Navigation Panel
        VisualElement bottomInfo = new VisualElement();
        bottomInfo.style.borderTopWidth = 1;
        bottomInfo.style.borderTopColor = new Color(0.6f, 0.48f, 0.25f, 0.5f);
        bottomInfo.style.paddingTop = 6;
        bottomInfo.style.flexDirection = FlexDirection.Column;

        _currentRegionLabel = new Label("Region: Loading...");
        _currentRegionLabel.style.color = new Color(0.95f, 0.82f, 0.45f);
        _currentRegionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _currentRegionLabel.style.fontSize = 11;
        bottomInfo.Add(_currentRegionLabel);

        _playerCoordsLabel = new Label("Coords: X=0 Y=0 Z=0");
        _playerCoordsLabel.style.color = new Color(0.75f, 0.75f, 0.8f);
        _playerCoordsLabel.style.fontSize = 10;
        bottomInfo.Add(_playerCoordsLabel);

        navPanel.Add(bottomInfo);
        body.Add(navPanel);

        RegisterClickWindowEvent(_windowEle, header);
        root.Add(_windowEle);

        yield return new WaitForEndOfFrame();

        RefreshLocationList();
        RefreshPinsOnMap();
        CenterOnPlayer();

        L2GameUI.Instance.WindowLoadComplete();
    }

    private void StyleMapControlButton(Button btn, float width = 28)
    {
        btn.style.width = width;
        btn.style.height = 24;
        btn.style.marginRight = 4;
        btn.style.fontSize = 11;
        btn.style.color = Color.white;
        btn.style.backgroundColor = new Color(0.18f, 0.18f, 0.22f, 0.85f);
        btn.style.borderTopWidth = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1;
        btn.style.borderRightWidth = 1;
        btn.style.borderTopColor = new Color(0.6f, 0.5f, 0.3f);
        btn.style.borderBottomColor = new Color(0.2f, 0.15f, 0.1f);
        btn.style.borderLeftColor = new Color(0.4f, 0.3f, 0.2f);
        btn.style.borderRightColor = new Color(0.4f, 0.3f, 0.2f);
    }

    private void AdjustZoom(float delta)
    {
        _zoomLevel = Mathf.Clamp(_zoomLevel + delta, 0.75f, 3.5f);
        ApplyMapTransform();
    }

    private void ApplyMapTransform()
    {
        if (_mapContainer == null) return;
        _mapContainer.style.scale = new StyleScale(new Scale(new Vector2(_zoomLevel, _zoomLevel)));
        _mapContainer.style.translate = new StyleTranslate(new Translate(_mapPanOffset.x, _mapPanOffset.y));
    }

    public void CenterOnPlayer()
    {
        Vector3 pos = PlayerEntity.Instance != null ? PlayerEntity.Instance.transform.position : Vector3.zero;
        int l2X = Mathf.RoundToInt(pos.z * 52.5f);
        int l2Y = Mathf.RoundToInt(pos.x * 52.5f);

        Vector2 norm = GetNormalizedMapPosition(l2X, l2Y);
        float mapSize = 600f;
        Vector2 targetPinPos = new Vector2(norm.x * mapSize, norm.y * mapSize);

        // Center targetPinPos in the 600x600 viewport
        _mapPanOffset = new Vector2(mapSize * 0.5f - targetPinPos.x * _zoomLevel, mapSize * 0.5f - targetPinPos.y * _zoomLevel);
        ApplyMapTransform();
    }

    public void OpenCategory(int categoryId)
    {
        _selectedCategoryId = Mathf.Clamp(categoryId, 0, 14);
        if (_categoryDropdown != null)
        {
            _categoryDropdown.index = _selectedCategoryId;
        }
        RefreshLocationList();
        RefreshPinsOnMap();
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        AudioManager.Instance?.PlayUISound("map_open_01");
        L2GameUI.Instance?.WindowOpened(this);
        UpdatePlayerPin();
        CenterOnPlayer();
    }

    public override void HideWindow(bool silent)
    {
        if (!silent && !_isWindowHidden)
        {
            AudioManager.Instance?.PlayUISound("map_close_01");
        }
        base.HideWindow(silent);
    }

    public override void ToggleHideWindow()
    {
        if (_isWindowHidden)
        {
            ShowWindow();
        }
        else
        {
            HideWindow(false);
        }
    }

    private void Update()
    {
        if (!_isWindowHidden)
        {
            UpdatePlayerPin();
        }
    }

    private void UpdatePlayerPin()
    {
        if (_playerPin == null || _playerCoordsLabel == null) return;

        Vector3 pos = PlayerEntity.Instance != null ? PlayerEntity.Instance.transform.position : Vector3.zero;
        int l2X = Mathf.RoundToInt(pos.z * 52.5f);
        int l2Y = Mathf.RoundToInt(pos.x * 52.5f);
        int l2Z = Mathf.RoundToInt(pos.y * 52.5f);

        Vector2 norm = GetNormalizedMapPosition(l2X, l2Y);
        float mapSize = 600f;
        _playerPin.style.left = norm.x * mapSize - 8;
        _playerPin.style.top = norm.y * mapSize - 8;

        _playerCoordsLabel.text = $"Coords: X={l2X} Y={l2Y} Z={l2Z}";

        if (World.Instance != null && _currentRegionLabel != null)
        {
            _currentRegionLabel.text = $"Region: {World.Instance.GetRegionName(pos)}";
        }
    }

    private Vector2 GetNormalizedMapPosition(float l2X, float l2Y)
    {
        float u = Mathf.Clamp01((l2X - L2_MIN_X) / (L2_MAX_X - L2_MIN_X));
        float v = Mathf.Clamp01((l2Y - L2_MIN_Y) / (L2_MAX_Y - L2_MIN_Y));
        return new Vector2(u, v);
    }

    private void RefreshPinsOnMap()
    {
        if (_pinsLayer == null) return;

        // Remove old pins except player pin
        foreach (var pin in _activeLocationPins)
        {
            pin.RemoveFromHierarchy();
        }
        _activeLocationPins.Clear();

        List<LocationPoint> list = GetFilteredLocations();
        float mapSize = 600f;

        foreach (var loc in list)
        {
            Vector2 norm = GetNormalizedMapPosition(loc.L2Pos.x, loc.L2Pos.y);

            VisualElement pin = new VisualElement();
            pin.style.position = Position.Absolute;
            pin.style.left = norm.x * mapSize - 5;
            pin.style.top = norm.y * mapSize - 5;
            pin.style.width = 10;
            pin.style.height = 10;
            pin.style.borderTopLeftRadius = 5;
            pin.style.borderTopRightRadius = 5;
            pin.style.borderBottomLeftRadius = 5;
            pin.style.borderBottomRightRadius = 5;
            pin.style.backgroundColor = loc.IsTown ? new Color(0.95f, 0.75f, 0.2f, 0.9f) : new Color(0.2f, 0.6f, 1f, 0.85f);
            pin.style.borderTopWidth = 1;
            pin.style.borderBottomWidth = 1;
            pin.style.borderLeftWidth = 1;
            pin.style.borderRightWidth = 1;
            pin.style.borderTopColor = Color.white;
            pin.style.borderBottomColor = Color.white;
            pin.style.borderLeftColor = Color.white;
            pin.style.borderRightColor = Color.white;

            pin.RegisterCallback<ClickEvent>(evt => {
                SelectLocation(loc);
                evt.StopPropagation();
            });

            _pinsLayer.Add(pin);
            _activeLocationPins.Add(pin);
        }

        // Ensure player pin stays on top
        if (_playerPin != null)
        {
            _playerPin.BringToFront();
        }
    }

    private void RefreshLocationList()
    {
        if (_locationsScrollView == null) return;

        _locationsScrollView.Clear();
        List<LocationPoint> list = GetFilteredLocations();

        Vector3 playerPos = PlayerEntity.Instance != null ? PlayerEntity.Instance.transform.position : Vector3.zero;

        // Sort by distance from player
        list.Sort((a, b) => {
            float distA = Vector3.Distance(playerPos, a.UnityPos);
            float distB = Vector3.Distance(playerPos, b.UnityPos);
            return distA.CompareTo(distB);
        });

        foreach (var loc in list)
        {
            VisualElement card = new VisualElement();
            card.style.backgroundColor = (_selectedLocation == loc) ? new Color(0.28f, 0.25f, 0.18f, 0.9f) : new Color(0.14f, 0.14f, 0.16f, 0.85f);
            card.style.marginBottom = 4;
            card.style.paddingLeft = 8;
            card.style.paddingRight = 8;
            card.style.paddingTop = 6;
            card.style.paddingBottom = 6;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0.45f, 0.38f, 0.22f, 0.5f);
            card.style.borderBottomColor = new Color(0.3f, 0.24f, 0.15f, 0.5f);
            card.style.borderLeftColor = new Color(0.35f, 0.3f, 0.18f, 0.5f);
            card.style.borderRightColor = new Color(0.35f, 0.3f, 0.18f, 0.5f);
            card.style.flexDirection = FlexDirection.Column;

            // Header row with Name and Distance
            VisualElement row1 = new VisualElement();
            row1.style.flexDirection = FlexDirection.Row;
            row1.style.justifyContent = Justify.SpaceBetween;

            Label nameLabel = new Label(loc.Name);
            nameLabel.style.color = loc.IsTown ? new Color(1f, 0.84f, 0.35f) : Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.fontSize = 11;
            row1.Add(nameLabel);

            float distM = Vector3.Distance(playerPos, loc.UnityPos);
            string distText = distM > 1000f ? $"{(distM / 1000f):0.0} km" : $"{Mathf.RoundToInt(distM)} m";
            Label distLabel = new Label(distText);
            distLabel.style.color = new Color(0.65f, 0.85f, 0.65f);
            distLabel.style.fontSize = 10;
            row1.Add(distLabel);

            card.Add(row1);

            // Sub row with Category tag and Buttons
            VisualElement row2 = new VisualElement();
            row2.style.flexDirection = FlexDirection.Row;
            row2.style.justifyContent = Justify.SpaceBetween;
            row2.style.alignItems = Align.Center;
            row2.style.marginTop = 4;

            Label catLabel = new Label(loc.CategoryTitle);
            catLabel.style.color = new Color(0.6f, 0.6f, 0.65f);
            catLabel.style.fontSize = 9;
            row2.Add(catLabel);

            VisualElement btnGroup = new VisualElement();
            btnGroup.style.flexDirection = FlexDirection.Row;

            Button showBtn = new Button(() => SelectLocation(loc));
            showBtn.text = "Show";
            StyleListActionButton(showBtn, 48, new Color(0.2f, 0.25f, 0.35f));
            btnGroup.Add(showBtn);

            Button teleportBtn = new Button(() => TeleportToLocation(loc));
            teleportBtn.text = "Teleport";
            StyleListActionButton(teleportBtn, 64, new Color(0.35f, 0.18f, 0.12f));
            btnGroup.Add(teleportBtn);

            row2.Add(btnGroup);
            card.Add(row2);

            _locationsScrollView.Add(card);
        }
    }

    private void StyleListActionButton(Button btn, float width, Color bg)
    {
        btn.style.width = width;
        btn.style.height = 18;
        btn.style.marginLeft = 4;
        btn.style.fontSize = 9;
        btn.style.color = Color.white;
        btn.style.backgroundColor = bg;
        btn.style.borderTopWidth = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftWidth = 1;
        btn.style.borderRightWidth = 1;
        btn.style.borderTopColor = new Color(0.6f, 0.5f, 0.3f);
        btn.style.borderBottomColor = new Color(0.2f, 0.15f, 0.1f);
        btn.style.borderLeftColor = new Color(0.4f, 0.3f, 0.2f);
        btn.style.borderRightColor = new Color(0.4f, 0.3f, 0.2f);
    }

    public void SelectLocation(LocationPoint loc)
    {
        _selectedLocation = loc;

        // Focus and center map on selected location
        Vector2 norm = GetNormalizedMapPosition(loc.L2Pos.x, loc.L2Pos.y);
        float mapSize = 600f;
        Vector2 targetPinPos = new Vector2(norm.x * mapSize, norm.y * mapSize);
        _mapPanOffset = new Vector2(mapSize * 0.5f - targetPinPos.x * _zoomLevel, mapSize * 0.5f - targetPinPos.y * _zoomLevel);
        ApplyMapTransform();

        RefreshLocationList();
    }

    public void TeleportToLocation(LocationPoint loc)
    {
        if (loc == null || World.Instance == null) return;

        AudioManager.Instance?.PlayUISound("ItemSound.quest_middle");

        Vector3 targetUnityPos = loc.UnityPos;
        World.Instance.SetPlayerGroundAnchor(targetUnityPos);
        targetUnityPos.y = World.Instance.GetGroundHeight(targetUnityPos);

        if (PlayerTransformReceive.Instance != null)
        {
            PlayerTransformReceive.Instance.SetNewPosition(targetUnityPos);
        }

        UpdatePlayerPin();
        CenterOnPlayer();
    }

    private List<LocationPoint> GetFilteredLocations()
    {
        IReadOnlyList<LocationPoint> all = LocationTable.Instance != null ? LocationTable.Instance.AllLocations : System.Array.Empty<LocationPoint>();
        List<LocationPoint> result = new List<LocationPoint>();

        for (int i = 0; i < all.Count; i++)
        {
            LocationPoint pt = all[i];
            if (_selectedCategoryId > 0 && (int)pt.Category != _selectedCategoryId)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                if (!pt.Name.ToLowerInvariant().Contains(_searchFilter) &&
                    !pt.CategoryTitle.ToLowerInvariant().Contains(_searchFilter))
                {
                    continue;
                }
            }

            result.Add(pt);
        }

        return result;
    }
}
