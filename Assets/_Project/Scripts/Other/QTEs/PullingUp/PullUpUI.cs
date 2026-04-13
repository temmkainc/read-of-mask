using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PullUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PullUpController _controller;
    [SerializeField] private GameObject _root;

    [Header("Bar")]
    [SerializeField] private RectTransform _barBackground;
    [SerializeField] private RectTransform _windowRect;
    [SerializeField] private RectTransform _pointerRect; 

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _repCounterText;
    [SerializeField] private Image _flashImage;
    [SerializeField] private Color _hitColor = new Color(1f, 0.85f, 0.2f, 0.25f);
    [SerializeField] private Color _missColor = new Color(1f, 0.2f, 0.2f, 0.25f);

    private float _flashTimer;
    private const float FLASH_DURATION = 0.18f;
    private void Awake()
    {
        _windowRect.pivot = new Vector2(0f, 0.5f);
        _pointerRect.pivot = new Vector2(0.5f, 0.5f); 
    }
    private void Update()
    {
        if (!_root.activeSelf) return;

        var challenge = _controller.Challenge;
        float barWidth = _barBackground.rect.width;

        float windowX = Mathf.Lerp(0f, barWidth, challenge.WindowStart);
        float windowWidth = (challenge.WindowEnd - challenge.WindowStart) * barWidth;
        _windowRect.anchoredPosition = new Vector2(windowX, 0f);
        _windowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, windowWidth);

        float pointerX = Mathf.Lerp(0f, barWidth, challenge.PointerT);
        _pointerRect.anchoredPosition = new Vector2(pointerX, 0f);

        _repCounterText.text = _controller.RepCount.ToString();

        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_flashTimer / FLASH_DURATION);
            var c = _flashImage.color;
            _flashImage.color = new Color(c.r, c.g, c.b, alpha * 0.25f);
        }
    }
    public void Show() => _root.SetActive(true);
    public void Hide() => _root.SetActive(false);

    public void UpdateRepCount(int count)
    {
        _repCounterText.text = count.ToString();
    }

    public void ShowHitFlash()
    {
        _flashImage.color = _hitColor;
        _flashTimer = FLASH_DURATION;
    }

    public void ShowMissFlash()
    {
        _flashImage.color = _missColor;
        _flashTimer = FLASH_DURATION;
    }
}