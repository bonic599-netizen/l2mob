using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Touch-friendly Action & Combat bar for Lineage 2 Mobile client.
/// Provides large touch targets for combat, targeting, skills, and menu navigation.
/// </summary>
public class MobileActionBar : MonoBehaviour
{
    public static MobileActionBar Instance { get; private set; }

    [Header("Main Combat Buttons")]
    [SerializeField] private Button _attackButton;
    [SerializeField] private Button _nextTargetButton;
    [SerializeField] private Button _pickupButton;
    [SerializeField] private Button _sitStandButton;

    [Header("Quick Skills")]
    [SerializeField] private Button[] _quickSkillButtons; // Slots 1-8

    [Header("UI Drawers")]
    [SerializeField] private Button _inventoryButton;
    [SerializeField] private Button _characterStatusButton;
    [SerializeField] private Button _chatToggleButton;
    [SerializeField] private Button _menuButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        // Bind combat buttons
        if (_attackButton != null)
            _attackButton.onClick.AddListener(OnAttackClicked);

        if (_nextTargetButton != null)
            _nextTargetButton.onClick.AddListener(OnNextTargetClicked);

        if (_pickupButton != null)
            _pickupButton.onClick.AddListener(OnPickupClicked);

        if (_sitStandButton != null)
            _sitStandButton.onClick.AddListener(OnSitStandClicked);

        // Bind quick skill slots (maps to F1-F8 in shortcut system)
        if (_quickSkillButtons != null)
        {
            for (int i = 0; i < _quickSkillButtons.Length; i++)
            {
                int slotIndex = i;
                _quickSkillButtons[i].onClick.AddListener(() => OnSkillSlotClicked(slotIndex));
            }
        }

        // Bind UI navigation buttons
        if (_inventoryButton != null)
            _inventoryButton.onClick.AddListener(ToggleInventory);

        if (_characterStatusButton != null)
            _characterStatusButton.onClick.AddListener(ToggleCharacterStatus);

        if (_chatToggleButton != null)
            _chatToggleButton.onClick.AddListener(ToggleChat);

        if (_menuButton != null)
            _menuButton.onClick.AddListener(ToggleSystemMenu);
    }

    public void OnAttackClicked()
    {
        if (PlayerActions.Instance != null)
        {
            PlayerActions.Instance.UseAction(ActionType.Attack);
        }
    }

    public void OnNextTargetClicked()
    {
        if (PlayerActions.Instance != null)
        {
            PlayerActions.Instance.UseAction(ActionType.NextTarget);
        }
        else if (TargetManager.Instance != null)
        {
            TargetManager.Instance.NextTarget();
        }
    }

    public void OnPickupClicked()
    {
        if (PlayerActions.Instance != null)
        {
            PlayerActions.Instance.UseAction(ActionType.Pickup);
        }
    }

    public void OnSitStandClicked()
    {
        if (PlayerActions.Instance != null)
        {
            PlayerActions.Instance.UseAction(ActionType.Sit);
        }
    }

    public void OnSkillSlotClicked(int slot)
    {
        if (PlayerShortcuts.Instance != null)
        {
            var shortcut = PlayerShortcuts.Instance.GetShortcutBySlot(slot);
            if (shortcut != null)
            {
                PlayerShortcuts.Instance.UseShortcut(shortcut);
            }
        }
    }

    public void ToggleInventory()
    {
        if (InventoryWindow.Instance != null)
            InventoryWindow.Instance.ToggleHideWindow();
    }

    public void ToggleCharacterStatus()
    {
        if (CharacterInfoWindow.Instance != null)
            CharacterInfoWindow.Instance.ToggleHideWindow();
    }

    public void ToggleChat()
    {
        if (ChatWindow.Instance != null)
            ChatWindow.Instance.ToggleHideWindow();
    }

    public void ToggleSystemMenu()
    {
        if (SystemMenuWindow.Instance != null)
            SystemMenuWindow.Instance.ToggleHideWindow();
    }
}
