using UnityEngine;

public class DemoBootstraper : MonoBehaviour
{
    [SerializeField] private DemoBootstrapMenu _demoBootstrapMenu;

    private void Start()
    {
        _demoBootstrapMenu.Interact();
    }
}
