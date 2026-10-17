using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen dynamic virtual joystick for Lineage 2 Mobile (iOS & Android).
/// Supports floating origin (appears where user places thumb on left half of screen).
/// </summary>
public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Settings")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;
    [SerializeField] private float _handleRange = 60f;
    [SerializeField] private bool _isFloating = true;

    private Canvas _canvas;
    private Camera _cam;
    private Vector2 _inputVector = Vector2.zero;
    private Vector2 _defaultPosition;

    public Vector2 Direction => _inputVector;
    public float Horizontal => _inputVector.x;
    public float Vertical => _inputVector.y;
    public bool IsPressed { get; private set; }

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            _cam = _canvas.worldCamera;
        }

        if (_background == null)
            _background = GetComponent<RectTransform>();

        if (_handle == null && _background.childCount > 0)
            _handle = _background.GetChild(0) as RectTransform;

        _defaultPosition = _background.anchoredPosition;

        if (_isFloating)
        {
            _background.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;

        if (_isFloating)
        {
            _background.gameObject.SetActive(true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                _cam,
                out Vector2 localPoint
            );
            _background.anchoredPosition = localPoint;
        }

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background,
            eventData.position,
            _cam,
            out Vector2 pos))
        {
            pos.x = (pos.x / _background.sizeDelta.x);
            pos.y = (pos.y / _background.sizeDelta.y);

            _inputVector = new Vector2(pos.x * 2f, pos.y * 2f);
            _inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized : _inputVector;

            if (_handle != null)
            {
                _handle.anchoredPosition = new Vector2(
                    _inputVector.x * _handleRange,
                    _inputVector.y * _handleRange
                );
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
        _inputVector = Vector2.zero;

        if (_handle != null)
        {
            _handle.anchoredPosition = Vector2.zero;
        }

        if (_isFloating)
        {
            _background.anchoredPosition = _defaultPosition;
            _background.gameObject.SetActive(false);
        }
    }
}
