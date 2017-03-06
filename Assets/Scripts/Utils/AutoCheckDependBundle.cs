using UnityEngine;
using System.Collections;

/**
 * anthor Nash
 * 
 */
public class AutoCheckDependBundle : MonoBehaviour
{
    public string url = "";

    public void OnDestroy()
    {
        if (Main.Instance != null && Main.Instance.gameObject != null)
        {
            DownloadManager.Instance.TryDisposeDependBundle(url);
        }
    }
}
