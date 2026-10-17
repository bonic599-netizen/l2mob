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
        // Select next monster
        if (World.Instance != null && PlayerStateMachine.Instance != null)
        {
            Vector3 myPos = PlayerStateMachine.Instance.transform.position;
            float closestDist = float.MaxValue;
            ObjectData closestMob = null;

            // Find closest monster within 1200 range
            if (World.Instance.Monsters != null)
            {
                foreach (var monster in World.Instance.Monsters)
                {
                    if (monster == null || monster.Entity.Status.IsDead)
                        continue;

                    float dist = Vector3.Distance(myPos, monster.ObjectTransform.position);
                    if (dist < 1200f && dist < closestDist)
                    {
                        closestDist = dist;
                        closestMob = monster;
                    }
                }
            }

            if (closestMob != null && TargetManager.Instance != null)
            {
                TargetManager.Instance.SetTarget(closestMob);
            }
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
            PlayerActions.Instance.UseAction(ActionType.SitStand);
        }
    }

    public void OnSkillSlotClicked(int slot)
    {
        if (PlayerShortcuts.Instance != null)
        {
            // Execute shortcut from page 0, slot
            PlayerShortcuts.Instance.UseShortcut(0, slot);
        }
    }

    public void ToggleInventory()
    {
        if (InventoryWindow.Instance != null)
            InventoryWindow.Instance.ToggleVisibility();
    }

    public void ToggleCharacterStatus()
    {
        if (CharacterStatusWindow.Instance != null)
            CharacterStatusWindow.Instance.ToggleVisibility();
    }

    public void ToggleChat()
    {
        if (ChatWindow.Instance != null)
            ChatWindow.Instance.ToggleVisibility();
    }

    public void ToggleSystemMenu()
    {
        if (SystemMenuWindow.Instance != null)
            SystemMenuWindow.Instance.ToggleVisibility();
    }
}
