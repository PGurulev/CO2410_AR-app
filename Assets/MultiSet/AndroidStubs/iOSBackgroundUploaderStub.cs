#if !UNITY_IOS
using System.Collections;
using UnityEngine;

public class iOSBackgroundUploader : MonoBehaviour
{
    void Awake() { }

    public void StartBackgroundUpload(string url, string filePath)
    {
        Debug.LogWarning("[iOSBackgroundUploader] Called on non-iOS platform. No-op.");
    }

    public float GetUploadProgress()
    {
        return 0f;
    }

    public void CancelUpload()
    {
        Debug.LogWarning("[iOSBackgroundUploader] Called on non-iOS platform. No-op.");
    }

    public void CancelUploadProcess()
    {
        Debug.LogWarning("[iOSBackgroundUploader] Called on non-iOS platform. No-op.");
    }

    public void SetupCompletionCallback(System.Action callback)
    {
        Debug.LogWarning("[iOSBackgroundUploader] Called on non-iOS platform. No-op.");
    }

    private IEnumerator MonitorProgress() { yield break; }
}
#endif