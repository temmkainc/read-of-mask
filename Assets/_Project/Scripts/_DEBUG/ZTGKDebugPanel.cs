using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugPanel : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F12))
            StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        AudioManager.Instance.StopAllCoroutines();
        AudioManager.Instance.StopMusic();
        yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}