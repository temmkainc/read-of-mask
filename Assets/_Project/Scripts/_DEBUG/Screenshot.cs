using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    [SerializeField] private string _path;
    [SerializeField] [Range(1,5)] private int size = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            _path += "screenshot ";
            _path += System.Guid.NewGuid().ToString() + ".png";
            ScreenCapture.CaptureScreenshot(_path, size);
        }
    }
}
