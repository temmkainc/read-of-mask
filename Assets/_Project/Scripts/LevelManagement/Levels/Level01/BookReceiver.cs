using UnityEngine;
using Cysharp.Threading.Tasks;
public class BookReceiver : SlotReceiver<Book>
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private VoicelineData _voicelineData;
    private bool _isActive;
    private bool _hasReceivedBookOnce;

    private void Awake()
    {
        Show(false);
    }

    protected override void OnObjectInserted(Book book)
    {
        book.SetCurrentSlot(this);
        if (!_hasReceivedBookOnce)
        {
            AudioManager.Instance.PlayVoicelineAsync(_voicelineData.Id, _audioSource).Forget();
            _hasReceivedBookOnce = true;
        }
    }

    public override bool CanHighlight(PlayerGrabbing grabbing)
    {
        return base.CanHighlight(grabbing) && _isActive;
    }

    public override void Show(bool active)
    {
        _renderer.enabled = active;
        _isActive = active;
    }
}