using UnityEngine;
public class BookReceiver : SlotReceiver<Book>
{
    [SerializeField] private MeshRenderer _renderer;
    private bool _isActive;

    private void Awake()
    {
        Show(false);
    }

    protected override void OnObjectInserted(Book book)
    {
        book.SetCurrentSlot(this);
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